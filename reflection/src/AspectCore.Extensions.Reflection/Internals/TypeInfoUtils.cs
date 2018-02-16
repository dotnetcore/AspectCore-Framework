using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection.Internals
{
    internal static class TypeInfoUtils
    {
        internal static bool AreEquivalent(TypeInfo t1, TypeInfo t2)
        {
            return t1 == t2 || t1.IsEquivalentTo(t2.AsType());
        }

        internal static bool IsNullableType(this TypeInfo type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static Type GetNonNullableType(this TypeInfo type)
        {
            if (IsNullableType(type))
            {
                return type.GetGenericArguments()[0];
            }
            return type.AsType();
        }

        internal static bool IsLegalExplicitVariantDelegateConversion(TypeInfo source, TypeInfo dest)
        {
            if (!IsDelegate(source) || !IsDelegate(dest) || !source.IsGenericType || !dest.IsGenericType)
                return false;

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

                if (IsInvariant(genericParameter))
                {
                    return false;
                }

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

        private static bool IsDelegate(TypeInfo t)
        {
            return t.IsSubclassOf(typeof(System.MulticastDelegate));
        }

        private static bool IsInvariant(TypeInfo t)
        {
            return 0 == (t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask);
        }

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