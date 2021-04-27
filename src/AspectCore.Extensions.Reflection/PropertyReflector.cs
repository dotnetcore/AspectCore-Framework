using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 属性反射操作
    /// </summary>
    public partial class PropertyReflector : MemberReflector<PropertyInfo>
    {
        /// <summary>
        /// 是否可读
        /// </summary>
        protected readonly bool _canRead;

        /// <summary>
        /// 是否可写
        /// </summary>
        protected readonly bool _canWrite;

        /// <summary>
        /// get访问器委托
        /// </summary>
        protected readonly Func<object, object> _getter;

        /// <summary>
        /// set访问器委托
        /// </summary>
        protected readonly Action<object, object> _setter;

        /// <summary>
        /// 属性反射操作
        /// </summary>
        /// <param name="reflectionInfo">属性对象</param>
        private PropertyReflector(PropertyInfo reflectionInfo) : base(reflectionInfo)
        {
            _getter = reflectionInfo.CanRead ? CreateGetter() : ins => throw new InvalidOperationException($"Property {_reflectionInfo.Name} does not define get accessor.");
            _setter = reflectionInfo.CanWrite ? CreateSetter() : (ins, val) => { throw new InvalidOperationException($"Property {_reflectionInfo.Name} does not define get accessor."); };
        }

        /// <summary>
        /// 创建一个代表属性get访问器方法的委托
        /// </summary>
        /// <returns>代表属性get访问器方法的委托</returns>
        protected virtual Func<object, object> CreateGetter()
        {
            var dynamicMethod = new DynamicMethod($"getter-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object) }, _reflectionInfo.Module, true);
            var ilGen = dynamicMethod.GetILGenerator();
            ilGen.EmitLoadArg(0);
            ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);
            ilGen.Emit(OpCodes.Callvirt, _reflectionInfo.GetMethod);
            if (_reflectionInfo.PropertyType.GetTypeInfo().IsValueType)
                ilGen.EmitConvertToObject(_reflectionInfo.PropertyType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// 创建一个代表属性set访问器方法的委托
        /// </summary>
        /// <returns>代表属性set访问器方法的委托</returns>
        protected virtual Action<object, object> CreateSetter()
        {
            var dynamicMethod = new DynamicMethod($"setter-{Guid.NewGuid()}", typeof(void), new Type[] { typeof(object), typeof(object) }, _reflectionInfo.Module, true);
            var ilGen = dynamicMethod.GetILGenerator();
            ilGen.EmitLoadArg(0);
            ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);
            ilGen.EmitLoadArg(1);
            ilGen.EmitConvertFromObject(_reflectionInfo.PropertyType);
            ilGen.Emit(OpCodes.Callvirt, _reflectionInfo.SetMethod);
            ilGen.Emit(OpCodes.Ret);
            return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <returns>属性值</returns>
        public virtual object GetValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _getter.Invoke(instance);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="value">值</param>
        public virtual void SetValue(object instance, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            _setter(instance, value);
        }

        /// <summary>
        /// 获取静态属性值
        /// </summary>
        /// <returns>值</returns>
        public virtual object GetStaticValue()
        {
            throw new InvalidOperationException($"Property {_reflectionInfo.Name} must be static to call this method. For get instance property value, call 'GetValue'.");
        }

        /// <summary>
        /// 设置静态属性值
        /// </summary>
        /// <param name="value">值</param>
        public virtual void SetStaticValue(object value)
        {
            throw new InvalidOperationException($"Property {_reflectionInfo.Name} must be static to call this method. For set instance property value, call 'SetValue'.");
        }

    }
}
