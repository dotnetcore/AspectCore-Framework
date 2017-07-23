using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector : MemberReflector<MethodInfo>
    {
        protected readonly Func<object, object[], object> _invoker;

        private MethodReflector(MethodInfo reflectionInfo) : base(reflectionInfo)
        {
            _invoker = CreateInvoker();
        }
        protected virtual Func<object, object[], object> CreateInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"invoker_{Guid.NewGuid()}",
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
                    ilGen.EmitLoadInt(i);
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
                ilGen.EmitLoadInt(i);
                ilGen.Emit(OpCodes.Ldelem_Ref);
                if (parameterTypes[i].IsByRef)
                {
                    var defType = parameterTypes[i].GetTypeInfo().MakeDefType();
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
                    ilGen.EmitLoadInt(indexedLocals[i].Index);
                    ilGen.Emit(OpCodes.Ldloc, indexedLocals[i].LocalBuilder);
                    ilGen.EmitConvertToObject(indexedLocals[i].LocalType);
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }
            });

            Func<object, object[], object> CreateDelegate(Action callback = null)
            {
                ilGen.EmitCall(OpCodes.Callvirt, _reflectionInfo, null);
                callback?.Invoke();
                if (_reflectionInfo.ReturnType == typeof(void)) ilGen.Emit(OpCodes.Ldnull);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
            }
        }

        public MethodInfo AsMethodInfo()
        {
            return _reflectionInfo;
        }

        public virtual object Invoke(object instance, params object[] parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _invoker(instance, parameters);
        }

        public virtual object StaticInvoke(params object[] parameters)
        {
            throw new InvalidOperationException($"Method {_reflectionInfo.Name} must be static. For invoke instance method, call 'Invoke'.");
        }

        //public override string ToString()
        //{
        //    return $"Method : {_reflectionInfo}  DeclaringType : {_reflectionInfo.DeclaringType}";
        //}
    }
}
