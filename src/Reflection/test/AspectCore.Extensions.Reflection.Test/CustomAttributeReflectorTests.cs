using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class CustomAttributeReflectorTests
    {
        [Fact]
        public void Invoke_Test()
        {
            var field = typeof(FieldFakes).GetTypeInfo().GetField("StaticFiled");
            var fieldReflector = field.GetReflector();
            var aa = fieldReflector.CustomAttributeReflectors.ToArray();
            var bb = aa[0].Invoke();
        }
    }
    public class AttributeFakes : Attribute
    {

    }
}