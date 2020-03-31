using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    [NonAspect]
    public interface IDataState
    {
        bool IsValid { get; }

        DataValidationErrorCollection Errors { get; }
    }
}