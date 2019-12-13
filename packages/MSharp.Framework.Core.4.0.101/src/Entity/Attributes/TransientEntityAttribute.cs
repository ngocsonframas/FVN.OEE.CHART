﻿using System;
using System.Reflection;

namespace MSharp.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TransientEntityAttribute : Attribute
    {
        public static bool IsTransient(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttribute<TransientEntityAttribute>(inherit: true) != null;
        }
    }
}
