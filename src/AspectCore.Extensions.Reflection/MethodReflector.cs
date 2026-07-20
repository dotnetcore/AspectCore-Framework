using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector : MemberReflector<MethodInfo>, IParameterReflectorProvider
    {
        protected readonly Func<object, object[], object> _invoker;
        private readonly ParameterReflector[] _parameterReflectors;

        public ParameterReflector[] ParameterReflectors => _parameterReflectors;

        private MethodReflector(MethodInfo reflectionInfo) : base(reflectionInfo)
        {
            _displayName = GetDisplayName(reflectionInfo);
            _invoker = CreateInvoker();
            _parameterReflectors = reflectionInfo.GetParameters().Select(x => ParameterReflector.Create(x)).ToArray();
        }

        protected virtual Func<object, object[], object> CreateInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"invoker_{_displayName}",
               typeof(object), new Type[] { typeof(object), typeof(object[]) }, _reflectionInfo.Module, true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            var parameterTypes = _reflectionInfo.GetParameterTypes();

            ilGen.EmitLoadArg(0);
            ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);

            if (parameterTypes.Length == 0)
            {
                return CreateDelegate();
            }

            var refParameterCount = parameterTypes.Count(x => x.IsByRef);
            if (refParameterCount == 0)
            {
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ilGen.EmitLoadArg(1);
                    ilGen.EmitInt(i);
                    ilGen.Emit(OpCodes.Ldelem_Ref);
                    ilGen.EmitConvertFromObject(parameterTypes[i]);
                }
                return CreateDelegate();
            }

            var indexedLocals = new IndexedLocalBuilder[refParameterCount];
            var index = 0;
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.EmitLoadArg(1);
                ilGen.EmitInt(i);
                ilGen.Emit(OpCodes.Ldelem_Ref);
                if (parameterTypes[i].IsByRef)
                {
                    var defType = parameterTypes[i].GetElementType();
                    var indexedLocal = new IndexedLocalBuilder(ilGen.DeclareLocal(defType), i);
                    indexedLocals[index++] = indexedLocal;
                    ilGen.EmitConvertFromObject(defType);
                    ilGen.Emit(OpCodes.Stloc, indexedLocal.LocalBuilder);
                    ilGen.Emit(OpCodes.Ldloca, indexedLocal.LocalBuilder);
                }
                else
                {
                    ilGen.EmitConvertFromObject(parameterTypes[i]);
                }
            }

            return CreateDelegate(() =>
            {
                for (var i = 0; i < indexedLocals.Length; i++)
                {
                    ilGen.EmitLoadArg(1);
                    ilGen.EmitInt(indexedLocals[i].Index);
                    ilGen.Emit(OpCodes.Ldloc, indexedLocals[i].LocalBuilder);
                    ilGen.EmitConvertToObject(indexedLocals[i].LocalType);
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }
            });

            Func<object, object[], object> CreateDelegate(Action callback = null)
            {
#if NET6_0
                //https://github.com/dotnet/runtime/issues/67084
                ilGen.Emit(OpCodes.Callvirt, _reflectionInfo);
#else
                ilGen.EmitCall(OpCodes.Callvirt, _reflectionInfo, null);
#endif
                callback?.Invoke();
                EmitReturn(ilGen, _reflectionInfo);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
            }
        }

        [RequiresDynamicCode("MethodReflector uses DynamicMethod (IL emit) for fast invocation. Use source-generated delegates for NativeAOT compatibility.")]
        public virtual object Invoke(object instance, params object[] parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _invoker(instance, parameters);
        }

        [RequiresDynamicCode("MethodReflector uses DynamicMethod (IL emit) for fast invocation. Use source-generated delegates for NativeAOT compatibility.")]
        public virtual object StaticInvoke(params object[] parameters)
        {
            throw new InvalidOperationException($"Method {_reflectionInfo.Name} must be static to call this method. For invoke instance method, call 'Invoke'.");
        }

        /// <summary>
        /// Emits the boxing/return conversion for an invoker's return value.
        /// Handles void, value-type boxing, and C# 7.0 ref/ref readonly returns
        /// (the call leaves a managed pointer T&amp; on the stack that must be
        /// dereferenced before being returned as <see cref="object"/>).
        /// </summary>
        private protected static void EmitReturn(ILGenerator ilGen, MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType == typeof(void))
            {
                ilGen.Emit(OpCodes.Ldnull);
                return;
            }

            if (returnType.IsByRef)
            {
                // ref / ref readonly return: dereference the managed pointer, then box
                // value types. The interceptor pipeline is value-based, so callers only
                // observe the pointed-to value here.
                var elementType = returnType.GetElementType();
                ilGen.EmitLdRef(elementType);
                if (elementType.GetTypeInfo().IsValueType)
                {
                    ilGen.EmitConvertToObject(elementType);
                }
                return;
            }

            if (returnType.GetTypeInfo().IsValueType)
            {
                ilGen.EmitConvertToObject(returnType);
            }
        }
    }
}
