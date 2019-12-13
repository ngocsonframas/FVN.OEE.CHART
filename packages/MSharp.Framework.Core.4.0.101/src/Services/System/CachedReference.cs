﻿namespace MSharp.Framework
{
    using System;

    internal interface ICachedReference { void Invalidate(); }

    /// <summary>
    /// Provides immediate access to retrieved entities. It is aware of deletes and updates.
    /// </summary>
    public class CachedReference<TEntity> : CachedReference<Guid, TEntity> where TEntity : GuidEntity
    {
    }

    /// <summary>
    /// Provides immediate access to retrieved entities. It is aware of deletes and updates.
    /// </summary>
    public class CachedReference<TId, TEntity> : ICachedReference where TEntity : Entity<TId> where TId : struct
    {
        TEntity Value;
        TId? Id;

        /// <summary>
        /// Gets the entity record from a specified database call expression.
        /// The first time it is loaded, all future calls will be immediately served.
        /// </summary>
        public TEntity Get(TId? id)
        {
            if (!Id.Equals(id)) Value = null; // Different ID from the cache.
            Id = id;

            if (Value == null)
            {
                if (id == null) return null;

                var result = Database.Get<TEntity>(id.ToString());

                if (!Database.AnyOpenTransaction())
                {
                    Value = result;
                    Value.RegisterCachedCopy(this);
                }
                else return result;
            }

            return Value;
        }

        protected void Bind(TEntity entity)
        {
            Id = entity?.ID ?? throw new ArgumentNullException(nameof(entity));
            Value = entity;

            if (!Database.AnyOpenTransaction())
                Value.RegisterCachedCopy(this);
        }

        void ICachedReference.Invalidate() => Value = null;
    }
}