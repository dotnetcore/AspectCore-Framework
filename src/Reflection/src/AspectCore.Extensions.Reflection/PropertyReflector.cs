using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector : MemberReflector<PropertyInfo>
    {
        protected readonly bool _canRead;
        protected readonly bool _canWrite;
        protected readonly Func<object, object> _getter;
        protected readonly Action<object, object> _setter;

        private PropertyReflector(PropertyInfo reflectionInfo) : base(reflectionInfo)
        {
            _getter = reflectionInfo.CanRead ? CreateGetter() : ins => throw new InvalidOperationException($"Property {_reflectionInfo.Name} does not define get accessor.");
            _setter = reflectionInfo.CanWrite ? CreateSetter() : (ins, val) => { throw new InvalidOperationException($"Property {_reflectionInfo.Name} does not define get accessor."); };
        }

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

        public virtual object GetValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _getter.Invoke(instance);
        }

        public virtual void SetValue(object instance, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            _setter(instance, value);
        }

        public virtual object GetStaticValue()
        {
            throw new InvalidOperationException($"Property {_reflectionInfo.Name} must be static to call this method. For get instance property value, call 'GetValue'.");
        }

        public virtual void SetStaticValue(object value)
        {
            throw new InvalidOperationException($"Property {_reflectionInfo.Name} must be static to call this method. For set instance property value, call 'SetValue'.");
        }

    }
}
