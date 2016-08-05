using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Descriptors
{
    public sealed class ReturnParameterDescriptor : ParameterDescriptor
    {
        public ReturnParameterDescriptor(object value , ParameterInfo parameterInfo) : base(value , parameterInfo)
        {
        }

        public override object Value
        {
            get
            {
                return ParameterType == typeof(void) ? null : base.Value;
            }
            set
            {
                if (ParameterType != typeof(void)) base.Value = value;
            }
        }
    }
}
