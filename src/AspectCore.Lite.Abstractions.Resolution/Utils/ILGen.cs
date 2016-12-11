using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Resolution.Utils
{
    internal static class IlGen
    {
#if NET45
        private static readonly Assembly ExpressionsAssembly = Assembly.GetAssembly(typeof(Expression));
#else
        private static readonly Assembly ExpressionsAssembly = Assembly.Load(new AssemblyName("System.Linq.Expressions"));
#endif
        private static readonly Type IlGenType = ExpressionsAssembly.GetType("System.Linq.Expressions.Compiler.ILGen");
       

        private static readonly Action<ILGenerator , Type , Type , bool> ConvertToType =
            (Action<ILGenerator , Type , Type , bool>)IlGenType.GetTypeInfo().DeclaredMethods.SingleOrDefault(m =>
            m.Name == "EmitConvertToType").CreateDelegate(typeof(Action<ILGenerator , Type , Type , bool>));

        internal static void EmitLoadArg(this ILGenerator il , int index)
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
                    if (index <= byte.MaxValue) il.Emit(OpCodes.Ldarg_S , (byte)index);
                    else il.Emit(OpCodes.Ldarg , index);
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

        internal static void EmitTypeof(this ILGenerator ilGenerator , Type type)
        {
            ilGenerator.Emit(OpCodes.Ldtoken , type);
            ilGenerator.Emit(OpCodes.Call , MethodConstant.GetTypeFromHandle);
        }

        internal static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method)
        {
            EmitMethodof(ilGenerator, method, method.DeclaringType);
        }

        internal static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method, Type declaringType)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, method);
            ilGenerator.Emit(OpCodes.Ldtoken, method.DeclaringType);
            ilGenerator.Emit(OpCodes.Call, MethodConstant.GetMothodFromHandle);
            ilGenerator.EmitConvertToType(typeof(MethodBase), typeof(MethodInfo), false);
        }

        internal static void EmitLoadInt(this ILGenerator ilGenerator , int value)
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
                        ilGenerator.Emit(OpCodes.Ldc_I4_S , (sbyte)value);
                    else
                        ilGenerator.Emit(OpCodes.Ldc_I4 , value);
                    break;
            }
        }
    }
}
