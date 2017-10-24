using System;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataValidationDescriptor
    {
        public Type ObjectType { get; }

        public Attribute[] Attributes { get; }

        public object ObjectInstance { get; }

        public DataValidationErrorCollection Errors { get; }

        public DataValidationState State { get; set; }

        public DataValidationDescriptor(Parameter paramter)
        {
            ObjectType = paramter.Type;
            ObjectInstance = paramter.Value;
            Attributes = paramter.ParameterInfo.GetReflector().GetCustomAttributes();
            Errors = new DataValidationErrorCollection();
        }
    }
}