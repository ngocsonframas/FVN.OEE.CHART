﻿namespace System
{
    using Collections.Concurrent;
    using Collections.Generic;
    using Linq;
    using Linq.Expressions;
    using Reflection;
    using System.Collections;
    using Threading;
    using Threading.Tasks;

    partial class MSharpExtensions
    {
        static ConcurrentDictionary<Tuple<Type, Type>, bool> TypeImplementsCache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();

        /// <summary>
        /// Gets all parent types hierarchy for this type.
        /// </summary>
        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            var result = new List<Type>();

            for (var @base = type.BaseType; @base != null; @base = @base.BaseType)
                result.Add(@base);

            return result;
        }

        /// <summary>
        /// Determines whether this type inherits from a specified base type, either directly or indirectly.
        /// </summary>
        public static bool InhritsFrom(this Type type, Type baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException("baseType");

            if (baseType.IsInterface)
                return type.Implements(baseType);

            return type.GetParentTypes().Contains(baseType);
        }

        public static bool Implements<T>(this Type type) => Implements(type, typeof(T));

        public static bool IsA<T>(this Type type) => typeof(T).IsAssignableFrom(type);

        public static bool IsA(this Type thisType, Type type) => type.IsAssignableFrom(thisType);

        public static bool References(this Assembly assembly, Assembly anotherAssembly)
        {
            if (assembly == null) throw new NullReferenceException("assembly should not be null.");
            if (anotherAssembly == null) throw new ArgumentNullException(nameof(anotherAssembly));

            return assembly.GetReferencedAssemblies().Any(each => each.FullName.Equals(anotherAssembly.FullName));
        }

        public static string GetDisplayName(this Type input)
        {
            var displayName = input.Name;

            for (int i = displayName.Length - 1; i >= 0; i--)
            {
                if (displayName[i] == char.ToUpper(displayName[i]))
                    if (i > 0)
                        displayName = displayName.Insert(i, " ");
            }

            return displayName;
        }

        public static IEnumerable<Type> WithAllParents(this Type @this)
        {
            yield return @this;

            if (@this.BaseType != null)
                foreach (var p in @this.BaseType.WithAllParents()) yield return p;
        }

        /// <summary>
        /// Retuns the name of this type in the same way that is used in C# programming.
        /// </summary>
        public static string GetCSharpName(this Type type, bool includeNamespaces = false)
        {
            if (type.GetGenericArguments().None()) return type.Name;

            return type.Name.TrimAfter("`", trimPhrase: true) + "<" +
                type.GetGenericArguments().Select(t => t.GetCSharpName(includeNamespaces)).ToString(", ") + ">";
        }

        public static bool Implements(this Type type, Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");

            var key = Tuple.Create(type, interfaceType);

            return TypeImplementsCache.GetOrAdd(key, t =>
            {
                if (!interfaceType.IsInterface)
                    throw new ArgumentException("The provided value for interfaceType, " + interfaceType.FullName + " is not an interface type.");

                if (t.Item1 == t.Item2) return true;

                var implementedInterface = t.Item1.GetInterface(t.Item2.FullName, ignoreCase: false);

                if (implementedInterface == null) return false;
                else return implementedInterface.FullName == t.Item2.FullName;
            });
        }

        /// <summary>
        /// Gets the value of this property on the specified object.
        /// </summary>
        public static object GetValue(this PropertyInfo property, object @object)
        {
            try
            {
                return property.GetValue(@object, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get the value of property '{0}.{1}' on the specified instance: {2}".FormatWith(property.DeclaringType.Name, property.Name, ex.Message), ex);
            }
        }

        /// <summary>
        /// Set the value of this property on the specified object.
        /// </summary>
        public static void SetValue(this PropertyInfo property, object @object, object value)
        {
            property.SetValue(@object, value, null);
        }

        /// <summary>
        /// Adds the specified types pair to this type dictionary.
        /// </summary>
        public static void Add<T, K>(this IDictionary<Type, Type> typeDictionary) => typeDictionary.Add(typeof(T), typeof(K));

        /// <summary>
        /// Creates the instance of this type.
        /// </summary>
        public static object CreateInstance(this Type type, params object[] constructorParameters)
        {
            return Activator.CreateInstance(type, constructorParameters);
        }

        /// <summary>
        /// Determines whether it has a specified attribute applied to it.
        /// </summary>
        public static bool Defines<TAttribute>(this MemberInfo member, bool inherit = true) where TAttribute : Attribute
        {
            return member.IsDefined(typeof(TAttribute), inherit);
        }

        /// <summary>
        /// Creates the instance of this type casted to the specified type.
        /// </summary>
        public static TCast CreateInstance<TCast>(this Type type, params object[] constructorParameters)
        {
            return (TCast)Activator.CreateInstance(type, constructorParameters);
        }

        /// <summary>
        /// Determines if this type is a nullable of something.
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            if (!type.IsGenericType) return false;

            if (type.GetGenericTypeDefinition() != typeof(Nullable<>)) return false;

            return true;
        }

        public static bool Is<T>(this PropertyInfo property, string propertyName)
        {
            var type1 = property.DeclaringType;
            var type2 = typeof(T);

            if (type1.IsA(type2) || type2.IsA(type1))
            {
                return property.Name == propertyName;
            }
            else return false;
        }

        /// <summary>
        /// Determines whether this property info is the specified property (in lambda expression).
        /// </summary>
        public static bool Is<T>(this PropertyInfo property, Expression<Func<T, object>> expression)
        {
            if (!typeof(T).IsA(property.DeclaringType)) return false;

            var body = expression.Body;

            string memberName;

            if (body is UnaryExpression)
            {
                body = (body as UnaryExpression).Operand;

                // if (!(body as UnaryExpression).Expression.Type.IsA(typeof(T))) return false;
                // memberName = ((body as UnaryExpression).Operand as MemberExpression).Member.Name;
            }

            if (body is MemberExpression)
            {
                // if (!(body as MemberExpression).Expression.Type.IsA(typeof(T))) return false;
                memberName = (body as MemberExpression).Member.Name;
            }
            else throw new NotSupportedException(body.GetType() + " is not supported.");

            return property.Name == memberName;
        }

        /// <summary>
        /// Determines whether this type is static.
        /// </summary>
        public static bool IsStatic(this Type type) => type.IsAbstract && type.IsSealed;

        public static bool IsExtensionMethod(this MethodInfo method)
        {
            return method.GetCustomAttributes<Runtime.CompilerServices.ExtensionAttribute>(inherit: false).Any();
        }

        // /// <summary>
        // /// Gets all defined attributes of the specified type.
        // /// </summary>
        // public static TAttribute[] GetCustomAttributes1<TAttribute>(this MemberInfo member, bool inherit = true) where TAttribute : Attribute
        // {
        //    member.GetCustomAttributes<TAttribute>()

        //    var result = member.GetCustomAttributes(typeof(TAttribute), inherit);
        //    if (result == null) return new TAttribute[0];
        //    else return result.Cast<TAttribute>().ToArray();
        // }

        #region Sub-Types

        static ConcurrentDictionary<Assembly, ConcurrentDictionary<Type, IEnumerable<Type>>> SubTypesCache = new ConcurrentDictionary<Assembly, ConcurrentDictionary<Type, IEnumerable<Type>>>();

        /// <summary>
        /// Gets all types in this assembly that are directly inherited from a specified base type.
        /// </summary>
        public static IEnumerable<Type> GetSubTypes(this Assembly assembly, Type baseType)
        {
            var cache = SubTypesCache.GetOrAdd(assembly, a => new ConcurrentDictionary<Type, IEnumerable<Type>>());

            // if (!SubTypesCache.ContainsKey(assembly))
            //    lock (SubTypesCache)
            //    {
            //        if (!SubTypesCache.ContainsKey(assembly))
            //            SubTypesCache.Add(assembly, new Dictionary<Type, IEnumerable<Type>>());
            //    }

            // var cache = SubTypesCache[assembly];

            return cache.GetOrAdd(baseType, bt =>
            {
                try
                {
                    return assembly.GetTypes().Where(t => t.BaseType == bt).ToArray();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    throw new Exception("Could not load the types of the assembly '{0}'. Type-load exceptions: {1}".FormatWith(assembly.FullName,
                        ex.LoaderExceptions.Select(e => e.Message).Distinct().ToString(" | ")));
                }
            });

            // if (!cache.ContainsKey(baseType))
            //    lock (assembly)
            //    {
            //        if (!cache.ContainsKey(baseType))
            //        {
            //            Type[] subTypes;
            //            try
            //            {
            //                subTypes = assembly.GetTypes().Where(t => t.BaseType == baseType).ToArray();
            //            }
            //            catch (ReflectionTypeLoadException ex)
            //            {
            //                throw new Exception("Could not load the types of the assembly '{0}'. Type-load exceptions: {1}".FormatWith(assembly.FullName,
            //                    ex.LoaderExceptions.Select(e => e.Message).Distinct().ToString(" | ")));
            //            }

            //            cache.Add(baseType, subTypes);
            //        }
            //    }

            // return cache[baseType];
        }

        #endregion

        static ConcurrentDictionary<Type, string> ProgrammingNameCache = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Gets the full programming name of this type. Unlike the standard FullName property, it handles Generic types properly.
        /// </summary>
        public static string GetProgrammingName(this Type type)
        {
            return ProgrammingNameCache.GetOrAdd(type, x => GetProgrammingName(x, useGlobal: false));
        }

        /// <summary>
        /// Gets the full programming name of this type. Unlike the standard FullName property, it handles Generic types properly.
        /// </summary>
        public static string GetProgrammingName(this Type type, bool useGlobal, bool useNamespace = true, bool useNamespaceForParams = true, bool useGlobalForParams = false)
        {
            if (type.GetGenericArguments().Any())
            {
                return "global::".OnlyWhen(useGlobal && type.FullName != null) +
                    "{0}{1}<{2}>".FormatWith(
                    type.Namespace.OnlyWhen(useNamespace).WithSuffix("."),
                    type.Name.Remove(type.Name.IndexOf('`')),
                    type.GetGenericArguments().Select(t => t.GetProgrammingName(useGlobalForParams, useNamespaceForParams, useNamespaceForParams, useGlobalForParams)).ToString(", "));
            }
            else
            {
                if (type.FullName == null) return type.Name.TrimEnd("&"); // Generic parameter name

                var typeName = type.Name.Replace("+", ".").TrimEnd("&");

                if (useGlobal || useNamespace)
                {
                    return "global::".OnlyWhen(useGlobal) + type.Namespace.OnlyWhen(useNamespace).WithSuffix(".") + typeName;
                }
                else return SimplifyCSharpName(typeName);
            }
        }

        static string SimplifyCSharpName(string type)
        {
            switch (type)
            {
                case "Boolean": return "bool";
                case "UInt16": return "ushort";
                case "Int16": return "short";
                case "UInt64": return "ulong";
                case "Int64": return "long";
                case "UInt32": return "uint";
                case "Int32": return "int";
                case "Single": return "float";
                case "SByte":
                case "Byte":
                case "Double":
                case "Decimal":
                case "Char":
                case "Object":
                case "String":
                    return type.ToLower();
                default: return type;
            }
        }

        /// <summary>
        /// Determines if this type is a generic class  of the specified type.
        /// </summary>
        public static bool IsGenericOf(this Type type, Type genericType, params Type[] genericParameters)
        {
            if (!type.IsGenericType) return false;

            if (type.GetGenericTypeDefinition() != genericType) return false;

            var args = type.GetGenericArguments();

            if (args.Length != genericParameters.Length) return false;

            for (var i = 0; i < args.Length; i++)
                if (args[i] != genericParameters[i]) return false;

            return true;
        }

        /// <summary>
        /// Gets the specified property.
        /// </summary>
        public static Expression<Func<T, object>> GetProperty<T>(this T instance, Expression<Func<T, object>> property)
        {
            return property;
        }

        /// <summary>
        /// Gets the specified property.
        /// </summary>
        public static string GetPropertyName<T>(this T instance, Expression<Func<T, object>> property)
        {
            return property.GetPropertyName();
        }

        /// <summary>
        /// Gets the specified property.
        /// </summary>
        public static string GetPropertyName<TObject, TProperty>(this TObject instance, Expression<Func<TObject, TProperty>> property)
        {
            return property.GetProperty().Name;
        }

        /// <summary>
        /// Gets the property name for a specified expression.
        /// </summary>
        public static MemberInfo GetMember<T, K>(this Expression<Func<T, K>> memberExpression)
        {
            var asMemberExpression = memberExpression.Body as MemberExpression;

            if (asMemberExpression == null)
            {
                // Maybe Unary:
                asMemberExpression = (memberExpression.Body as UnaryExpression)?.Operand as MemberExpression;
            }

            if (asMemberExpression == null) throw new Exception("Invalid expression");

            return asMemberExpression.Member;
        }

        // /// <summary>
        // /// Gets the property name for a specified expression.
        // /// </summary>
        // public static string GetPropertyName<T>(this Expression<Func<T, object>> propertyExpression)
        // {
        //    return propertyExpression.GetProperty<T, object>().Name;
        // }

        /// <summary>
        /// Gets the property name for a specified expression.
        /// </summary>
        public static string GetPropertyName<TObject, TProperty>(this Expression<Func<TObject, TProperty>> propertyExpression)
        {
            return propertyExpression.GetProperty<TObject, TProperty>().Name;
        }

        public static bool IsAnyOf(this Type type, params Type[] types)
        {
            if (type == null) return types.Any(x => x == null);

            return types.Contains(type);
        }

        static ConcurrentDictionary<Type, string> AssemblyQualifiedNameCache = new ConcurrentDictionary<Type, string>();
        public static string GetCachedAssemblyQualifiedName(this Type type)
        {
            return AssemblyQualifiedNameCache.GetOrAdd(type, x => x.AssemblyQualifiedName);
        }

        public static MemberInfo GetPropertyOrField(this Type type, string name)
        {
            return type.GetProperty(name) ?? (MemberInfo)type.GetField(name);
        }

        public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type type, BindingFlags flags)
        {
            return type.GetProperties(flags).Cast<MemberInfo>().Concat(type.GetFields(flags));
        }

        public static Type GetPropertyOrFieldType(this MemberInfo member)
        {
            return (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType;
        }

        /// <summary>
        /// Gets the root entity type of this type.
        /// If this type inherits directly from Entity&lt;T&gt; then it will be returned, otherwise its parent...
        /// </summary>
        public static Type GetRootEntityType(this Type objectType)
        {
            if (objectType.BaseType == null)
                throw new NotSupportedException(objectType.FullName + " not recognised. It must be a subclass of MSharp.Framework.Entity.");

            if (objectType.BaseType.Name == "GuidEntity") return objectType;
            if (objectType.BaseType == typeof(MSharp.Framework.Entity<int>)) return objectType;
            if (objectType.BaseType == typeof(MSharp.Framework.Entity<long>)) return objectType;
            if (objectType.BaseType == typeof(MSharp.Framework.Entity<string>)) return objectType;

            return GetRootEntityType(objectType.BaseType);
        }

        /// <summary>
        /// Gets all types in the current appDomain which implement this interface.
        /// </summary>
        public static List<Type> FindImplementerClasses(this Type interfaceType)
        {
            if (!interfaceType.IsInterface) throw new InvalidOperationException(interfaceType.GetType().FullName + " is not an Interface.");

            var result = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.References(interfaceType.Assembly)))
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type == interfaceType) continue;
                        if (!type.IsClass) continue;

                        if (type.Implements(interfaceType))
                        {
                            result.Add(type);
                        }
                    }
                }
                catch
                {
                    // Can't load assembly
                }
            }

            return result;
        }

        public static object GetObjectByPropertyPath(this Type type, object instance, string propertyPath)
        {
            if (propertyPath.Contains("."))
            {
                var directProperty = type.GetProperty(propertyPath.TrimAfter("."));

                if (directProperty == null)
                    throw new Exception(type.FullName + " does not have a property named '" + propertyPath.TrimAfter(".") + "'");

                var associatedObject = directProperty.GetValue(instance);
                if (associatedObject == null) return null;

                var remainingPath = propertyPath.TrimStart(directProperty.Name + ".");
                return associatedObject.GetType().GetObjectByPropertyPath(associatedObject, remainingPath);
            }
            else
                return type.GetProperty(propertyPath).GetValue(instance);
        }

        /// <summary>
        /// Creates a new thread and copies the current Culture and UI Culture.
        /// </summary>
        public static Thread CreateNew(this Thread thread, Action threadStart) => CreateNew(thread, threadStart, null);

        /// <summary>
        /// Creates a new thread and copies the current Culture and UI Culture.
        /// </summary>
        public static Thread CreateNew(this Thread thread, Action threadStart, Action<Thread> initializer)
        {
            var result = new Thread(new ThreadStart(threadStart))
            {
                CurrentCulture = thread.CurrentCulture,
                CurrentUICulture = thread.CurrentUICulture
            };

            initializer?.Invoke(result);
            return result;
        }

        public static PropertyInfo GetProperty<TModel, TProperty>(this Expression<Func<TModel, TProperty>> property)
        {
            return property.GetMember<TModel, TProperty>() as PropertyInfo;
        }

        /// <summary>
        /// Gets the default value for this type. It's equivalent to default(T).
        /// </summary>
        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        /// <summary>
        /// If it specifies DisplayNameAttribute the value from that will be returned.
        /// Otherwise it returns natural English literal text for the name of this member.
        /// For example it coverts "ThisIsSomething" to "This is something".
        /// </summary>
        public static string GetDisplayName(this MemberInfo member)
        {
            var byAttribute = member.GetCustomAttribute<ComponentModel.DisplayNameAttribute>()?.DisplayName;
            return byAttribute.Or(() => member.Name.ToLiteralFromPascalCase());
        }

        /// <summary>
        /// Determine whether this property is static.
        /// </summary>
        public static bool IsStatic(this PropertyInfo property)
        {
            return (property.GetGetMethod() ?? property.GetSetMethod()).IsStatic;
        }

        /// <summary>
        /// Awaits this task. Use this method to skip the Visual Studio warning on calling async methods in sync callers.
        /// </summary>
        public static void Await(this Task task) => new Action(async () => await task).Invoke();

        /// <summary>
        /// It works similar to calling .Result property, but it forces a context switch to prevent deadlocks in UI and ASP.NET context.
        /// </summary>
        public static TResult AwaitResult<TResult>(this Task<TResult> task) => Task.Run(async () => await task).Result;

        public static string GetPropertyPath(this Expression expression)
        {
            if (expression is MemberExpression m)
            {
                var result = m.Member.Name;

                if (m.Expression.ToString().Contains("."))
                    result = m.Expression.GetPropertyPath() + "." + result;

                return result;
            }

            if (expression is LambdaExpression l) return l.Body.GetPropertyPath();

            if (expression is UnaryExpression u) return u.Operand.GetPropertyPath();

            throw new Exception("Failed to get the property name from this expression: " + expression);
        }

        /// <summary>
        /// Invokes this static method.
        /// </summary>
        public static object InvokeStatic(this MethodInfo method, params object[] arguments)
        {
            if (!method.IsStatic) throw new Exception(method.Name + " is not static.");

            try { return method.Invoke(null, arguments); }
            catch (TargetInvocationException ex) { throw ex.InnerException; }
        }

        /// <summary>
        /// If this type implements IEnumerable«T» it returns typeof(T).
        /// </summary>
        public static Type GetEnumerableItemType(this Type type)
        {
            if (!type.Implements<IEnumerable>()) return null;

            if (type.IsArray) return type.GetElementType();

            if (type.IsGenericType)
            {
                var implementedIEnumerableT = type.GetInterfaces().FirstOrDefault(x =>
                x.GetGenericArguments().IsSingle() &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                return implementedIEnumerableT?.GetGenericArguments().Single();
            }

            return null;
        }

        internal static bool IsBasicNumeric(this Type type)
        {
            if (type == typeof(int)) return true;
            if (type == typeof(float)) return true;
            if (type == typeof(double)) return true;
            if (type == typeof(short)) return true;
            if (type == typeof(decimal)) return true;
            if (type == typeof(long)) return true;

            return false;
        }
    }
}