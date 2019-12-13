﻿namespace MSharp.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Linq;
    using MSharp.Framework.Data;

    public class IntEntity : Entity<int>
    {
        bool IsIdLoaded;
        int id;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<Type, int> NewIdGenerator = DefaultNewIdGenerator;

        /// <summary>
        /// Gets a unique Identifier for this instance. In the database, this will be the primary key of this object.
        /// </summary>
        public override int ID
        {
            get
            {
                if (IsIdLoaded) return id;

                if (GetType().Defines<IdByDatabaseAttribute>(inherit: true))
                    throw new InvalidOperationException($"ID is not avialable for instances of '{GetType().Name}' before being saved to the database.");

                id = NewIdGenerator(GetType());
                IsIdLoaded = true;
                return id;

            }
            set
            {
                id = value;
                IsIdLoaded = true;
            }
        }

        static ConcurrentDictionary<Type, int> LastUsedIds = new ConcurrentDictionary<Type, int>();

        static int DefaultNewIdGenerator(Type type)
        {
            // One generator per hierarchy
            if (type.BaseType != typeof(IntEntity))
                return DefaultNewIdGenerator(type.BaseType);

            Func<Type, int> initialize = (t =>
            {
                if (TransientEntityAttribute.IsTransient(t)) return 1;

                return Database.GetList(t, new[] { QueryOption.Take(1), QueryOption.OrderByDescending("ID") })
                    .FirstOrDefault().Get(x => (int)x.GetId() + 1) ?? 1;
            });

            return LastUsedIds.AddOrUpdate(type, initialize, (t, old) => old + 1);
        }

        public static bool operator !=(IntEntity entity, int? id) => entity?.ID != id;

        public static bool operator ==(IntEntity entity, int? id) => entity?.ID == id;

        public static bool operator !=(IntEntity entity, int id) => entity?.ID != id;

        public static bool operator ==(IntEntity entity, int id) => entity?.ID == id;

        public static bool operator !=(int? id, IntEntity entity) => entity?.ID != id;

        public static bool operator ==(int? id, IntEntity entity) => entity?.ID == id;

        public static bool operator !=(int id, IntEntity entity) => entity?.ID != id;

        public static bool operator ==(int id, IntEntity entity) => entity?.ID == id;

        public override bool Equals(Entity other)
        {
            if (GetType() != other?.GetType()) return false;

            if (IsNew && other.IsNew) return ReferenceEquals(this, other);

            if (IsNew || other.IsNew) return false;

            return ID == (other as IntEntity)?.ID;
        }

        public override bool Equals(object other) => Equals(other as Entity);

        public override int GetHashCode() => IsNew ? 0 : ID.GetHashCode();
    }
}