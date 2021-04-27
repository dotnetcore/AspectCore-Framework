using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 方法反射操作
    /// </summary>
    public partial class MethodReflector : MemberReflector<MethodInfo>, IParameterReflectorProvider
    {
        protected readonly Func<object, object[], object> _invoker;
        private readonly ParameterReflector[] _parameterReflectors;

        public ParameterReflector[] ParameterReflectors => _parameterReflectors;

        /// <summary>
        /// 采用的Callvirt调用的方法反射操作
        /// </summary>
        /// <param name="reflectionInfo">方法</param>
        private MethodReflector(MethodInfo reflectionInfo) : base(reflectionInfo)
        {
            _displayName = GetDisplayName(reflectionInfo);
            _invoker = CreateInvoker();
            _parameterReflectors = reflectionInfo.GetParameters().Select(x => ParameterReflector.Create(x)).ToArray();
        }

        /// <summary>
        /// 创建代表实例方法的委托,方法采用的Callvirt调用
        /// </summary>
        /// <returns>代表实例方法的委托</returns>
        protected virtual Func<object, object[], object> CreateInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"invoker_{_displayName}",
               typeof(object), new Type[] { typeof(object), typeof(object[]) }, _reflectionInfo.Module, true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            var parameterTypes = _reflectionInfo.GetParameterTypes();

            //推送调用方法的对象引用
            ilGen.EmitLoadArg(0);
            ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);

            //无参数
            if (parameterTypes.Length == 0)
            {
                return CreateDelegate();
            }

            //参数都不按引用传递
            var refParameterCount = parameterTypes.Count(x => x.IsByRef);
            if (refParameterCount == 0)
            {
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ilGen.EmitLoadArg(1);
                    ilGen.EmitInt(i);
                    //OpCodes.Ldelem_Ref: 将位于指定数组索引处的包含对象引用的元素作为 O 类型（对象引用）加载到计算堆栈的顶部。
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

            //callback用于推送方法参数
            Func<object, object[], object> CreateDelegate(Action callback = null)
            {
                ilGen.EmitCall(OpCodes.Callvirt, _reflectionInfo, null);
                callback?.Invoke();
                if (_reflectionInfo.ReturnType == typeof(void)) ilGen.Emit(OpCodes.Ldnull);
                else if (_reflectionInfo.ReturnType.GetTypeInfo().IsValueType)
                    ilGen.EmitConvertToObject(_reflectionInfo.ReturnType);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
            }
        }

        /// <summary>
        /// 实例方法调用
        /// </summary>
        /// <param name="instance">实例对象</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>返回值</returns>
        public virtual object Invoke(object instance, params object[] parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _invoker(instance, parameters);
        }

        /// <summary>
        /// 静态方法调用
        /// </summary>
        /// <param name="parameters">参数数组</param>
        /// <returns>返回值</returns>
        public virtual object StaticInvoke(params object[] parameters)
        {
            throw new InvalidOperationException($"Method {_reflectionInfo.Name} must be static to call this method. For invoke instance method, call 'Invoke'.");
        }
    }
}
