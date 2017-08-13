using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector
    {
        private class OpenGenericPropertyReflector : PropertyReflector
        {
            public OpenGenericPropertyReflector(PropertyInfo reflectionInfo) : base(reflectionInfo)
            {
            }

            public override object GetValue(object instance) => throw new InvalidOperationException("Late bound operations cannot be performed on property with types for which Type.ContainsGenericParameters is true");

            public override void SetValue(object instance, object value) => throw new InvalidOperationException("Late bound operations cannot be performed on property with types for which Type.ContainsGenericParameters is true");

            public override object GetStaticValue() => throw new InvalidOperationException("Late bound operations cannot be performed on property with types for which Type.ContainsGenericParameters is true");

            public override void SetStaticValue(object value) => throw new InvalidOperationException("Late bound operations cannot be performed on property with types for which Type.ContainsGenericParameters is true");
        }
    }
}
