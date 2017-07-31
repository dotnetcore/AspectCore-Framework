using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public partial class CustomAttributeReflector
    {
        private readonly CustomAttributeData _customAttributeData;
        private readonly Func<object> _invoker;

        public Type AttributeType { get;}

        private CustomAttributeReflector(CustomAttributeData customAttributeData)
        {  
            _customAttributeData = customAttributeData ?? throw new ArgumentNullException(nameof(customAttributeData));
            AttributeType = _customAttributeData.AttributeType;
            _invoker = CreateInvoker();
        }

        private Func<object> CreateInvoker()
        {
            var dynamicMethod = new DynamicMethod($"invoker-{Guid.NewGuid()}", typeof(object), null, AttributeType.GetTypeInfo().Module, true);
            var ilGen = dynamicMethod.GetILGenerator();

            foreach (var constructorParameter in _customAttributeData.ConstructorArguments)
            {
                ilGen.EmitConstant(constructorParameter.Value, constructorParameter.ArgumentType);
            }

            var attributeLocal = ilGen.DeclareLocal(AttributeType);

            ilGen.EmitNew(_customAttributeData.Constructor);

            ilGen.Emit(OpCodes.Stloc, attributeLocal);

            var attributeTypeInfo = AttributeType.GetTypeInfo();

            foreach (var namedArgument in _customAttributeData.NamedArguments)
            {
                ilGen.Emit(OpCodes.Ldloc, attributeLocal);
                if (namedArgument.TypedValue.ArgumentType.IsArray)
                {
                    ilGen.EmitArray((Array)namedArgument.TypedValue.Value, namedArgument.TypedValue.ArgumentType.GetTypeInfo().UnWrapArrayType());
                }
                else
                {
                    ilGen.EmitConstant(namedArgument.TypedValue.Value, namedArgument.TypedValue.ArgumentType);
                }
                if (namedArgument.IsField)
                {
                    var field = attributeTypeInfo.GetField(namedArgument.MemberName);
                    ilGen.Emit(OpCodes.Stfld, field);
                }
                else
                {
                    var property = attributeTypeInfo.GetProperty(namedArgument.MemberName);
                    ilGen.Emit(OpCodes.Callvirt, property.SetMethod);
                }
            }
            ilGen.Emit(OpCodes.Ldloc, attributeLocal);
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
