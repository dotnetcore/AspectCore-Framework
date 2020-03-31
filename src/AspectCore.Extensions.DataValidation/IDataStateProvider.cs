using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    [NonAspect]
    public interface IDataStateProvider
    {
        IDataState DataState { get; set; }
    }
}