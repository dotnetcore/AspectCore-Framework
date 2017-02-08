using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static AspectCore.Abstractions.Extensions.ReflectionExtensions;

namespace AspectCore.Abstractions.Extensions
{
    public sealed class MethodAccessor
    {
        private static readonly ConcurrentDictionary<Tuple<MethodInfo, bool>, Func<object, object[], object>> invokerCache = new ConcurrentDictionary<Tuple<MethodInfo, bool>, Func<object, object[], object>>();

        private readonly bool isLookupVTable = false;

        private readonly MethodInfo method;

        public MethodAccessor(MethodInfo methodInfo)
            : this(methodInfo, true)
        {
        }

        public MethodAccessor(MethodInfo methodInfo, bool isLookupVTable)
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

            this.method = methodInfo;
            this.isLookupVTable = isLookupVTable;
        }

        public Func<object, object[], object> CreateMethodInvoker()
        {
            return invokerCache.GetOrAdd(Tuple.Create(method, isLookupVTable), key => InternalCreateMethodInvoker());
        }

        private Func<object, object[], object> InternalCreateMethodInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"{method.Name}_handler",
                typeof(object), new Type[] { typeof(object), typeof(object[]) }, method.Module, true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

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

            if (!method.IsStatic)
            {
                var lable = ilGen.DefineLabel();
                ilGen.EmitLoadArg(0);
                ilGen.Emit(OpCodes.Brtrue_S, lable);
                ilGen.Emit(OpCodes.Ldstr, "instance");
                ilGen.Emit(OpCodes.Newobj, MethodInfoConstant.ArgumentNullExceptionCtor);
                ilGen.Emit(OpCodes.Throw);
                ilGen.MarkLabel(lable);
                ilGen.EmitLoadArg(0);
                ilGen.EmitConvertToType(typeof(object), method.DeclaringType, true);
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

            ilGen.EmitCall((method.IsStatic || method.DeclaringType.GetTypeInfo().IsValueType || !isLookupVTable) ? OpCodes.Call : OpCodes.Callvirt, method, null);

            if (method.ReturnType != typeof(void)) ilGen.EmitConvertToType(method.ReturnType, typeof(object), true);

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

            if (method.ReturnType == typeof(void)) ilGen.Emit(OpCodes.Ldnull);

            ilGen.Emit(OpCodes.Ret);

            return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }
    }
}