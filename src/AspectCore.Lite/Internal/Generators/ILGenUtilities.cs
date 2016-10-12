using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

#if NETSTANDARD1_6
using System.Runtime.Loader;
#endif

namespace AspectCore.Lite.Generators
{
    internal static class ILGenUtilities
    {
#if NETSTANDARD1_6
        //private static readonly Assembly ExpressionsAssembly = AssemblyLoadContext.Default.
        //    LoadFromAssemblyName(new AssemblyName("System.Linq.Expressions"));

        private static readonly Assembly ExpressionsAssembly = Assembly.Load(new AssemblyName("System.Linq.Expressions"));
#elif NET451
        private static readonly Assembly ExpressionsAssembly = Assembly.Load(new AssemblyName("System.Linq.Expressions"));
#endif
        private static readonly Type ILGenType = ExpressionsAssembly.GetType("System.Linq.Expressions.Compiler.ILGen");
        
        private static readonly Action<ILGenerator , Type , Type , bool> ConvertToType =
            (Action<ILGenerator , Type , Type , bool>)ILGenType.GetTypeInfo().DeclaredMethods.SingleOrDefault(m =>
            m.Name == "EmitConvertToType").CreateDelegate(typeof(Action<ILGenerator , Type , Type , bool>));
        
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
    }
}
