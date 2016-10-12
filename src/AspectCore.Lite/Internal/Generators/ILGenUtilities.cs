using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    internal static class ILGenUtilities
    {
        private static readonly Assembly ExpressionsAssembly = Assembly.Load(new AssemblyName(GeneratorConstants.ExpressionsAssembly));
        private static readonly Type ILGenType = ExpressionsAssembly.GetType(GeneratorConstants.ILGenType);
        private static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetTypeInfo().GetMethod("GetTypeFromHandle");

        private static readonly Action<ILGenerator , Type , Type , bool> ConvertToType =
            (Action<ILGenerator , Type , Type , bool>)ILGenType.GetTypeInfo().DeclaredMethods.SingleOrDefault(m =>
            m.Name == GeneratorConstants.EmitConvertToType).CreateDelegate(typeof(Action<ILGenerator , Type , Type , bool>));

        internal static void EmitLoadArg(this ILGenerator il, int index)
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

        internal static void EmitConvertToType(this ILGenerator il , Type typeFrom , Type typeTo , bool isChecked)
        {
            ConvertToType(il , typeFrom , typeTo , isChecked);
        }

        internal static void EmitThis(this ILGenerator ilGenerator)
        {
             ilGenerator.EmitLoadArg(0);
        }



        internal static void EmitTypeof(this ILGenerator ilGenerator, Type type)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.Emit(OpCodes.Call, GetTypeFromHandle);
        }

        internal static void EmitLoadInt(this ILGenerator ilGenerator, int value)
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
