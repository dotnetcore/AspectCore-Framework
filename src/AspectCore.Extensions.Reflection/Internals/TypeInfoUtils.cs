using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection.Internals
{
    internal static class TypeInfoUtils
    {
        /// <summary>
        /// 判断两个类型是否等效
        /// </summary>
        /// <param name="t1">类型1</param>
        /// <param name="t2">类型2</param>
        /// <returns>是否等效</returns>
        internal static bool AreEquivalent(TypeInfo t1, TypeInfo t2)
        {
            return t1 == t2 || t1.IsEquivalentTo(t2.AsType());
        }

        /// <summary>
        /// 判断是否为Nullable<>类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>结果</returns>
        internal static bool IsNullableType(this TypeInfo type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 如果类型为Nullable<>类型,则返回其泛型类型参数
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>结果</returns>
        internal static Type GetNonNullableType(this TypeInfo type)
        {
            if (IsNullableType(type))
            {
                return type.GetGenericArguments()[0];
            }
            return type.AsType();
        }

        /// <summary>
        /// 判断两个泛型委托是否可显示转化
        /// </summary>
        /// <param name="source">源委托</param>
        /// <param name="dest">目标委托</param>
        /// <returns>结果</returns>
        internal static bool IsLegalExplicitVariantDelegateConversion(TypeInfo source, TypeInfo dest)
        {
            if (!IsDelegate(source) || !IsDelegate(dest) || !source.IsGenericType || !dest.IsGenericType)
                return false;

            //泛型类型定义(如：Dictionary<,>)是可用于构造其他类型的模板
            //如果使用相同的类型实参从相同的泛型类型定义创建两个构造类型，GetGenericTypeDefinition 方法对两个类型都返回相同的 Type 对象。
            var genericDelegate = source.GetGenericTypeDefinition();

            if (dest.GetGenericTypeDefinition() != genericDelegate)
                return false;

            var genericParameters = genericDelegate.GetTypeInfo().GetGenericArguments();
            var sourceArguments = source.GetGenericArguments();
            var destArguments = dest.GetGenericArguments();

            for (int iParam = 0; iParam < genericParameters.Length; ++iParam)
            {
                var sourceArgument = sourceArguments[iParam].GetTypeInfo();
                var destArgument = destArguments[iParam].GetTypeInfo();

                if (AreEquivalent(sourceArgument, destArgument))
                {
                    continue;
                }

                var genericParameter = genericParameters[iParam].GetTypeInfo();

                //不可变特征
                if (IsInvariant(genericParameter))
                {
                    return false;
                }
                //协变
                if (IsCovariant(genericParameter))
                {
                    if (!HasReferenceConversion(sourceArgument, destArgument))
                    {
                        return false;
                    }
                }
                else if (IsContravariant(genericParameter))
                {
                    if (sourceArgument.IsValueType || destArgument.IsValueType)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 类型是否为委托类型
        /// </summary>
        /// <param name="t">类型</param>
        /// <returns>结果</returns>
        private static bool IsDelegate(TypeInfo t)
        {
            return t.IsSubclassOf(typeof(System.MulticastDelegate));
        }

        /// <summary>
        /// 泛型类型参数是否不具有协变,逆变特征
        /// </summary>
        /// <param name="t">泛型类型参数</param>
        /// <returns>true 不具有协变,逆变特征,否则返回false</returns>
        private static bool IsInvariant(TypeInfo t)
        {
            return 0 == (t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask);
        }

        /// <summary>
        /// 泛型类型参数是否是协变
        /// </summary>
        /// <param name="t">泛型类型参数</param>
        /// <returns>是否协变</returns>
        private static bool IsCovariant(this TypeInfo t)
        {
            return 0 != (t.GenericParameterAttributes & GenericParameterAttributes.Covariant);
        }

        internal static bool HasReferenceConversion(TypeInfo source, TypeInfo dest)
        {
            // void -> void conversion is handled elsewhere
            // (it's an identity conversion)
            // All other void conversions are disallowed.
            if (source.AsType() == typeof(void) || dest.AsType() == typeof(void))
            {
                return false;
            }

            var nnSourceType = TypeInfoUtils.GetNonNullableType(source).GetTypeInfo();
            var nnDestType = TypeInfoUtils.GetNonNullableType(dest).GetTypeInfo();

            //派生关系的判断
            // Down conversion
            if (nnSourceType.IsAssignableFrom(nnDestType))
            {
                return true;
            }
            // Up conversion
            if (nnDestType.IsAssignableFrom(nnSourceType))
            {
                return true;
            }
            // Interface conversion
            if (source.IsInterface || dest.IsInterface)
            {
                return true;
            }
            // Variant delegate conversion
            if (IsLegalExplicitVariantDelegateConversion(source, dest))
                return true;

            // Object conversion
            if (source.AsType() == typeof(object) || dest.AsType() == typeof(object))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 该泛型类型参数是否是逆变
        /// </summary>
        /// <param name="t">泛型类型参数</param>
        /// <returns>true 是逆变,否则false</returns>
        private static bool IsContravariant(TypeInfo t)
        {
            return 0 != (t.GenericParameterAttributes & GenericParameterAttributes.Contravariant);
        }

        internal static bool IsConvertible(this TypeInfo typeInfo)
        {
            var type = GetNonNullableType(typeInfo);
            if (typeInfo.IsEnum)
            {
                return true;
            }
            //Type.GetTypeCode,获取指定 Type 的基础类型代码
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 是否为无符号的类型
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <returns>true 无符号,否则false</returns>
        internal static bool IsUnsigned(TypeInfo typeInfo)
        {
            var type = GetNonNullableType(typeInfo);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.Char:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsFloatingPoint(TypeInfo typeInfo)
        {
            var type = GetNonNullableType(typeInfo);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }
    }
}