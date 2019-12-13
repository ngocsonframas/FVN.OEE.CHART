namespace MSharp.Framework
{
    using Data;
    using MSharp.Framework.Services;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Web;

    /// <summary> 
    /// Provides an utility for handling Binary property types.
    /// </summary>
    [Serializable, JsonConverter(typeof(PessimisticJsonConverter))]
    public class Document : IComparable<Document>, IComparable
    {
        /// <summary>
        /// In Test projects particularly, having files save themselves on the disk can waste space.
        /// To prevent that, apply this setting in the config file.
        /// </summary>
        static bool ShouldSuppressPersistence = Config.Get("Document.Suppress.Persistence", defaultValue: false);

        static bool AreDocumentsStoredWithFileName = Config.Get("Document.Stored.With.File.Name", defaultValue: false);

        const int IMAGE_DEFAULT_QUALITY = 70;
        const string EMPTY_FILE = "NoFile.Empty";
        public const string DefaultEncryptionKey = "Default_ENC_Key:_This_Better_Be_Calculated_If_Possible";
        public static string SecureVirtualRoot = "/Download.File.aspx?";

        static string[] UnsafeExtensions = new[] { "aspx", "ascx", "ashx", "axd", "master", "bat", "bas", "asp", "app", "bin","cla","class", "cmd", "com","sitemap","skin", "asa", "cshtml",
            "cpl","crt","csc","dll","drv","exe","hta","htm","html", "ini", "ins","js","jse","lnk","mdb","mde","mht","mhtm","mhtml","msc", "msi","msp", "mdb", "ldb","resources", "resx",
        "mst","obj", "config","ocx","pgm","pif","scr","sct","shb","shs", "smm", "sys","url","vb","vbe","vbs","vxd","wsc","wsf","wsh" , "php", "asmx", "cs", "jsl", "asax","mdf",
        "cdx","idc", "shtm", "shtml", "stm", "browser"};

        public static ConcurrentDictionary<AccessMode, string> PhysicalFilesRoots = new ConcurrentDictionary<AccessMode, string>();

        Entity ownerEntity;
        public AccessMode FileAccessMode;
        bool IsEmptyDocument;
        //byte[] fileData;
        byte[] CachedFileData;
        protected byte[] NewFileData;

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        public Document() { }

        /// <summary>
        /// Initializes a new Document instance with the specified data and file name.
        /// </summary>
        public Document(byte[] data, string fileName)
        {
            NewFileData = data;
            this.fileName = fileName.ToSafeFileName();
        }

        /// <summary>
        /// Initializes a new Document instance with the specified file name.
        /// </summary>
        public Document(string fileName) : this(null, fileName) { }

        /// <summary>
        /// Initializes a new Document instance, for the specified file on disk.
        /// </summary>
        public Document(FileInfo file) : this(File.ReadAllBytes(file.FullName), file.Name) { }

        ///// <summary>
        ///// Initializes a new Document instance, for the specified posted file.
        ///// </summary>
        //public Document(HttpPostedFile file)
        //{
        //    if (file == null) throw new ArgumentNullException(nameof(file));

        //    fileName = Path.GetFileName(file.FileName);

        //    NewFileData = new byte[file.InputStream.Length];
        //    file.InputStream.Position = 0;
        //    file.InputStream.Read(NewFileData, 0, NewFileData.Length);
        //}

        internal Entity Owner => ownerEntity;

        /// <summary>
        /// Gets the address of the property owning this document in the format: Type/ID/Property
        /// </summary>
        public string GetOwnerPropertyReference()
        {
            if (ownerEntity == null || OwnerProperty.IsEmpty()) return null;
            return $"{ownerEntity?.GetType().FullName}/{ownerEntity?.GetId()}/{OwnerProperty}";
        }

        public enum AccessMode { Open, Secure }

        public string OwnerProperty { get; private set; }

        IDocumentStorageProvider GetStorageProvider()
        {
            return DocumentStorageProviderFactory.GetProvider(FolderName);
        }

        #region FileName Property
        string fileName;
        [JsonExposed]
        public string FileName
        {
            get { return fileName.Or(EMPTY_FILE); }
            set { fileName = value; }
        }
        #endregion

        #region FileExtension Property
        public string FileExtension
        {
            get
            {
                if (fileName.IsEmpty()) return string.Empty;
                else
                {
                    var result = Path.GetExtension(fileName) ?? string.Empty;
                    if (result.Length > 0 && !result.StartsWith("."))
                        result = "." + result;
                    return result;
                }
            }
        }
        #endregion

        #region Image Optimization

        /// <summary>
        /// Optimizes the image based on the settings in the arguments.
        /// </summary>
        public void OptimizeImage(int maxWidth, int maxHeight)
        {
            OptimizeImage(maxWidth, maxHeight, IMAGE_DEFAULT_QUALITY);
        }

        /// <summary>
        /// Optimizes the image based on the settings in the arguments.
        /// </summary>
        public void OptimizeImage(int maxWidth, int maxHeight, int quality, bool toJpeg = true)
        {
            if (NewFileData != null && NewFileData.Length > 10)
            {
                var optimizer = new ImageOptimizer(maxWidth, maxHeight, quality);
                NewFileData = optimizer.Optimize(NewFileData, toJpeg);
            }
        }

        #endregion

        /// <summary>
        /// Gets an empty document object.
        /// </summary>
        public static Document Empty() => new Document(null, EMPTY_FILE) { IsEmptyDocument = true };

        /// <summary>
        /// Gets or sets the data of this document.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public virtual byte[] FileData
        {
            get
            {
                if (IsEmpty()) return new byte[0];

                if (NewFileData.HasAny()) return NewFileData;

                return CachedFileData = GetStorageProvider().Load(this);
            }
        }

        /// <summary>
        /// Gets all fall-back paths for this Document
        /// </summary>
        public IEnumerable<string> FallbackPaths
        {
            get
            {
                return (ownerEntity as IPickyDocumentContainer)?.GetFallbackPaths(this)
                       ?? Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the Url of this document.
        /// </summary>
        public override string ToString() => Url();

        #region Textual Content
        /// <summary>
        /// Gets the content
        /// </summary>
        /// <returns></returns>
        public string GetContentText()
        {
            if (IsEmpty()) return string.Empty;

            try
            {
                using (var mem = new MemoryStream(FileData))
                {
                    using (var reader = new StreamReader(mem))
                        return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("The {0} of the {1} entity ({2}) cannot be converted to text."
                    .FormatWith(OwnerProperty, ownerEntity?.GetType().FullName, ownerEntity?.GetId()), ex);
            }
        }
        #endregion

        /// <summary>
        /// Gets a Url to this document.
        /// </summary>
        public string Url(AccessMode mode)
        {
            if (ownerEntity == null) return null;

            if (ownerEntity is IPickyDocumentUrlContainer picky)
                return picky.GetUrl(this);

            var result = GetVirtualFolderUrl(mode) + GetFileNameWithoutExtension() + FileExtension;
            if (mode == AccessMode.Open && result.Lacks("?"))
            {
                result += "?" + FileName.Replace(":", " ").Replace("/", " ").Replace("\\", " ")
                          .KeepReplacing("  ", " ").UrlEncode();
            }

            return result;
        }

        /// <summary>
        /// Gets a Url to this document.
        /// </summary>
        public string Url() => Url(FileAccessMode);

        /// <summary>
        /// Returns the Url of this document, or the provided default Url if this is Empty.
        /// </summary>
        public string UrlOr(string defaultUrl)
        {
            if (IsEmpty()) return defaultUrl;
            else return Url();
        }

        /// <summary>
        /// Gets a cache safe URL to this document.
        /// </summary>
        public string GetCacheSafeUrl()
        {
            var result = Url();

            if (result.IsEmpty()) return result;

            return result + (result.Contains("?") ? "&" : "?") + "RANDOM=" + Guid.NewGuid();
        }

        public string GetVirtualFolderUrl(AccessMode accessMode)
        {
            if (ownerEntity is IPickyDocumentContainer)
            {
                var result = (ownerEntity as IPickyDocumentContainer).GetVirtualFolderPath(this);
                if (result.HasValue()) return result.TrimEnd('/') + "/";
            }

            var root = Config.Get("UploadFolder.VirtualRoot").Or("/App_Documents/");

            if (accessMode == AccessMode.Secure) root = Config.Get("UploadFolder.VirtualRoot.Secure").Or(SecureVirtualRoot);

            return root + FolderName + "/";
        }

        string folderName;
        public string FolderName
        {
            get
            {
                if (folderName == null)
                {
                    if (ownerEntity == null) return OwnerProperty;
                    folderName = ownerEntity.GetType().Name + "." + OwnerProperty;
                }

                return folderName;
            }
            set
            {
                folderName = value;
            }
        }

        bool hasValue; // For performance, cache it

        /// <summary>
        /// Determines whether this is an empty document.
        /// </summary>
        public bool IsEmpty()
        {
            if (hasValue) return false;

            if (IsEmptyDocument) return true;

            if (FileName == EMPTY_FILE) return true;

            if (NewFileData.HasAny()) return false;

            if (GetStorageProvider() is IRemoteDocumentStorageProvider remote
                && remote.CostsToCheckExistence())
            {
                // Rely on file name. 
                hasValue = true;
                return false;
            }

            if (GetStorageProvider().FileExists(this))
            {
                hasValue = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this document has any content.
        /// </summary>
        public bool HasValue() => !IsEmpty();

        /// <summary>
        /// Creates a clone of this document.
        /// </summary>
        public Document Clone() => Clone(attach: false, @readonly: false);

        public Document Clone(bool attach, bool @readonly)
        {
            if (!attach && @readonly) throw new ArgumentException("readonly can be set to true only when attaching.");

            Document result;

            if (ownerEntity != null)
            {
                if (NewFileData.HasAny()) result = new Document(NewFileData, FileName);
                else result = new ClonedDocument(this);

                if (attach)
                {
                    if (!@readonly) Attach(ownerEntity, OwnerProperty, FileAccessMode);
                    else
                    {
                        result.ownerEntity = ownerEntity;
                        result.OwnerProperty = OwnerProperty;
                        result.FileAccessMode = FileAccessMode;
                    }
                }
            }
            else if (NewFileData.HasAny()) result = new Document(NewFileData, FileName);
            else result = new Document(FileName);

            return result;
        }

        /// <summary>
        /// Attaches this Document to a specific record's file property.
        /// </summary>
        public Document Attach(Entity owner, string propertyName)
        {
            return Attach(owner, propertyName, AccessMode.Open);
        }

        /// <summary>
        /// Attaches this Document to a specific record's file property.
        /// </summary>
        public Document Attach(Entity owner, string propertyName, AccessMode accessMode)
        {
            ownerEntity = owner;
            OwnerProperty = propertyName;
            FileAccessMode = accessMode;

            if (owner is GuidEntity) owner.Saving += Owner_Saving;
            else owner.Saved += Owner_Saved;

            owner.Deleting += Delete;
            return this;
        }

        /// <summary>
        /// Detaches this Document.
        /// </summary>
        public void Detach()
        {
            if (ownerEntity == null) return;

            ownerEntity.Saving -= Owner_Saving;
            ownerEntity.Saved -= Owner_Saved;
            ownerEntity.Deleting -= Delete;
        }

        /// <summary>
        /// Deletes this document from the disk.
        /// </summary>
        void Delete(object sender, EventArgs e)
        {
            if (ShouldSuppressPersistence) return;

            if (ownerEntity.GetType().Defines<SoftDeleteAttribute>()) return;

            DeleteFromDisk();
        }

        void DeleteFromDisk()
        {
            if (ownerEntity == null) throw new InvalidOperationException();

            GetStorageProvider().Delete(this);

            CachedFileData = NewFileData = null;
        }

        void Owner_Saving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ShouldSuppressPersistence) SaveOnDisk();
        }

        void Owner_Saved(object sender, SaveEventArgs e)
        {
            if (!ShouldSuppressPersistence) SaveOnDisk();
        }

        /// <summary>
        /// Saves this file on the disk.
        /// </summary>
        public virtual void SaveOnDisk()
        {
            if (NewFileData.HasAny()) GetStorageProvider().Save(this);
            else if (IsEmptyDocument && (ownerEntity == null || !ownerEntity.IsNew)) DeleteFromDisk();
        }

        //class ShallowCopiedDocument : Document
        //{
        //    public override void SaveOnDisk()
        //    {
        //        if (fileData == null)
        //            base.SaveOnDisk();
        //    }


        //    public override byte[] FileData => base.FileData;
        //}

        /// <summary>
        /// Gets the mime type based on the file extension.
        /// </summary>
        public string GetMimeType()
        {
            // The document may be in-memory.
            return $"c:\\{FileName}".AsFile().GetMimeType();
        }

        /// <summary>Determines if this document's file extension is for audio or video.</summary>
        public bool IsMedia() => GetMimeType().StartsWithAny("audio/", "video/");

        string GetFilesRoot() => GetPhysicalFilesRoot(FileAccessMode).FullName;

        /// <summary>
        /// Gets the physical path root.
        /// </summary>
        public static DirectoryInfo GetPhysicalFilesRoot(AccessMode accessMode)
        {
            var result = PhysicalFilesRoots.GetOrAdd(accessMode,
                m =>
                {
                    var folderConfigKey = accessMode == AccessMode.Secure ? "UploadFolder.Secure" : "UploadFolder";
                    var defaultFolder = accessMode == AccessMode.Secure ? "App_Data\\" : "App_Documents\\";

                    var folder = Config.Get(folderConfigKey).Or(defaultFolder).TrimEnd('\\') + "\\";

                    if (!folder.StartsWith("\\\\") && folder[1] != ':') // Relative address:
                        folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);

                    return folder;
                });

            return new DirectoryInfo(result);
        }

        internal string LocalFolder
        {
            get
            {
                if (ownerEntity == null) return null;

                if (ownerEntity is IPickyDocumentContainer)
                {
                    var result = (ownerEntity as IPickyDocumentContainer).GetPhysicalFolderPath(this);
                    if (result.HasValue()) return result;
                }

                var docsFolder = Path.Combine(GetFilesRoot(), FolderName + "\\");

                return docsFolder;
            }
        }

        /// <summary>
        ///  This will return the document object linked to the correct entity.
        /// </summary>
        /// <param name="reference">Expected format: Type/Id/Property.</param>
        public static Document FromReference(string reference)
        {
            var parts = reference.OrEmpty().Split('/');
            if (parts.Length != 3) throw new ArgumentException("Expected format is Type/ID/Property.");

            var type = EntityFinder.GetEntityType(parts.First());

            if (type == null)
                throw new ArgumentException($"The type '{parts.First()}' is not found in the currently loaded assemblies.");

            var id = parts[1];
            var propertyName = parts.Last();

            var entity = Database.GetOrDefault(id, type);
            if (entity == null)
                throw new ArgumentException($"Could not load an instance of '{parts.First()}' with the ID of '{id} from the database."); ;

            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new Exception($"The type {type.FullName} does not have a property named {propertyName}.");

            return property.GetValue(entity) as Document;
        }

        /// <summary>
        /// Gets the local physical path of this file.
        /// </summary>
        public string LocalPath
        {
            get
            {
                if (ownerEntity == null) return null;

                var result = Path.Combine(LocalFolder, GetFileNameWithoutExtension() + FileExtension);

                if (!Directory.Exists(LocalFolder)) Directory.CreateDirectory(LocalFolder);

                return result;
            }
        }

        public string GetFileNameWithoutExtension()
        {
            if (ownerEntity == null) return null;
            if (ownerEntity is IntEntity && ownerEntity.IsNew) return null;

            if (ownerEntity is IPickyDocumentContainer)
            {
                var result = (ownerEntity as IPickyDocumentContainer).GetFileNameWithoutExtension(this);

                if (result.HasValue()) return result;
            }

            if (AreDocumentsStoredWithFileName)
                return FileName.TrimEnd(FileExtension);

            return ownerEntity?.GetId().ToStringOrEmpty();
        }

        #region Unsafe Files Handling

        /// <summary>
        /// Gets a list of unsafe file extensions.
        /// </summary>
        public static string[] GetUnsafeExtensions() => UnsafeExtensions;

        /// <summary>
        /// Determines whether the extension of this file is potentially unsafe.
        /// </summary>
        public bool HasUnsafeExtension() => HasUnsafeFileExtension(FileName);

        public static bool HasUnsafeFileExtension(string fileName)
        {
            if (fileName.IsEmpty()) return false;

            var extension = Path.GetExtension(fileName.Trim().TrimEnd('.', '\\', '/'))
                .OrEmpty().Where(x => x.IsLetter()).ToArray().ToString("").ToLower();

            return UnsafeExtensions.Contains(extension);
        }

        #endregion

        public override bool Equals(object obj)
        {
            var otherDocument = obj as Document;

            if (otherDocument == null) return false;
            else if (object.ReferenceEquals(this, otherDocument)) return true;
            else if (IsEmpty() && otherDocument.IsEmpty()) return true;

            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(Document left, Document right)
        {
            if (ReferenceEquals(left, right)) return true;
            else if (ReferenceEquals(left, null)) return false;
            else return left.Equals(right);
        }

        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FileName);

        public static bool operator !=(Document left, Document right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets this document if it has a value, otherwise another specified document.
        /// </summary>
        public Document Or(Document other)
        {
            if (IsEmpty()) return other;
            else return this;
        }

        #region IComparable<Document> Members

        /// <summary>
        /// Compares this document versus a specified other document.
        /// </summary>
        public int CompareTo(Document other)
        {
            if (other == null) return 1;

            if (IsEmpty())
            {
                if (other.IsEmpty()) return 0;
                else return -1;
            }
            else if (other.IsEmpty()) return 1;
            else
            {
                var me = FileData.Length;
                var him = other.FileData.Length;
                if (me == him) return 0;
                if (me > him) return 1;
                else return -1;
            }
        }

        /// <summary>
        /// Compares this document versus a specified other document.
        /// </summary>
        public int CompareTo(object obj) => CompareTo(obj as Document);

        #endregion
    }
}