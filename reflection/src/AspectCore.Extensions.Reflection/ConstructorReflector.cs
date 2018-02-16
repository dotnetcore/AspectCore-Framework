using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection
{
    public partial class ConstructorReflector : MemberReflector<ConstructorInfo>, IParameterReflectorProvider
    {
        private readonly Func<object[], object> _invoker;
        private readonly ParameterReflector[] _parameterReflectors;

        public ParameterReflector[] ParameterReflectors => _parameterReflectors;

        private ConstructorReflector(ConstructorInfo constructorInfo) : base(constructorInfo)
        {
            _invoker = CreateInvoker();
            _parameterReflectors = constructorInfo.GetParameters().Select(x => ParameterReflector.Create(x)).ToArray();
        }

        protected virtual Func<object[], object> CreateInvoker()
        {
            var dynamicMethod = new DynamicMethod($"invoker-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object[]) }, _reflectionInfo.Module, true);
            var ilGen = dynamicMethod.GetILGenerator();

            var parameterTypes = _reflectionInfo.GetParameterTypes();
            if (parameterTypes.Length == 0)
            {
                ilGen.Emit(OpCodes.Newobj, _reflectionInfo);
                return CreateDelegate();
            }
            var refParameterCount = parameterTypes.Count(x => x.IsByRef);
            if (refParameterCount == 0)
            {
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ilGen.EmitLoadArg(0);
                    ilGen.EmitInt(i);
                    ilGen.Emit(OpCodes.Ldelem_Ref);
                    ilGen.EmitConvertFromObject(parameterTypes[i]);
                }
                ilGen.Emit(OpCodes.Newobj, _reflectionInfo);
                return CreateDelegate();
            }
            var indexedLocals = new IndexedLocalBuilder[refParameterCount];
            var index = 0;
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                ilGen.EmitLoadArg(0);
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
            ilGen.Emit(OpCodes.Newobj, _reflectionInfo);        
            for (var i = 0; i < indexedLocals.Length; i++)
            {
                ilGen.EmitLoadArg(0);
                ilGen.EmitInt(indexedLocals[i].Index);
                ilGen.Emit(OpCodes.Ldloc, indexedLocals[i].LocalBuilder);
                ilGen.EmitConvertToObject(indexedLocals[i].LocalType);
                ilGen.Emit(OpCodes.Stelem_Ref);
            }
            return CreateDelegate();

            Func<object[], object> CreateDelegate()
            {
                if (_reflectionInfo.DeclaringType.GetTypeInfo().IsValueType)
                    ilGen.EmitConvertToObject(_reflectionInfo.DeclaringType);
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