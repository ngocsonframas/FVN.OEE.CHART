﻿namespace MSharp.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ManyToManyAttribute : Attribute
    {
        static ConcurrentDictionary<Tuple<Type, bool?>, PropertyInfo[]> Cache = new ConcurrentDictionary<Tuple<Type, bool?>, PropertyInfo[]>();

        /// <summary>
        /// Gets a list of types that depend on a given entity.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetManyToManyProperties(Type type)
        {
            return GetManyToManyProperties(type, lazy: null);
        }

        /// <summary>
        /// Gets a list of types that depend on a given entity.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetManyToManyProperties(Type type, bool? lazy)
        {
            if (type == null) throw new ArgumentNullException("type");

            var key = Tuple.Create(type, lazy);

            return Cache.GetOrAdd(key, x => FindManyToManyProperties(x.Item1, x.Item2));
        }

        /// <summary>
        /// Returns a list of types that depend on a given entity.
        /// </summary>
        static PropertyInfo[] FindManyToManyProperties(Type type, bool? lazy)
        {
            return (from p in type.GetProperties()
                    let att = p.GetCustomAttribute<ManyToManyAttribute>(inherit: true)
                    where att != null
                    where lazy == null || att.Lazy == lazy
                    select p).Distinct().ToArray();
        }

        #region Lazy
        /// <summary>
        /// Gets or sets the Lazy of this ManyToManyAttribute.
        /// </summary>
        public bool Lazy { get; set; }
        #endregion
    }
}