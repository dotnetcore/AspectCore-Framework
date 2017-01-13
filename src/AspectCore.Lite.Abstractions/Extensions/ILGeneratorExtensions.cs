using System;
using System.Reflection;
using System.Reflection.Emit;


namespace AspectCore.Lite.Abstractions.Extensions
{
    public static class ILGeneratorExtensions
    {
        private static readonly Delegate ConvertToType = MethodInfoConstant.EmitConvertToType.CreateDelegate(typeof(Action<ILGenerator, Type, Type, bool>));

        public static void EmitLoadArg(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= byte.MaxValue) il.Emit(OpCodes.Ldarg_S, (byte)index);
                    else il.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        public static void EmitLoadArgA(this ILGenerator il, int index)
        {
            if (index <= byte.MaxValue) il.Emit(OpCodes.Ldarga_S, (byte)index);
            else il.Emit(OpCodes.Ldarga, index);
        }

        public static void EmitConvertToType(this ILGenerator il, Type typeFrom, Type typeTo, bool isChecked)
        {
            ((Action<ILGenerator, Type, Type, bool>)ConvertToType)(il, typeFrom, typeTo, isChecked);
        }

        public static void EmitConvertToObject(this ILGenerator il, Type typeFrom)
        {
            if (typeFrom.GetTypeInfo().IsGenericParameter)
            {
                il.Emit(OpCodes.Box, typeFrom);
            }
            else
            {
                il.EmitConvertToType(typeFrom, typeof(object), false);
            }
        }

        public static void EmitThis(this ILGenerator ilGenerator)
        {
            ilGenerator.EmitLoadArg(0);
        }

        public static void EmitTypeof(this ILGenerator ilGenerator, Type type)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetTypeFromHandle);
        }

        public static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method)
        {
            EmitMethodof(ilGenerator, method, method.DeclaringType);
        }

        internal static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method, Type declaringType)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, method);
            ilGenerator.Emit(OpCodes.Ldtoken, method.DeclaringType);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetMethodFromHandle);
            ilGenerator.EmitConvertToType(typeof(MethodBase), typeof(MethodInfo), false);
        }

        public static void EmitLoadInt(this ILGenerator ilGenerator, int value)
        {
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
    }
}
