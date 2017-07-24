using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection.Emit
{
    public static class ILGeneratorExtensions
    {
        public static void EmitLoadArg(this ILGenerator ilGenerator, int index)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            switch (index)
            {
                case 0:
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= byte.MaxValue) ilGenerator.Emit(OpCodes.Ldarg_S, (byte)index);
                    else ilGenerator.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        public static void EmitLoadArgA(this ILGenerator ilGenerator, int index)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            if (index <= byte.MaxValue) ilGenerator.Emit(OpCodes.Ldarga_S, (byte)index);
            else ilGenerator.Emit(OpCodes.Ldarga, index);
        }

        public static void EmitConvertToObject(this ILGenerator ilGenerator, Type typeFrom)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (typeFrom == null)
            {
                throw new ArgumentNullException(nameof(typeFrom));
            }

            if (typeFrom.GetTypeInfo().IsGenericParameter)
            {
                ilGenerator.Emit(OpCodes.Box, typeFrom);
            }
            else
            {
                ilGenerator.EmitConvertToType(typeFrom, typeof(object), true);
            }
        }

        public static void EmitConvertFromObject(this ILGenerator ilGenerator, Type typeTo)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (typeTo == null)
            {
                throw new ArgumentNullException(nameof(typeTo));
            }

            if (typeTo.GetTypeInfo().IsGenericParameter)
            {
                ilGenerator.Emit(OpCodes.Unbox_Any, typeTo);
            }
            else
            {
                ilGenerator.EmitConvertToType(typeof(object), typeTo, true);
            }
        }

        public static void EmitThis(this ILGenerator ilGenerator)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            ilGenerator.EmitLoadArg(0);
        }

        public static void EmitTypeof(this ILGenerator ilGenerator, Type type)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetTypeFromHandle);
        }

        public static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            EmitMethodof(ilGenerator, method, method.DeclaringType);
        }

        public static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method, Type declaringType)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }

            ilGenerator.Emit(OpCodes.Ldtoken, method);
            ilGenerator.Emit(OpCodes.Ldtoken, method.DeclaringType);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetMethodFromHandle);
            ilGenerator.EmitConvertToType(typeof(MethodBase), typeof(MethodInfo));
        }

        public static void EmitLoadInt(this ILGenerator ilGenerator, int value)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            switch (value)
            {
                case -1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    ilGenerator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    ilGenerator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    ilGenerator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    ilGenerator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    ilGenerator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value > -129 && value < 128)
                        ilGenerator.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    else
                        ilGenerator.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        internal static void EmitConvertToType(this ILGenerator ilGen, Type typeFrom, Type typeTo, bool isChecked = true)
        {
            if (ilGen == null)
            {
                throw new ArgumentNullException(nameof(ilGen));
            }
            if (typeFrom == null)
            {
                throw new ArgumentNullException(nameof(typeFrom));
            }
            if (typeTo == null)
            {
                throw new ArgumentNullException(nameof(typeTo));
            }

            var typeFromInfo = typeFrom.GetTypeInfo();
            var typeToInfo = typeTo.GetTypeInfo();

            var nnExprType = typeFromInfo.GetNonNullableType();
            var nnType = typeToInfo.GetNonNullableType();

            if (TypeInfoUtils.AreEquivalent(typeFromInfo, typeToInfo))
            {
                return;
            }

            if (typeFromInfo.IsInterface || // interface cast
              typeToInfo.IsInterface ||
               typeFrom == typeof(object) || // boxing cast
               typeTo == typeof(object) ||
               typeFrom == typeof(System.Enum) ||
               typeFrom == typeof(System.ValueType) ||
               TypeInfoUtils.IsLegalExplicitVariantDelegateConversion(typeFromInfo, typeToInfo))
            {
                ilGen.EmitCastToType(typeFromInfo, typeToInfo);
            }
            else if (typeFromInfo.IsNullableType() || typeToInfo.IsNullableType())
            {
                ilGen.EmitNullableConversion(typeFromInfo, typeToInfo, isChecked);
            }
            else if (!(typeFromInfo.IsConvertible() && typeToInfo.IsConvertible()) // primitive runtime conversion
                     &&
                     (nnExprType.GetTypeInfo().IsAssignableFrom(nnType) || // down cast
                     nnType.GetTypeInfo().IsAssignableFrom(nnExprType))) // up cast
            {
                ilGen.EmitCastToType(typeFromInfo, typeToInfo);
            }
            else if (typeFromInfo.IsArray && typeToInfo.IsArray)
            {
                // See DevDiv Bugs #94657.
                ilGen.EmitCastToType(typeFromInfo, typeToInfo);
            }
            else
            {
                ilGen.EmitNumericConversion(typeFromInfo, typeToInfo, isChecked);
            }
        }

        private static void EmitCastToType(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo)
        {

            if (!typeFrom.IsValueType && typeTo.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, typeTo.AsType());
            }
            else if (typeFrom.IsValueType && !typeTo.IsValueType)
            {
                il.Emit(OpCodes.Box, typeFrom.AsType());
                if (typeTo.AsType() != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, typeTo.AsType());
                }
            }
            else if (!typeFrom.IsValueType && !typeTo.IsValueType)
            {
                il.Emit(OpCodes.Castclass, typeTo.AsType());
            }
            else
            {
                throw new InvalidCastException($"Caanot cast {typeFrom} to {typeTo}.");
            }
        }

        private static void EmitNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            bool isTypeFromNullable = TypeInfoUtils.IsNullableType(typeFrom);
            bool isTypeToNullable = TypeInfoUtils.IsNullableType(typeTo);
            if (isTypeFromNullable && isTypeToNullable)
                il.EmitNullableToNullableConversion(typeFrom, typeTo, isChecked);
            else if (isTypeFromNullable)
                il.EmitNullableToNonNullableConversion(typeFrom, typeTo, isChecked);
            else
                il.EmitNonNullableToNullableConversion(typeFrom, typeTo, isChecked);
        }

        private static void EmitNullableToNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            Label labIfNull = default(Label);
            Label labEnd = default(Label);
            LocalBuilder locFrom = null;
            LocalBuilder locTo = null;
            locFrom = il.DeclareLocal(typeFrom.AsType());
            il.Emit(OpCodes.Stloc, locFrom);
            locTo = il.DeclareLocal(typeTo.AsType());
            // test for null
            il.Emit(OpCodes.Ldloca, locFrom);
            il.EmitHasValue(typeFrom.AsType());
            labIfNull = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, labIfNull);
            il.Emit(OpCodes.Ldloca, locFrom);
            il.EmitGetValueOrDefault(typeFrom.AsType());
            Type nnTypeFrom = TypeInfoUtils.GetNonNullableType(typeFrom);
            Type nnTypeTo = TypeInfoUtils.GetNonNullableType(typeTo);
            il.EmitConvertToType(nnTypeFrom, nnTypeTo, isChecked);
            // construct result type
            ConstructorInfo ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            il.Emit(OpCodes.Newobj, ci);
            il.Emit(OpCodes.Stloc, locTo);
            labEnd = il.DefineLabel();
            il.Emit(OpCodes.Br_S, labEnd);
            // if null then create a default one
            il.MarkLabel(labIfNull);
            il.Emit(OpCodes.Ldloca, locTo);
            il.Emit(OpCodes.Initobj, typeTo.AsType());
            il.MarkLabel(labEnd);
            il.Emit(OpCodes.Ldloc, locTo);
        }

        private static void EmitNullableToNonNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            if (typeTo.IsValueType)
                il.EmitNullableToNonNullableStructConversion(typeFrom, typeTo, isChecked);
            else
                il.EmitNullableToReferenceConversion(typeFrom);
        }

        private static void EmitNullableToNonNullableStructConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            LocalBuilder locFrom = null;
            locFrom = il.DeclareLocal(typeFrom.AsType());
            il.Emit(OpCodes.Stloc, locFrom);
            il.Emit(OpCodes.Ldloca, locFrom);
            il.EmitGetValue(typeFrom.AsType());
            Type nnTypeFrom = TypeInfoUtils.GetNonNullableType(typeFrom);
            il.EmitConvertToType(nnTypeFrom, typeTo.AsType(), isChecked);
        }

        private static void EmitNullableToReferenceConversion(this ILGenerator il, TypeInfo typeFrom)
        {
            // We've got a conversion from nullable to Object, ValueType, Enum, etc.  Just box it so that
            // we get the nullable semantics.  
            il.Emit(OpCodes.Box, typeFrom.AsType());
        }

        private static void EmitNonNullableToNullableConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            LocalBuilder locTo = null;
            locTo = il.DeclareLocal(typeTo.AsType());
            Type nnTypeTo = TypeInfoUtils.GetNonNullableType(typeTo);
            il.EmitConvertToType(typeFrom.AsType(), nnTypeTo, isChecked);
            ConstructorInfo ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            il.Emit(OpCodes.Newobj, ci);
            il.Emit(OpCodes.Stloc, locTo);
            il.Emit(OpCodes.Ldloc, locTo);
        }

        private static void EmitNumericConversion(this ILGenerator il, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            bool isFromUnsigned = TypeInfoUtils.IsUnsigned(typeFrom);
            bool isFromFloatingPoint = TypeInfoUtils.IsFloatingPoint(typeFrom);
            if (typeTo.AsType() == typeof(Single))
            {
                if (isFromUnsigned)
                    il.Emit(OpCodes.Conv_R_Un);
                il.Emit(OpCodes.Conv_R4);
            }
            else if (typeTo.AsType() == typeof(Double))
            {
                if (isFromUnsigned)
                    il.Emit(OpCodes.Conv_R_Un);
                il.Emit(OpCodes.Conv_R8);
            }
            else
            {
                TypeCode tc = Type.GetTypeCode(typeTo.AsType());
                if (isChecked)
                {
                    // Overflow checking needs to know if the source value on the IL stack is unsigned or not.
                    if (isFromUnsigned)
                    {
                        switch (tc)
                        {
                            case TypeCode.SByte:
                                il.Emit(OpCodes.Conv_Ovf_I1_Un);
                                break;
                            case TypeCode.Int16:
                                il.Emit(OpCodes.Conv_Ovf_I2_Un);
                                break;
                            case TypeCode.Int32:
                                il.Emit(OpCodes.Conv_Ovf_I4_Un);
                                break;
                            case TypeCode.Int64:
                                il.Emit(OpCodes.Conv_Ovf_I8_Un);
                                break;
                            case TypeCode.Byte:
                                il.Emit(OpCodes.Conv_Ovf_U1_Un);
                                break;
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                il.Emit(OpCodes.Conv_Ovf_U2_Un);
                                break;
                            case TypeCode.UInt32:
                                il.Emit(OpCodes.Conv_Ovf_U4_Un);
                                break;
                            case TypeCode.UInt64:
                                il.Emit(OpCodes.Conv_Ovf_U8_Un);
                                break;
                            default:
                                throw new InvalidCastException();
                        }
                    }
                    else
                    {
                        switch (tc)
                        {
                            case TypeCode.SByte:
                                il.Emit(OpCodes.Conv_Ovf_I1);
                                break;
                            case TypeCode.Int16:
                                il.Emit(OpCodes.Conv_Ovf_I2);
                                break;
                            case TypeCode.Int32:
                                il.Emit(OpCodes.Conv_Ovf_I4);
                                break;
                            case TypeCode.Int64:
                                il.Emit(OpCodes.Conv_Ovf_I8);
                                break;
                            case TypeCode.Byte:
                                il.Emit(OpCodes.Conv_Ovf_U1);
                                break;
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                il.Emit(OpCodes.Conv_Ovf_U2);
                                break;
                            case TypeCode.UInt32:
                                il.Emit(OpCodes.Conv_Ovf_U4);
                                break;
                            case TypeCode.UInt64:
                                il.Emit(OpCodes.Conv_Ovf_U8);
                                break;
                            default:
                                throw new InvalidCastException();
                        }
                    }
                }
                else
                {
                    switch (tc)
                    {
                        case TypeCode.SByte:
                            il.Emit(OpCodes.Conv_I1);
                            break;
                        case TypeCode.Byte:
                            il.Emit(OpCodes.Conv_U1);
                            break;
                        case TypeCode.Int16:
                            il.Emit(OpCodes.Conv_I2);
                            break;
                        case TypeCode.UInt16:
                        case TypeCode.Char:
                            il.Emit(OpCodes.Conv_U2);
                            break;
                        case TypeCode.Int32:
                            il.Emit(OpCodes.Conv_I4);
                            break;
                        case TypeCode.UInt32:
                            il.Emit(OpCodes.Conv_U4);
                            break;
                        case TypeCode.Int64:
                            if (isFromUnsigned)
                            {
                                il.Emit(OpCodes.Conv_U8);
                            }
                            else
                            {
                                il.Emit(OpCodes.Conv_I8);
                            }
                            break;
                        case TypeCode.UInt64:
                            if (isFromUnsigned || isFromFloatingPoint)
                            {
                                il.Emit(OpCodes.Conv_U8);
                            }
                            else
                            {
                                il.Emit(OpCodes.Conv_I8);
                            }
                            break;
                        default:
                            throw new InvalidCastException();
                    }
                }
            }
        }

        internal static void EmitHasValue(this ILGenerator il, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("get_HasValue", BindingFlags.Instance | BindingFlags.Public);
            il.Emit(OpCodes.Call, mi);
        }

        internal static void EmitGetValueOrDefault(this ILGenerator il, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("GetValueOrDefault", Type.EmptyTypes);
            il.Emit(OpCodes.Call, mi);
        }

        internal static void EmitGetValue(this ILGenerator il, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
            il.Emit(OpCodes.Call, mi);
        }
    }
}
