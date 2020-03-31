using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<TypeInfo, bool> isTaskOfTCache = new ConcurrentDictionary<TypeInfo, bool>();
        private static readonly ConcurrentDictionary<TypeInfo, bool> isValueTaskOfTCache = new ConcurrentDictionary<TypeInfo, bool>();

        public static MethodInfo GetMethodBySignature(this TypeInfo typeInfo, MethodSignature signature)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).
                FirstOrDefault(m => new MethodSignature(m) == signature);
        }

        public static MethodInfo GetDeclaredMethodBySignature(this TypeInfo typeInfo, MethodSignature signature)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.DeclaredMethods.FirstOrDefault(m => new MethodSignature(m) == signature);
        }

        public static object GetDefaultValue(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (typeInfo.AsType() == typeof(void))
            {
                return null;
            }

            switch (Type.GetTypeCode(typeInfo.AsType()))
            {
                case TypeCode.Object:
                case TypeCode.DateTime:
                    if (typeInfo.IsValueType)
                    {
                        return Activator.CreateInstance(typeInfo.AsType());
                    }
                    else
                    {
                        return null;
                    }

                case TypeCode.Empty:
                case TypeCode.String:
                    return null;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return 0;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return 0;

                case TypeCode.Single:
                    return default(Single);

                case TypeCode.Double:
                    return default(Double);

                case TypeCode.Decimal:
                    return new Decimal(0);

                default:
                    throw new InvalidOperationException("Code supposed to be unreachable.");
            }
        }

        public static object GetDefaultValue(this Type type)
        {
            return type?.GetTypeInfo()?.GetDefaultValue();
        }

        public static bool IsVisible(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (typeInfo.IsNested)
            {
                if (!typeInfo.DeclaringType.GetTypeInfo().IsVisible())
                {
                    return false;
                }
                if (!typeInfo.IsVisible || !typeInfo.IsNestedPublic)
                {
                    return false;
                }
            }
            else
            {
                if (!typeInfo.IsVisible || !typeInfo.IsPublic)
                {
                    return false;
                }
            }
            if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                foreach (var argument in typeInfo.GenericTypeArguments)
                {
                    if (!argument.GetTypeInfo().IsVisible())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsTask(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.AsType() == typeof(Task);
        }

        public static bool IsTaskWithResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return isTaskOfTCache.GetOrAdd(typeInfo, Info => Info.IsGenericType && typeof(Task).GetTypeInfo().IsAssignableFrom(Info));
        }
        
        public static bool IsValueTask(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.AsType() == typeof(ValueTask);
        }

        public static bool IsValueTaskWithResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return isValueTaskOfTCache.GetOrAdd(typeInfo, Info => Info.IsGenericType && Info.GetGenericTypeDefinition() == typeof(ValueTask<>));
        }

        public static bool IsNullableType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}