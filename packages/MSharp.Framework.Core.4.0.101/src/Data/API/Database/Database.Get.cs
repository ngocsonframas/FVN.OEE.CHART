namespace MSharp.Framework
{
    using System;
    using System.Linq;
    using MSharp.Framework.Data;

    partial class Database
    {
        // Note: This feature can prevent a rare concurrency issue in highly concurrent applications.
        // But it comes at the cost of performance degradation. If your application doesn't have extremely concurrent processing
        // with multiple threads reading and updating records at the same time, you can disable it in web.config to improve performance.
        internal static bool IsCacheConcurrencyAware =
            Config.Get("Database.Concurrency.Aware.Cache", defaultValue: true);

        internal static IEntity GetConcrete(object entityID, Type concreteType)
        {
            var result = Cache.Current.Get(concreteType, entityID.ToString());
            if (result != null) return result;

            DateTime? timestamp = null;

            if (IsCacheConcurrencyAware) timestamp = DateTime.UtcNow;

            result = GetConcreteFromDatabase(entityID, concreteType);

            // Don't cache the result if it is fetched in a transaction.
            if (result != null)
            {
                if (!AnyOpenTransaction())
                {
                    // Cache globally:

                    if (IsCacheConcurrencyAware)
                    {
                        if (!Cache.Current.IsUpdatedSince(result, timestamp.Value))
                            Cache.Current.Add(result);
                    }
                    else
                    {
                        Cache.Current.Add(result);
                    }
                }
            }

            return result;
        }

        static IEntity GetConcreteFromDatabase(object entityID, Type concreteType)
        {
            if (SmallTableAttribute.IsEnabled(concreteType))
            {
                if (entityID is string && !concreteType.IsA<Entity<string>>())
                {
                    return GetList(concreteType).FirstOrDefault(x => x.GetId().ToString().Equals(entityID));
                }
                else
                {
                    return GetList(concreteType).FirstOrDefault(x => x.GetId().Equals(entityID));
                }
            }
            else
            {
                var result = GetProvider(concreteType).Get(entityID);

                if (result != null) EntityManager.RaiseOnLoaded(result as Entity);

                return result;
            }
        }

        /// <summary>
        /// Gets an Entity of the given type with the given Id from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>        
        /// <param name="entityId">The primary key value of the object to load in string format.</param>
        public static T Get<T>(string entityId) where T : IEntity
        {
            if (entityId.IsEmpty()) return default(T);

            return (T)Get((object)entityId, typeof(T));
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public static T Get<T>(Guid id) where T : IEntity
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Could not load the " + typeof(T).Name + " because the given objectID is empty.");

            return (T)Get(id, typeof(T));
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public static T Get<T>(Guid? id) where T : IEntity
        {
            if (id.HasValue) return Get<T>(id.Value);
            else return default(T);
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public static T Get<T>(int? id) where T : IEntity<int>
        {
            if (id == null) return default(T);
            return (T)Get((object)id.Value, typeof(T));
        }

        public static T Get<T>(int id) where T : IEntity<int> => (T)Get((object)id, typeof(T));

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <param name="entityID">The primary key value of the object to load.</param>
        public static IEntity<Guid> Get(Guid entityID, Type objectType)
        {
            return Get((object)entityID, objectType) as IEntity<Guid>;
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If it can't find the object, an exception will be thrown.
        /// </summary>
        /// <param name="entityID">The primary key value of the object to load.</param>
        public static IEntity Get(object entityID, Type objectType)
        {
            if (objectType == null) return null;

            // First try -> session memory:
            var result = SessionMemory.Get(objectType, entityID);

            if (result != null) return result;

            if (NeedsTypeResolution(objectType))
            {
                foreach (var provider in ResolveDataProviders(objectType))
                {
                    try
                    {
                        result = Cache.Current.Get(entityID.ToString());
                        if (result != null) return result;

                        result = provider.Get(entityID);
                        if (result != null) break;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else
            {
                result = GetConcrete(entityID, objectType);
            }

            if (result != null) return result;
            else
                throw new ArgumentException("Could not load the " + objectType.FullName + " instance with the ID of " + entityID + ".");
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the key does not exist, it will return null, rather than throwing an exception.
        /// </summary>
        /// <typeparam name="T">The type of the object to get</typeparam>
        /// <param name="id">The primary key value of the object to load.</param>
        public static T GetOrDefault<T>(object id) where T : IEntity
        {
            return (T)GetOrDefault(id, typeof(T));

            // if (id == null || id.Value == Guid.Empty) return default(T);

            // try { return Get<T>(id.Value); }
            // catch { return default(T); }
        }

        /// <summary>
        /// Get an entity with the given type and ID from the database.
        /// If the key does not exist, it will return null, rather than throwing an exception.
        /// </summary>
        /// <param name="type">The type of the object to get</param>
        /// <param name="id">The primary key value of the object to load.</param>        
        public static IEntity GetOrDefault(object id, Type type)
        {
            if (id.ToStringOrEmpty().IsEmpty()) return null;

            try { return Get(id, type); }
            catch { return null; }
        }
    }
}