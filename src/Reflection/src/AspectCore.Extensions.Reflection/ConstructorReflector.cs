using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection
{
    public partial class ConstructorReflector : MemberReflector<ConstructorInfo>
    {
        private readonly Func<object[], object> _invoker;
        private ConstructorReflector(ConstructorInfo constructorInfo) : base(constructorInfo)
        {
            _invoker = CreateInvoker();
        }

        protected virtual Func<object[], object> CreateInvoker()
        {
            var dynamicMethod = new DynamicMethod($"invoker-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object[]) }, _reflectionInfo.Module, true);
            var ilGen = dynamicMethod.GetILGenerator();

            var parameterTypes = _reflectionInfo.GetParameterTypes();
            if (parameterTypes.Length == 0)
            {
                return CreateDelegate();
            }
            var refParameterCount = parameterTypes.Count(x => x.IsByRef);
            if (refParameterCount == 0)
            {
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ilGen.EmitLoadArg(0);
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
                ilGen.EmitLoadArg(0);
                ilGen.EmitLoadInt(i);
                ilGen.Emit(OpCodes.Ldelem_Ref);
                if (parameterTypes[i].IsByRef)
                {
                    var defType = parameterTypes[i].GetTypeInfo().MakeDefType();
                    var indexedLocal = new IndexedLocalBuilder(ilGen.DeclareLocal(defType), i);
                    indexedLocals[index++] = indexedLocal;
                    ilGen.Emit(OpCodes.Stloc, indexedLocal.LocalBuilder);
                    ilGen.Emit(OpCodes.Ldloca, indexedLocal.LocalBuilder);
                }
                else
                {
                    ilGen.EmitConvertFromObject(parameterTypes[i]);
                }
            }
            ilGen.Emit(OpCodes.Newobj, _reflectionInfo);
            for (var i = 0; i < indexedLocals.Length; i++)
            {
                ilGen.EmitLoadArg(0);
                ilGen.EmitLoadInt(indexedLocals[i].Index);
                ilGen.Emit(OpCodes.Ldloc, indexedLocals[i].LocalBuilder);
                ilGen.EmitConvertToObject(indexedLocals[i].LocalType);
                ilGen.Emit(OpCodes.Stelem_Ref);
            }
            ilGen.Emit(OpCodes.Ret);
            return (Func<object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object[], object>));

            Func<object[], object> CreateDelegate()
            {
                ilGen.Emit(OpCodes.Newobj, _reflectionInfo);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object[], object>));
            }
        }

        public virtual object Invoke(params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            return _invoker(args);
        }
    }
}