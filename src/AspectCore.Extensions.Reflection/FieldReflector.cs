using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 字段反射操作
    /// </summary>
    public partial class FieldReflector : MemberReflector<FieldInfo>
    {
        protected readonly Func<object, object> _getter;
        protected readonly Action<object, object> _setter;

        /// <summary>
        /// 字段反射操作
        /// </summary>
        /// <param name="reflectionInfo">字段</param>
        protected FieldReflector(FieldInfo reflectionInfo) : base(reflectionInfo)
        {
            _getter = CreateGetter();
            _setter = CreateSetter();
        }

        /// <summary>
        /// 创建针对此字段的get访问器方法
        /// </summary>
        /// <returns>一个代表get访问器的委托</returns>
        protected virtual Func<object, object> CreateGetter()
        {
            var dynamicMethod = new DynamicMethod($"getter-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object) }, _reflectionInfo.Module, true);
            var ilGen = dynamicMethod.GetILGenerator();
            ilGen.EmitLoadArg(0);
            ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);
            ilGen.Emit(OpCodes.Ldfld, _reflectionInfo);
            ilGen.EmitConvertToObject(_reflectionInfo.FieldType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// 创建针对此字段的set访问器方法
        /// </summary>
        /// <returns>一个代表set访问器的委托</returns>
        protected virtual Action<object, object> CreateSetter()
        {
            var dynamicMethod = new DynamicMethod($"setter-{Guid.NewGuid()}", typeof(void), new Type[] { typeof(object), typeof(object) }, _reflectionInfo.Module, true);
            var ilGen = dynamicMethod.GetILGenerator();
            ilGen.EmitLoadArg(0);
            ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);
            ilGen.EmitLoadArg(1);
            ilGen.EmitConvertFromObject(_reflectionInfo.FieldType);
            ilGen.Emit(OpCodes.Stfld, _reflectionInfo);
            ilGen.Emit(OpCodes.Ret);
            return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <returns>值</returns>
        public virtual object GetValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _getter(instance);
        }

        /// <summary>
        /// 设置值
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
        /// 静态字段获取值
        /// </summary>
        /// <returns>值</returns>
        public virtual object GetStaticValue()
        {
            throw new InvalidOperationException($"Field {_reflectionInfo.Name} must be static to call this method. For get instance field value, call 'GetValue'.");
        }

        /// <summary>
        /// 静态字段设置值
        /// </summary>
        /// <param name="value">值</param>
        public virtual void SetStaticValue(object value)
        {
            throw new InvalidOperationException($"Field {_reflectionInfo.Name} must be static to call this method. For set instance field value, call 'SetValue'.");
        }
    }
}