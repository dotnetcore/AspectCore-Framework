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
    }
}
