using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection
{
    public sealed class CustomAttributeReflector
    {
        private readonly CustomAttributeData _customAttributeData;
        private readonly Func<object> _invoker;

        public Type AttributeType { get;}

        private CustomAttributeReflector(CustomAttributeData customAttributeData)
        {
            _customAttributeData = customAttributeData ?? throw new ArgumentNullException(nameof(customAttributeData));
            AttributeType = _customAttributeData.AttributeType;
        }

        private Func<object> CreateInvoker()
        {
            var dynamicMethod = new DynamicMethod("CustomAttributeInvoker", typeof(object), null, AttributeType.GetTypeInfo().Module, true);
            var ilGen = dynamicMethod.GetILGenerator();


            ilGen.Emit(OpCodes.Newobj, _customAttributeData.Constructor);

            ilGen.Emit(OpCodes.Ret);
            return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
        }

        public object Invoke()
        {
            return _invoker();
        }

        public CustomAttributeData GetCustomAttributeData()
        {
            return _customAttributeData;
        }
    }
}
