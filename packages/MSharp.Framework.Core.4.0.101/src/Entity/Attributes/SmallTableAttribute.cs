﻿namespace MSharp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SmallTableAttribute : Attribute
    {
        static bool SMALL_TABLE_ALL = Config.Get<bool>("Database.SmallTable.All", defaultValue: false);
        const bool DEFAULT_UNCONFIGURED = false;

        static Dictionary<Type, bool> Cache = new Dictionary<Type, bool>();

        /// <summary>
        /// Determines if small table is specified for a given type.
        /// </summary>
        public static bool IsEnabled(Type type)
        {
            if (SMALL_TABLE_ALL) return true;

            if (type == null) throw new ArgumentNullException("type");

            bool result;

            if (Cache.TryGetValue(type, out result)) return result;

            return DetectAndCache(type);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static bool DetectAndCache(Type type)
        {
            var result = type.IsDefined(typeof(SmallTableAttribute), inherit: true);

            try { return Cache[type] = result; }
            catch { return result; }
        }
    }
}