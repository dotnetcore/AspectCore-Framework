using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Common
{
    public class MethodAccessor
    {
        private static readonly ConcurrentDictionary<MethodInfo, MethodInvoker> _table = new ConcurrentDictionary<MethodInfo, MethodInvoker>();

        protected MethodInfo _methodInfo;

        protected MethodAccessor()
        {
        }

        internal MethodAccessor(MethodInfo methodInfo)
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
            _methodInfo = methodInfo;
        }

        public MethodInvoker CreateMethodInvoker()
        {
            return _table.GetOrAdd(_methodInfo, key => InternalCreateMethodInvoker());
        }

        protected virtual MethodInvoker InternalCreateMethodInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"{_methodInfo.Name}_handler",
                typeof(object), new Type[] { typeof(object), typeof(object[]) }, _methodInfo.Module, true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();

            var parameterTypes = _methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();

            if (parameterTypes.Length != 0)
            {
                var lable = ilGen.DefineLabel();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Brtrue_S, lable);
                ilGen.Emit(OpCodes.Ldstr, "parameters");
                ilGen.Emit(OpCodes.Newobj, MethodConstant.ArgumentNullExceptionCtor);
                ilGen.Emit(OpCodes.Throw);
                ilGen.MarkLabel(lable);
            }

            if (!_methodInfo.IsStatic)
            {
                var lable = ilGen.DefineLabel();
                ilGen.EmitLoadArg(0);
                ilGen.Emit(OpCodes.Brtrue_S, lable);
                ilGen.Emit(OpCodes.Ldstr, "instance");
                ilGen.Emit(OpCodes.Newobj, MethodConstant.ArgumentNullExceptionCtor);
                ilGen.Emit(OpCodes.Throw);
                ilGen.MarkLabel(lable);
                ilGen.EmitLoadArg(0);
                ilGen.EmitConvertToType(typeof(object), _methodInfo.DeclaringType, true);
            }

            LocalBuilder[] locals = new LocalBuilder[parameterTypes.Length];

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.EmitLoadArg(1);
                ilGen.EmitLoadInt(i);
                if (parameterTypes[i].IsByRef)
                {
                    var defType = parameterTypes[i].MakeDefType();
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

            ilGen.EmitCall(_methodInfo.IsStatic || _methodInfo.DeclaringType.GetTypeInfo().IsValueType ? OpCodes.Call : OpCodes.Callvirt, _methodInfo, null);

            if (_methodInfo.ReturnType != typeof(void)) ilGen.EmitConvertToType(_methodInfo.ReturnType, typeof(object), true);

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i].IsByRef)
                {
                    ilGen.EmitLoadArg(1);
                    ilGen.EmitLoadInt(i);
                    ilGen.Emit(OpCodes.Ldloc, locals[i]);
                    ilGen.EmitConvertToType(parameterTypes[i].MakeDefType(), typeof(object), true);
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }
            }

            if (_methodInfo.ReturnType == typeof(void)) ilGen.Emit(OpCodes.Ldnull);

            ilGen.Emit(OpCodes.Ret);

            return (MethodInvoker)dynamicMethod.CreateDelegate(typeof(MethodInvoker));
        }
    }
}
