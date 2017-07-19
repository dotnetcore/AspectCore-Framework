using System.Reflection;

namespace AspectCore.Core
{
    internal class ReturnParameterDescriptor : ParameterDescriptor
    {
        public ReturnParameterDescriptor(object value, ParameterInfo parameterInfo) : base(value, parameterInfo)
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
