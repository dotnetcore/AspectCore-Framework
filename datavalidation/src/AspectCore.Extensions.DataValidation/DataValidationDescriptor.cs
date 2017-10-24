using System;
using System.Linq;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Extensions.Reflection;
using System.ComponentModel.DataAnnotations;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataValidationDescriptor
    {
        public Type Type { get; }

        public Attribute[] Attributes { get; }

        public object Value { get; }

        public string DisplayName { get; }

        public string MemberName { get; }

        public DataValidationErrorCollection Errors { get; }

        public DataValidationState State { get; set; }

        public DataValidationDescriptor(Parameter paramter)
        {
            Type = paramter.Type;
            Value = paramter.Value;
            MemberName = paramter.Name;
            Attributes = paramter.ParameterInfo.GetReflector().GetCustomAttributes();
            Errors = new DataValidationErrorCollection();
            var displayAttribute = Attributes.FirstOrDefault(x => x is DisplayAttribute) as DisplayAttribute;
            DisplayName = displayAttribute?.Name ?? MemberName;
        }
    }
}