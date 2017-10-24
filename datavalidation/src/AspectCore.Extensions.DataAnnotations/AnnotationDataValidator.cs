using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DataValidation;
using System.Linq;

namespace AspectCore.Extensions.DataAnnotations
{
    [NonAspect]
    public class AnnotationDataValidator : IDataValidator
    {
        public void Validate(DataValidationContext context)
        {
            foreach (var descriptor in context.DataValidationDescriptors)
                Validate(descriptor, context.AspectContext.ServiceProvider);
        }

        private void Validate(DataValidationDescriptor dataValidationDescriptor, IServiceProvider serviceProvider)
        {
            var skip = dataValidationDescriptor.Attributes.FirstOrDefault(x => x is SkipValidationAttribute);
            if (skip != null)
            {
                dataValidationDescriptor.State = DataValidationState.Skipped;
                return;
            }
            dataValidationDescriptor.State = DataValidationState.Valid;   
            foreach (var attribute in dataValidationDescriptor.Attributes)
            {
                if (attribute is ValidationAttribute validation)
                {
                    var validationContext = new ValidationContext(dataValidationDescriptor.Value, serviceProvider, null)
                    {
                        MemberName = dataValidationDescriptor.MemberName,
                        DisplayName = dataValidationDescriptor.DisplayName
                    };
                    var result = validation.GetValidationResult(dataValidationDescriptor.Value, validationContext);
                    if (result != ValidationResult.Success)
                    {
                        dataValidationDescriptor.State = DataValidationState.Invalid;
                        dataValidationDescriptor.Errors.Add(new DataValidationError(dataValidationDescriptor.MemberName, result.ErrorMessage));
                    }
                }
            }
        }
    }
}
