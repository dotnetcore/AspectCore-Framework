using System;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DataValidation;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.DataAnnotations
{
    [NonAspect]
    public class AnnotationDataValidator : IDataValidator
    {
        private readonly IPropertyValidator _propertyValidator;

        public AnnotationDataValidator(IPropertyValidator propertyValidator)
        {
            _propertyValidator = propertyValidator ?? throw new ArgumentNullException(nameof(propertyValidator));
        }

        public void Validate(DataValidationContext context)
        {
            foreach (var descriptor in context.DataMetaDatas)
            {
                Validate(descriptor, context.AspectContext);
            }
        }

        private void Validate(DataMetaData dataMetaData, AspectContext aspectContext)
        {
            var skip = dataMetaData.Attributes.FirstOrDefault(x => x is SkipValidationAttribute);
            if (skip != null)
            {
                dataMetaData.State = DataValidationState.Skipped;
                return;
            }
            foreach (var property in dataMetaData.Type.GetTypeInfo().GetProperties())
            {
                if (!property.CanRead)
                    continue;
                var propertyValidationContext = new PropertyValidationContext(new PropertyMetaData(property, dataMetaData.Value), aspectContext);
                var results = _propertyValidator.Validate(propertyValidationContext).ToList();
                dataMetaData.State = results.Count == 0 ? DataValidationState.Valid : DataValidationState.Invalid;
                results.ForEach(result => dataMetaData.Errors.Add(result));
            }
        }
    }
}