using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public class ParameterDescriptor
    {
        private object _value;
        private ParameterInfo _parameterInfo;

        public ParameterDescriptor(object value, ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }
            _parameterInfo = parameterInfo;
            _value = value;
        }

        public virtual string Name
        {
            get
            {
                return _parameterInfo.Name;
            }
        }

        public virtual object Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == null)
                {
                    if (ParameterType.GetTypeInfo().IsValueType && !(ParameterType.GetTypeInfo().IsGenericType && ParameterType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        throw new InvalidOperationException($"object type are not equal \"{Name}\" parameter type or not a derived type of parameter type.");
                    }
                    this._value = value;
                    return;
                }
                Type valueType = value.GetType();
                if (valueType != ParameterType && !ParameterType.GetTypeInfo().IsAssignableFrom(valueType.GetTypeInfo()))
                {
                    throw new InvalidOperationException($"object type are not equal \"{Name}\" parameter type or not a derived type of parameter type.");
                }
                this._value = value;
            }
        }

        public virtual Type ParameterType
        {
            get
            {
                return _parameterInfo.ParameterType;
            }
        }

        public virtual ParameterInfo ParameterInfo
        {
            get
            {
                return _parameterInfo;
            }
        }

        public virtual Attribute[] CustomAttributes
        {
            get
            {
                return _parameterInfo.GetCustomAttributes().ToArray();
            }
        }
    }
}
