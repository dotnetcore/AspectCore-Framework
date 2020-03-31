using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DataValidation;

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
            if (dataMetaData.Value is IValidatableObject validatableObject)
            {
                var validationContext = new ValidationContext(validatableObject, null, null);
                var results = validatableObject.Validate(validationContext)?.ToList() ?? new List<ValidationResult>();
                dataMetaData.State = results.Count == 0 ? DataValidationState.Valid : DataValidationState.Invalid;
                foreach (var result in results)
                    foreach (var member in result.MemberNames ?? new List<string>())
                        dataMetaData.Errors.Add(new DataValidationError(member, result.ErrorMessage));

            }
            foreach (var property in dataMetaData.Type.GetTypeInfo().GetProperties())
            {
                //property.GetIndexParameters().Length > 0 ；Indicates that the attribute is an indexer
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    continue;
                var propertyValidationContext = new PropertyValidationContext(new PropertyMetaData(property, dataMetaData.Value), aspectContext);
                var results = _propertyValidator.Validate(propertyValidationContext).ToList();
                dataMetaData.State = results.Count == 0 && dataMetaData.State != DataValidationState.Invalid ? DataValidationState.Valid : DataValidationState.Invalid;
                results.ForEach(result => dataMetaData.Errors.Add(result));
            }
        }
    }
}