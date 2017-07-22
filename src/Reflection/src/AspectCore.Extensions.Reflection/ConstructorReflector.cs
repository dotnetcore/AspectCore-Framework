using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public sealed class ConstructorReflector : MemberReflector<ConstructorInfo>
    {
        private readonly Func<object[], object> _invoker;
        private ConstructorReflector(ConstructorInfo constructorInfo) : base(constructorInfo)
        {
            _invoker = CreateInvoker();
        }

        #region private
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<object[], object> CreateInvoker()
        {
            var dynamicMethod = new DynamicMethod($"invoker-{Guid.NewGuid()}", TypeConstants.ObjectType, TypeConstants.ConstructorInvokerParameter, _reflectionInfo.Module, true);

            var ilGen = dynamicMethod.GetILGenerator();

            var parameterTypes = _reflectionInfo.GetParameterTypes();

            if (parameterTypes.Length == 0)
            {
                return CreateDelegate(dynamicMethod, ilGen);
            }

            //var lable = ilGen.DefineLabel();
            //ilGen.EmitLoadArg(0);
            //ilGen.Emit(OpCodes.Brtrue_S, lable);
            //ilGen.Emit(OpCodes.Ldstr, "parameters");
            //ilGen.Emit(OpCodes.Newobj, MethodInfoConstant.ArgumentNullExceptionCtor);
            //ilGen.Emit(OpCodes.Throw);
            //ilGen.MarkLabel(lable);

            var refParameterCount = parameterTypes.Count(x => x.IsByRef);
            if (refParameterCount == 0)
            {
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ilGen.EmitLoadArg(0);
                    ilGen.EmitLoadInt(i);
                    ilGen.Emit(OpCodes.Ldelem_Ref);
                    ilGen.EmitConvertToType(typeof(object), parameterTypes[i], true);
                }

                return CreateDelegate(dynamicMethod, ilGen);
            }
            var indexedLocals = new IndexedLocalBuilder[refParameterCount];
            var index = 0;
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i].IsByRef)
                {
                    var defType = parameterTypes[i].GetTypeInfo().MakeDefType();
                    var indexedLocal = new IndexedLocalBuilder(ilGen.DeclareLocal(defType), i);
                    indexedLocals[index++] = indexedLocal;
                    ilGen.EmitLoadArg(0);
                    ilGen.EmitLoadInt(i); ilGen.Emit(OpCodes.Ldelem_Ref);
                    ilGen.EmitConvertToType(typeof(object), defType, true);
                    ilGen.Emit(OpCodes.Stloc, indexedLocal.LocalBuilder);
                    ilGen.Emit(OpCodes.Ldloca, indexedLocal.LocalBuilder);
                }
                else
                {
                    ilGen.Emit(OpCodes.Ldelem_Ref);
                    ilGen.EmitConvertToType(typeof(object), parameterTypes[i], true);
                }
            }

            ilGen.Emit(OpCodes.Newobj, _reflectionInfo);

            for (var i = 0; i < indexedLocals.Length; i++)
            {
                ilGen.EmitLoadArg(0);
                ilGen.EmitLoadInt(indexedLocals[i].Index);
                ilGen.Emit(OpCodes.Ldloc, indexedLocals[i].LocalBuilder);
                ilGen.EmitConvertToType(indexedLocals[i].LocalType, typeof(object), true);
                ilGen.Emit(OpCodes.Stelem_Ref);
            }

            ilGen.Emit(OpCodes.Ret);
            return (Func<object[], object>)dynamicMethod.CreateDelegate(TypeConstants.ConstructorInvokerType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<object[], object> CreateDelegate(DynamicMethod dynamicMethod, ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Newobj, _reflectionInfo);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object[], object>)dynamicMethod.CreateDelegate(TypeConstants.ConstructorInvokerType);
        }

        #endregion

        #region internal
        internal static ConstructorReflector Create(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException(nameof(constructorInfo));
            }
            return ReflectorCache<ConstructorInfo, ConstructorReflector>.GetOrAdd(constructorInfo, info => new ConstructorReflector(constructorInfo));
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            return _invoker(args);
        }

        public ConstructorInfo AsConstructorInfo()
        {
            return _reflectionInfo;
        }
    }
}