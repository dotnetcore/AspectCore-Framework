using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector : MemberReflector<PropertyInfo>
    {
        protected readonly MethodReflector _getMethodReflector;
        protected readonly MethodReflector _setMethodReflector;

        private PropertyReflector(PropertyInfo reflectionInfo, CallOptions callOption) : base(reflectionInfo)
        {
            if (reflectionInfo.CanRead)
            {
                _getMethodReflector = MethodReflector.Create(reflectionInfo.GetMethod, callOption);
            }
            if (reflectionInfo.CanWrite)
            {
                _setMethodReflector = MethodReflector.Create(reflectionInfo.SetMethod, callOption);
            }
        }

        public virtual object GetValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _getMethodReflector.Invoke(instance);
        }

        public virtual void SetValue(object instance, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            _setMethodReflector.Invoke(instance, value);
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
