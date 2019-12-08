using System;
using System.Linq;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Extensions.Reflection;
using System.ComponentModel.DataAnnotations;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class DataMetaData
    {
        public Type Type { get; }

        public Attribute[] Attributes { get; }

        public object Value { get; }

        public DataValidationErrorCollection Errors { get; }

        public DataValidationState State { get; set; }

        public DataMetaData(Parameter paramter)
        {
            Type = paramter.Type;
            Value = paramter.Value;
            Attributes = paramter.ParameterInfo.GetReflector().GetCustomAttributes();
            Errors = new DataValidationErrorCollection();
        }
    }
}