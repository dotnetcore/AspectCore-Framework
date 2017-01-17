using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Extensions
{
    public sealed class MethodAccessor
    {
        private static readonly ConcurrentDictionary<MethodInfo, MethodInvoker> invokerTable = new ConcurrentDictionary<MethodInfo, MethodInvoker>();

        private readonly MethodInfo methodInfo;

        public MethodAccessor(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            if (methodInfo.IsGenericMethodDefinition)
            {
                throw new ArgumentException($"GenericMethodDefinition \"{methodInfo}\" generic types are not valid.");
            }
            if (methodInfo.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
                throw new ArgumentException($"genericTypeDefinition \"{methodInfo.DeclaringType}\" generic types are not valid.");

            this.methodInfo = methodInfo;
        }

        public MethodInvoker CreateMethodInvoker()
        {
            return invokerTable.GetOrAdd(methodInfo, key => InternalCreateMethodInvoker());
        }

        private MethodInvoker InternalCreateMethodInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"{methodInfo.Name}_handler",
                typeof(object), new Type[] { typeof(object), typeof(object[]) }, methodInfo.Module, true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            var parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();

            if (parameterTypes.Length != 0)
            {
                var lable = ilGen.DefineLabel();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Brtrue_S, lable);
                ilGen.Emit(OpCodes.Ldstr, "parameters");
                ilGen.Emit(OpCodes.Newobj, MethodInfoConstant.ArgumentNullExceptionCtor);
                ilGen.Emit(OpCodes.Throw);
                ilGen.MarkLabel(lable);
            }

            if (!methodInfo.IsStatic)
            {
                var lable = ilGen.DefineLabel();
                ilGen.EmitLoadArg(0);
                ilGen.Emit(OpCodes.Brtrue_S, lable);
                ilGen.Emit(OpCodes.Ldstr, "instance");
                ilGen.Emit(OpCodes.Newobj, MethodInfoConstant.ArgumentNullExceptionCtor);
                ilGen.Emit(OpCodes.Throw);
                ilGen.MarkLabel(lable);
                ilGen.EmitLoadArg(0);
                ilGen.EmitConvertToType(typeof(object), methodInfo.DeclaringType, true);
            }

            LocalBuilder[] locals = new LocalBuilder[parameterTypes.Length];

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.EmitLoadArg(1);
                ilGen.EmitLoadInt(i);
                if (parameterTypes[i].IsByRef)
                {
                    var defType = parameterTypes[i].GetTypeInfo().MakeDefType();
                    ilGen.Emit(OpCodes.Ldelem_Ref);
                    ilGen.EmitConvertToType(typeof(object), defType, true);
                    ilGen.Emit(OpCodes.Stloc_S, (locals[i] = ilGen.DeclareLocal(defType, true)));
                    ilGen.Emit(OpCodes.Ldloca, locals[i]);
                }
                else
                {
                    ilGen.Emit(OpCodes.Ldelem_Ref);
                    ilGen.EmitConvertToType(typeof(object), parameterTypes[i], true);
                }
            }

            ilGen.EmitCall(methodInfo.IsStatic || methodInfo.DeclaringType.GetTypeInfo().IsValueType ? OpCodes.Call : OpCodes.Callvirt, methodInfo, null);

            if (methodInfo.ReturnType != typeof(void)) ilGen.EmitConvertToType(methodInfo.ReturnType, typeof(object), true);

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i].IsByRef)
                {
                    ilGen.EmitLoadArg(1);
                    ilGen.EmitLoadInt(i);
                    ilGen.Emit(OpCodes.Ldloc, locals[i]);
                    ilGen.EmitConvertToType(parameterTypes[i].GetTypeInfo().MakeDefType(), typeof(object), true);
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }
            }

            if (methodInfo.ReturnType == typeof(void)) ilGen.Emit(OpCodes.Ldnull);

            ilGen.Emit(OpCodes.Ret);

            return (MethodInvoker)dynamicMethod.CreateDelegate(typeof(MethodInvoker));
        }
    }
}
