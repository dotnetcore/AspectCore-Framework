using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core.Descriptors
{
    public class ParameterDescriptor
    {
        private object value;
        private ParameterInfo metaDataInfo;

        public ParameterDescriptor(object value, ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));
            this.metaDataInfo = parameterInfo;
            this.Value = value;         
        }

        public string Name
        {
            get
            {
                return metaDataInfo.Name;
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
                    if (ParameterType.GetTypeInfo().IsValueType && ParameterType != typeof(Nullable<>))
                        throw new InvalidOperationException($"object type are not equal \"{Name}\" parameter type or not a derived type of parameter type.");
                    this.value = value;
                    return;
                }

                Type valueType = value.GetType();

                if (valueType != ParameterType)
                {
                    if (!ParameterType.GetTypeInfo().IsAssignableFrom(valueType.GetTypeInfo()))
                        throw new InvalidOperationException($"object type are not equal \"{Name}\" parameter type or not a derived type of parameter type.");
                }

                this.value = value;
            }
        }

        public Type ParameterType
        {
            get
            {
                return metaDataInfo.ParameterType;
            }
        }

        public ParameterInfo MetaDataInfo
        {
            get
            {
                return metaDataInfo;
            }
        }

        public Attribute[] CustomAttributes
        {
            get
            {
                return metaDataInfo.GetCustomAttributes().ToArray();
            }
        }
    }
}
