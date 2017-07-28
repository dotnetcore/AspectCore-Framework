using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector
    {
        private class StaticPropertyReflector : PropertyReflector
        {
            public StaticPropertyReflector(PropertyInfo reflectionInfo)
                : base(reflectionInfo, CallOptions.Call)
            {
            }

            public override object GetValue(object instance)
            {
                CheckGetReflector();
                return _getMethodReflector.StaticInvoke();
            }

            public override void SetValue(object instance, object value)
            {
                CheckSetReflector();
                _setMethodReflector.StaticInvoke(value);
            }

            public override object GetStaticValue()
            {
                CheckGetReflector();
                return _getMethodReflector.StaticInvoke();
            }

            public override void SetStaticValue(object value)
            {
                CheckSetReflector();
                _setMethodReflector.StaticInvoke(value);
            }
        }
    }
}