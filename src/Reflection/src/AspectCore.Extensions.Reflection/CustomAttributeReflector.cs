using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection
{
    public sealed class CustomAttributeReflector
    {
        private readonly CustomAttributeData _customAttributeData;

        public Type AttributeType { get; set; }


        private CustomAttributeReflector(CustomAttributeData reflectionInfo)
        {
            _customAttributeData = reflectionInfo;
        }

        //public object Invoke()
        //{

        //}
    }
}
