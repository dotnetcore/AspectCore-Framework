using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    internal static class ILGenUtilities
    {
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
    }
}
