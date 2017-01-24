using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;


namespace AspectCore.Abstractions.Extensions
{
    public static class ILGeneratorExtensions
    {
        private static readonly Delegate ConvertToType = MethodInfoConstant.EmitConvertToType.CreateDelegate(typeof(Action<ILGenerator, Type, Type, bool>));

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

        public static void EmitConvertToType(this ILGenerator ilGenerator, Type typeFrom, Type typeTo, bool isChecked)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            ((Action<ILGenerator, Type, Type, bool>)ConvertToType)(ilGenerator, typeFrom, typeTo, isChecked);
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
                ilGenerator.EmitConvertToType(typeFrom, typeof(object), false);
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

        internal static void EmitMethodof(this ILGenerator ilGenerator, MethodInfo method, Type declaringType)
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
            ilGenerator.EmitConvertToType(typeof(MethodBase), typeof(MethodInfo), false);
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

        internal static class MethodInfoConstant
        {
            private static readonly Type ILGenType = typeof(Expression).GetTypeInfo().Assembly.GetType("System.Linq.Expressions.Compiler.ILGen");

            internal static readonly MethodInfo EmitConvertToType = ILGenType.GetTypeInfo().GetMethod("EmitConvertToType", BindingFlags.NonPublic | BindingFlags.Static);

            internal static readonly MethodInfo GetTypeFromHandle = MethodInfosExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

            internal static readonly MethodInfo GetMethodFromHandle = MethodInfosExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

            internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
        }
    }
}
