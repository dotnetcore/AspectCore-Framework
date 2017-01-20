using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public class ParameterDescriptor
    {
        private object value;
        private ParameterInfo parameterInfo;

        public ParameterDescriptor(object value, ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }
            this.parameterInfo = parameterInfo;
            this.value = value;
        }

        public virtual string Name
        {
            get
            {
                return parameterInfo.Name;
            }
        }

        public virtual object Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value == null)
                {
                    if (ParameterType.GetTypeInfo().IsValueType && !(ParameterType.GetTypeInfo().IsGenericType && ParameterType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        throw new InvalidOperationException($"object type are not equal \"{Name}\" parameter type or not a derived type of parameter type.");
                    }
                    this.value = value;
                    return;
                }
                Type valueType = value.GetType();
                if (valueType != ParameterType && !ParameterType.GetTypeInfo().IsAssignableFrom(valueType.GetTypeInfo()))
                {
                    throw new InvalidOperationException($"object type are not equal \"{Name}\" parameter type or not a derived type of parameter type.");
                }
                this.value = value;
            }
        }

        public virtual Type ParameterType
        {
            get
            {
                return parameterInfo.ParameterType;
            }
        }

        public virtual ParameterInfo ParameterInfo
        {
            get
            {
                return parameterInfo;
            }
        }

        public virtual Attribute[] CustomAttributes
        {
            get
            {
                return parameterInfo.GetCustomAttributes().ToArray();
            }
        }
    }
}
