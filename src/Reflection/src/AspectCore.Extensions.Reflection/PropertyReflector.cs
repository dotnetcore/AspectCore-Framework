using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector : MemberReflector<PropertyInfo>
    {
        protected readonly MethodReflector _getMethodReflector;
        protected readonly MethodReflector _setMethodReflector;
        protected readonly bool _canRead;

        private PropertyReflector(PropertyInfo reflectionInfo, CallOptions callOption) : base(reflectionInfo)
        {
            _canRead = reflectionInfo.CanRead;
            if (reflectionInfo.CanRead)
            {
                _getMethodReflector = MethodReflector.Create(reflectionInfo.GetMethod, callOption);
            }
            if (reflectionInfo.CanWrite)
            {
                _setMethodReflector = MethodReflector.Create(reflectionInfo.SetMethod, callOption);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckGetReflector()
        {
            if (_getMethodReflector == null)
            {
                throw new InvalidOperationException($"Property {_reflectionInfo.Name} does not define get accessor.");
            }
        }

        protected void CheckSetReflector()
        {
            if (_setMethodReflector == null)
            {
                throw new InvalidOperationException($"Property {_reflectionInfo.Name} does not define set accessor.");
            }
        }

        public virtual object GetValue(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (!_canRead)
            {
                throw new InvalidOperationException($"Property {_reflectionInfo.Name} does not define get accessor.");
            }
            return _getMethodReflector.Invoke(instance);
        }

        public virtual void SetValue(object instance, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            CheckSetReflector();
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
