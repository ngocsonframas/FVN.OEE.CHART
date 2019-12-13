using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Transactions;
using MSharp.Framework.Data;

namespace MSharp.Framework
{
    /// <summary>
    /// Data access code for Application components.
    /// </summary>
    public static partial class Database
    {
        static bool IsSet(SaveBehaviour setting, SaveBehaviour behaviour) => (setting & behaviour) == behaviour;

        static bool IsSet(DeleteBehaviour setting, DeleteBehaviour behaviour) => (setting & behaviour) == behaviour;

        static bool NeedsTypeResolution(Type type) => type.IsInterface || type == typeof(Entity);

        public static event EventHandler CacheRefreshed;
        /// <summary>
        /// Clears the cache of all items.
        /// </summary>
        public static void Refresh()
        {
            Cache.Current.ClearAll();

            CacheRefreshed?.Invoke(typeof(Database), EventArgs.Empty);
        }

        public static bool AnyOpenTransaction() => Transaction.Current != null || DbTransactionScope.Root != null;

        /// <summary>
        /// If there is an existing open transaction, it will simply run the specified action in it, Otherwise it will create a new transaction.
        /// </summary>
        public static void EnlistOrCreateTransaction(Action action)
        {
            if (AnyOpenTransaction())
            {
                action?.Invoke();
            }
            else
            {
                using (var scope = CreateTransactionScope())
                {
                    action?.Invoke();

                    scope.Complete();
                }
            }
        }
        /// <summary>
        /// Returns the first record of the specified type of which ToString() would return the specified text .
        /// </summary>
        public static T Parse<T>(string toString, bool caseSensitive = false) where T : IEntity
        {
            var comparison = caseSensitive ? StringComparison.InvariantCulture : StringComparison.OrdinalIgnoreCase;

            foreach (var instance in GetList<T>())
            {
                string objectString;
                try { objectString = instance.ToString(); }
                catch (Exception ex)
                {
                    throw new Exception("Database.Parse() failed. Calling ToString() throw an exception on the {0} object with ID of '{1}'"
                        .FormatWith(typeof(T).Name, instance.GetId(), ex));
                }

                if (toString == null && objectString == null) return instance;

                if (toString == null || objectString == null) continue;

                if (objectString.Equals(toString, comparison)) return instance;
            }

            return default(T);
        }

        /// <summary>
        /// Gets the total number of objects in cache.
        /// </summary>
        public static int CountAllObjectsInCache() => Cache.Current.CountAllObjects();

        public static IEnumerable<string> ReadManyToManyRelation(IEntity instance, string property)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            return GetProvider(instance).ReadManyToManyRelation(instance, property);
        }

        /// <summary>
        /// Reloads a specified instance from database. This should be used when an object in memory is not synscronized with the database.
        /// </summary>
        public static void Reload<T>(ref T instance) where T : IEntity
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            Cache.Current.Remove(instance);

            instance = (T)Get(instance.GetId(), instance.GetType());
        }

        /// <summary>
        /// Gets a reloaded instance from the database to get a synced copy.
        /// </summary>
        public static T Reload<T>(T instance) where T : IEntity
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            Cache.Current.Remove(instance);

            return (T)Get(instance.GetId(), instance.GetType());
        }

        /// <summary>
        /// Determines if there is any object in the database of the specified type.
        /// </summary>
        public static bool Any<T>() where T : IEntity => Find<T>() != null;

        /// <summary>
        /// Determines if there is any object in the database of the specified type matching a given criteria.
        /// </summary>
        public static bool Any<T>(Expression<Func<T, bool>> criteria) where T : IEntity => Find<T>(criteria) != null;

        /// <summary>
        /// Determines whether there is no object of the specified type in the database.
        /// </summary>
        public static bool None<T>() where T : IEntity => !Any<T>();

        /// <summary>
        /// Determines whether none of the objects in the database match a given criteria.
        /// </summary>
        public static bool None<T>(Expression<Func<T, bool>> criteria) where T : IEntity => !Any<T>(criteria);

        public static void TryCache(IEntity item, DateTime? queryTime)
        {
            if (AnyOpenTransaction()) return;
            if (queryTime != null && Cache.Current.IsUpdatedSince(item, queryTime.Value)) return;
            Cache.Current.Add(item);
        }
    }
}