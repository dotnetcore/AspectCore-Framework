using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    public partial class ConstructorReflector
    {
        private class OpenGenericConstructorReflector : ConstructorReflector
        {
            public OpenGenericConstructorReflector(ConstructorInfo constructorInfo) : base(constructorInfo)
            {
            }

            protected override Func<object[], object> CreateInvoker() => null;

            public override object Invoke(params object[] args) => throw new InvalidOperationException($"Cannot create an instance of {_reflectionInfo.DeclaringType} because Type.ContainsGenericParameters is true.");
        }
    }
}
