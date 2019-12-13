﻿namespace MSharp.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CacheDependentAttribute : Attribute
    {
        /// <summary>
        /// Creates a new CacheDependantAttribute instance.
        /// </summary>
        public CacheDependentAttribute(Type dependentType)
        {
            if (dependentType == null)
                throw new ArgumentNullException("dependentType");

            DependentType = dependentType;
        }

        /// <summary>
        /// Gets the dependent type.
        /// </summary>
        public Type DependentType { get; private set; }

        static ConcurrentDictionary<Type, Type[]> Cache = new ConcurrentDictionary<Type, Type[]>();

        /// <summary>
        /// Gets a list of types that depend on a given entity.
        /// </summary>
        public static IEnumerable<Type> GetDependentTypes(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            return Cache.GetOrAdd(entityType, FindDependentTypes);
        }

        /// <summary>
        /// Finds a list of types that depend on a given entity.
        /// </summary>
        static Type[] FindDependentTypes(Type entityType)
        {
            return (from type in entityType.Assembly.GetTypes()
                    from p in type.GetProperties()
                    let att = Attribute.GetCustomAttribute(p, typeof(CacheDependentAttribute)) as CacheDependentAttribute
                    where att != null && att.DependentType.IsAssignableFrom(entityType)
                    select type).Distinct().ToArray();
        }
    }
}