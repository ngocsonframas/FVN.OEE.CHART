namespace MSharp.Framework.Services
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Provides email sending services.
    /// </summary>
    public static partial class EmailService
    {
        static IEmailServiceConfigurator Configurator;
        const string ALL_CATEGORIES = "*";
        static Type ConcreteEmailQueueItemType;
        static object SyncLock = new object();
        static Random Random = new Random();

        public static int MaximumRetries => Config.Get<int>("Email.Maximum.Retries", 4);

        /// <summary>
        /// Specifies a factory to instantiate EmailQueueItem objects.
        /// </summary>
        public static Func<IEmailQueueItem> EmailQueueItemFactory = CreateEmailQueueItem;

        /// <summary>
        /// Provides a message which can dispatch an email message.
        /// Returns whether the message was sent successfully.
        /// </summary>
        public static Func<IEmailQueueItem, MailMessage, bool> EmailDispatcher = SendViaSmtp;

        #region Events

        /// <summary>
        /// Occurs when the smtp mail message for this email is sent. Sender is the IEmailQueueItem instance that was sent.
        /// </summary>
        public static event EventHandler<EventArgs<MailMessage>> Sent;

        /// <summary>
        /// Occurs when the smtp mail message for this email is about to be sent.
        /// </summary>
        public static event EventHandler<EventArgs<MailMessage>> Sending;

        public static void Initialized(IEmailServiceConfigurator configurator) => Configurator = configurator;

        /// <summary>
        /// Occurs when an exception happens when sending an email. Sender parameter will be the IEmailQueueItem instance that couldn't be sent.
        /// </summary>
        public static event EventHandler<EventArgs<Exception>> SendError;
        static void OnSendError(IEmailQueueItem email, Exception error)
        {
            SendError?.Invoke(email, new EventArgs<Exception>(error));
        }

        /// <summary>
        /// Raises the Sending event.
        /// </summary>
        internal static void OnSending(IEmailQueueItem item, MailMessage message)
        {
            Sending?.Invoke(item, new EventArgs<MailMessage>(message));
        }

        /// <summary>
        /// Raises the Sent event.
        /// </summary>
        internal static void OnSent(IEmailQueueItem item, MailMessage message)
        {
            if (!item.IsNew) Database.Delete(item);

            Sent?.Invoke(item, new EventArgs<MailMessage>(message));
        }

        #endregion

        #region Factory

        static IEmailQueueItem CreateEmailQueueItem()
        {
            if (ConcreteEmailQueueItemType != null)
                return Activator.CreateInstance(ConcreteEmailQueueItemType) as IEmailQueueItem;

            var possible = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.References(typeof(IEmailQueueItem).Assembly))
                .SelectMany(a => { try { return a.GetExportedTypes(); } catch { return new Type[0]; /* No logging needed */ } })
                .Where(t => t.IsClass && !t.IsAbstract && t.Implements<IEmailQueueItem>()).ToList();

            if (possible.Count == 0)
            {
                throw new Exception("No type in the currently loaded assemblies implements IEmailQueueItem.");
            }

            if (possible.Count > 1)
            {
                throw new Exception("More than one type in the currently loaded assemblies implement IEmailQueueItem:" + possible.Select(x => x.FullName).ToString(" and "));
            }

            ConcreteEmailQueueItemType = possible.Single();
            return CreateEmailQueueItem();
        }

        #endregion

        static bool IsSendingPermitted(string to)
        {
            var permittedDomains = Config.Get("Email.Permitted.Domains").Or("geeks.ltd.uk|uat.co").ToLowerOrEmpty();
            if (permittedDomains == "*") return true;

            if (permittedDomains.Split('|').Trim().Any(d => to.TrimEnd(">").EndsWith("@" + d))) return true;

            var permittedAddresses = Config.Get("Email.Permitted.Addresses").ToLowerOrEmpty().Split('|').Trim();

            return permittedAddresses.Any() && new MailAddress(to).Address.IsAnyOf(permittedAddresses);
        }

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        public static void SendAll() => SendAll(ALL_CATEGORIES, TimeSpan.Zero);

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        /// <param name="category">The category of the emails to send. Use "*" to indicate "all emails".</param>
        public static void SendAll(string category) => SendAll(category, TimeSpan.Zero);

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        /// <param name="delay">The time to wait in between sending each outstanding email.</param>
        public static void SendAll(TimeSpan delay) => SendAll(ALL_CATEGORIES, delay);

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        /// <param name="category">The category of the emails to send. Use "*" to indicate "all emails".</param>
        public static void SendAll(string category, TimeSpan delay)
        {
            lock (SyncLock)
            {
                foreach (var mail in Database.GetList<IEmailQueueItem>().OrderBy(e => e.Date).ToArray())
                {
                    if (mail.Retries >= MaximumRetries) continue;

                    if (category != ALL_CATEGORIES)
                    {
                        if (category.IsEmpty() && mail.Category.HasValue()) continue;
                        if (category != mail.Category) continue;
                    }

                    if (delay > TimeSpan.Zero)
                    {
                        var multiply = 1 + (Random.NextDouble() - 0.5) / 4; // from 0.8 to 1.2

                        try
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(delay.TotalMilliseconds * multiply));
                        }
                        catch (ThreadAbortException)
                        {
                            // Application terminated.
                            return;
                        }
                    }

                    try { mail.Send(); }
                    catch (Exception ex)
                    {
                        Log.Error("Could not send a queued email item " + mail.GetId(), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Will try to send the specified email and returns true for successful sending.
        /// </summary>
        public static bool Send(IEmailQueueItem mailItem)
        {
            if (mailItem == null) throw new ArgumentNullException(nameof(mailItem));

            if (mailItem.Retries >= MaximumRetries) return false;

            try
            {
                using (var mail = CreateMailMessage(mailItem))
                {
                    if (mail == null) return false;
                    return EmailDispatcher(mailItem, mail);
                }
            }
            catch (Exception ex)
            {
                OnSendError(mailItem, ex);
                mailItem.RecordRetry();
                Log.Error("Error in sending an email for this EmailQueueItem of '" + mailItem.GetId() + "'", ex);
                return false;
            }
        }

        static bool SendViaSmtp(IEmailQueueItem mailItem, MailMessage mail)
        {
            // Developer note: Web.config setting for SSL is designed to take priority over the specific setting of the email.
            // If in your application you want the email item's setting to take priority, do this:
            //      1. Remove the 'Email.Enable.Ssl' setting from web.config totally.
            //      2. If you need a default value, use  your application's Global Settings object and use that value everywhere you create an EmailQueueItem.
            if (Configurator == null) throw new InvalidOperationException($"{nameof(EmailService)} is not initialized.");

            using (var smtpClient = new SmtpClient { EnableSsl = Config.Get<bool>("Email.Enable.Ssl", mailItem.EnableSsl) })
            {
                Configurator.Configure(smtpClient);

                if (mailItem.SmtpHost.HasValue())
                    smtpClient.Host = mailItem.SmtpHost;

                if (mailItem.SmtpPort.HasValue)
                    smtpClient.Port = mailItem.SmtpPort.Value;

                if (mailItem.Username.HasValue())
                    smtpClient.Credentials = new NetworkCredential(mailItem.Username, mailItem.Password.Or((smtpClient.Credentials as NetworkCredential).Get(c => c.Password)));

                if (Config.IsDefined("Email.Random.Usernames"))
                {
                    var userName = Config.Get("Email.Random.Usernames").Split(',').Trim().PickRandom();
                    smtpClient.Credentials = new NetworkCredential(userName, Config.Get("Email.Password"));
                }

                OnSending(mailItem, mail);

                smtpClient.Send(mail);

                OnSent(mailItem, mail);
            }

            return true;
        }

        /// <summary>
        /// Gets the email items which have been sent (marked as soft deleted).
        /// </summary>
        public static IEnumerable<T> GetSentEmails<T>() where T : IEmailQueueItem
        {
            using (new SoftDeleteAttribute.Context(bypassSoftdelete: false))
            {
                return Database.GetList<T>().Where(x => EntityManager.IsSoftDeleted((Entity)(IEntity)x));
            }
        }

        /// <summary>
        /// Creates an SMTP mail message for a specified mail item.
        /// </summary>
        static MailMessage CreateMailMessage(IEmailQueueItem mailItem)
        {
            // Make sure it's due:
            if (mailItem.Date > LocalTime.Now) return null;

            var mail = new MailMessage { Subject = mailItem.Subject.Or("[NO SUBJECT]").Remove("\r", "\n") };

            #region Set Body

            if (mailItem.Html)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(mailItem.Body, new ContentType("text/html; charset=UTF-8"));

                // Add Linked Resources
                htmlView.LinkedResources.AddRange<LinkedResource>(mailItem.GetLinkedResources());

                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mailItem.Body.RemoveHtmlTags(), new ContentType("text/plain; charset=UTF-8")));
                mail.AlternateViews.Add(htmlView);
            }
            else
            {
                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mailItem.Body.RemoveHtmlTags(), new ContentType("text/plain; charset=UTF-8")));
            }

            if (mailItem.VCalendarView.HasValue())
            {
                var calendarType = new ContentType("text/calendar");
                calendarType.Parameters.Add("method", "REQUEST");
                calendarType.Parameters.Add("name", "meeting.ics");

                var calendarView = AlternateView.CreateAlternateViewFromString(mailItem.VCalendarView, calendarType);
                calendarView.TransferEncoding = TransferEncoding.SevenBit;

                mail.AlternateViews.Add(calendarView);
            }

            #endregion

            mail.From = mailItem.GetSender();

            mail.ReplyToList.Add(mailItem.GetReplyTo());

            #region Set Receivers

            // Add To:
            foreach (var address in mailItem.To.Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.To.Add(address);

            // Add Cc:
            foreach (var address in mailItem.Cc.Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.CC.Add(address);

            foreach (var address in Config.Get("Email.Auto.CC.Address").Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.CC.Add(address);

            // Add Bcc:
            foreach (var address in mailItem.Bcc.Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.Bcc.Add(address);

            if (mail.To.None() && mail.CC.None() && mail.Bcc.None())
                return null;

            #endregion

            // Add attachments
            mail.Attachments.AddRange(mailItem.GetAttachments());

            return mail;
        }

        public static MailAddress GetSender(this IEmailQueueItem mailItem)
        {
            var addressPart = mailItem.SenderAddress.Or(Config.Get("Email.Sender.Address"));
            var displayNamePart = mailItem.SenderName.Or(Config.Get("Email.Sender.Name"));
            return new MailAddress(addressPart, displayNamePart);
        }

        public static MailAddress GetReplyTo(this IEmailQueueItem mailItem)
        {
            var result = mailItem.GetSender();

            var asCustomReplyTo = mailItem as ICustomReplyToEmailQueueItem;
            if (asCustomReplyTo == null) return result;

            return new MailAddress(asCustomReplyTo.ReplyToAddress.Or(result.Address),
                    asCustomReplyTo.ReplyToName.Or(result.DisplayName));
        }

        /// <summary>
        /// Creates a VCalendar text with the specified parameters.
        /// </summary>
        /// <param name="meetingUniqueIdentifier">This uniquely identifies the meeting and is used for changes / cancellations. It is recommended to use the ID of the owner object.</param>
        public static string CreateVCalendarView(string meetingUniqueIdentifier, DateTime start, DateTime end, string subject, string description, string location)
        {
            var dateFormat = "yyyyMMddTHHmmssZ";

            Func<string, string> cleanUp = s => s.Or("").Remove("\r").Replace("\n", "\\n");

            var r = new StringBuilder();
            r.AppendLine(@"BEGIN:VCALENDAR");
            r.AppendLine(@"PRODID:-//Microsoft Corporation//Outlook 12.0 MIMEDIR//EN");
            r.AppendLine(@"VERSION:1.0");
            r.AppendLine(@"BEGIN:VEVENT");

            r.AddFormattedLine(@"DTSTART:{0}", start.ToString(dateFormat));
            r.AddFormattedLine(@"DTEND:{0}", end.ToString(dateFormat));
            r.AddFormattedLine(@"UID:{0}", meetingUniqueIdentifier);
            r.AddFormattedLine(@"SUMMARY:{0}", cleanUp(subject));
            r.AppendLine("LOCATION:" + cleanUp(location));
            r.AppendLine("DESCRIPTION:" + cleanUp(description));

            // bodyCalendar.AppendLine(@"PRIORITY:3");
            r.AppendLine(@"END:VEVENT");
            r.AppendLine(@"END:VCALENDAR");

            return r.ToString();
        }
    }

    public interface IEmailServiceConfigurator
    {
        void Configure(SmtpClient client);
    }
}