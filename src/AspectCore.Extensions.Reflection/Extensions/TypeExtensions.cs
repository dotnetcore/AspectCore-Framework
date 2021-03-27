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
        private static readonly Type voidTaskResultType = Type.GetType("System.Threading.Tasks.VoidTaskResult", false);

        /// <summary>
        /// 通过签名获取方法
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <param name="signature">签名</param>
        /// <returns>方法</returns>
        public static MethodInfo GetMethodBySignature(this TypeInfo typeInfo, MethodSignature signature)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).
                FirstOrDefault(m => new MethodSignature(m) == signature);
        }

        /// <summary>
        /// 获取类型中声明的与签名一致方法
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <param name="signature">签名</param>
        /// <returns>方法</returns>
        public static MethodInfo GetDeclaredMethodBySignature(this TypeInfo typeInfo, MethodSignature signature)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.DeclaredMethods.FirstOrDefault(m => new MethodSignature(m) == signature);
        }

        /// <summary>
        /// 获取类型默认值
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <returns>默认值</returns>
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

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>默认值</returns>
        public static object GetDefaultValue(this Type type)
        {
            return type?.GetTypeInfo()?.GetDefaultValue();
        }

        /// <summary>
        /// 获取可见性
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <returns>是否可见</returns>
        public static bool IsVisible(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            //IsNested：是否为嵌套类型
            if (typeInfo.IsNested)
            {
                if (!typeInfo.DeclaringType.GetTypeInfo().IsVisible())
                {
                    return false;
                }
                //IsNestedPublic：是嵌套的并且声明为公共的
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

        /// <summary>
        /// 类型是否为Task类型
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <returns>结果</returns>
        public static bool IsTask(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.AsType() == typeof(Task);
        }

        /// <summary>
        /// 类型是否为Task<>类型
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <returns>结果</returns>
        public static bool IsTaskWithResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return isTaskOfTCache.GetOrAdd(typeInfo, Info => Info.IsGenericType && typeof(Task).GetTypeInfo().IsAssignableFrom(Info));
        }

        /// <summary>
        /// 类型是否为Task<T>类型,T为System.Threading.Tasks.VoidTaskResult类型
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <returns>结果</returns>
        public static bool IsTaskWithVoidTaskResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            return typeInfo.GenericTypeArguments?.Length > 0 && typeInfo.GenericTypeArguments[0] == voidTaskResultType; ;
        }

        /// <summary>
        /// 类型是否为ValueTask类型
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        public static bool IsValueTask(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.AsType() == typeof(ValueTask);
        }

        /// <summary>
        /// 类型是否为ValueTask<>类型
        /// </summary>
        /// <param name="typeInfo">待判断的类型</param>
        /// <returns>结果</returns>
        public static bool IsValueTaskWithResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            //GetGenericTypeDefinition方法说明：返回一个表示可用于构造当前泛型类型的泛型类型定义的 Type 对象（泛型模板）
            return isValueTaskOfTCache.GetOrAdd(typeInfo, Info => Info.IsGenericType && Info.GetGenericTypeDefinition() == typeof(ValueTask<>));
        }

        /// <summary>
        /// 类型是否为Nullable<>类型
        /// </summary>
        /// <param name="type">待判断的类型</param>
        /// <returns>结果</returns>
        public static bool IsNullableType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}