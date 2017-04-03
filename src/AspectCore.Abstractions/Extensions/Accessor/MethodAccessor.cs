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

        private readonly bool _isCallvirt = false;

        private readonly MethodInfo _method;

        public MethodAccessor(MethodInfo methodInfo)
            : this(methodInfo, true)
        {
        }

        public MethodAccessor(MethodInfo methodInfo, bool isCallvirt)
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

            _method = methodInfo;
            _isCallvirt = isCallvirt;
        }

        public Func<object, object[], object> CreateMethodInvoker()
        {
            return invokerCache.GetOrAdd(Tuple.Create(_method, _isCallvirt), key => InternalCreateMethodInvoker());
        }

        private Func<object, object[], object> InternalCreateMethodInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"{_method.Name}_handler",
                typeof(object), new Type[] { typeof(object), typeof(object[]) }, _method.Module, true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            var parameterTypes = _method.GetParameters().Select(p => p.ParameterType).ToArray();

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

            if (!_method.IsStatic)
            {
                var lable = ilGen.DefineLabel();
                ilGen.EmitLoadArg(0);
                ilGen.Emit(OpCodes.Brtrue_S, lable);
                ilGen.Emit(OpCodes.Ldstr, "instance");
                ilGen.Emit(OpCodes.Newobj, MethodInfoConstant.ArgumentNullExceptionCtor);
                ilGen.Emit(OpCodes.Throw);
                ilGen.MarkLabel(lable);
                ilGen.EmitLoadArg(0);
                ilGen.EmitConvertToType(typeof(object), _method.DeclaringType, true);
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

            ilGen.EmitCall((_method.IsStatic || _method.DeclaringType.GetTypeInfo().IsValueType || !_isCallvirt) ? OpCodes.Call : OpCodes.Callvirt, _method, null);

            if (_method.ReturnType != typeof(void)) ilGen.EmitConvertToType(_method.ReturnType, typeof(object), true);

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

            if (_method.ReturnType == typeof(void)) ilGen.Emit(OpCodes.Ldnull);

            ilGen.Emit(OpCodes.Ret);

            return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }
    }
}