using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public partial class CustomAttributeReflector
    {
        private readonly CustomAttributeData _customAttributeData;
        private readonly Func<Attribute> _invoker;
        private readonly Type _attributeType;

        internal readonly HashSet<RuntimeTypeHandle> _tokens;

        public Type AttributeType => _attributeType;

        private CustomAttributeReflector(CustomAttributeData customAttributeData)
        {
            _customAttributeData = customAttributeData ?? throw new ArgumentNullException(nameof(customAttributeData));
            _attributeType = _customAttributeData.AttributeType;
            _invoker = CreateInvoker();
            _tokens = GetAttrTokens(_attributeType);
        }

        private Func<Attribute> CreateInvoker()
        {
            var dynamicMethod = new DynamicMethod($"invoker-{Guid.NewGuid()}", typeof(Attribute), null, _attributeType.GetTypeInfo().Module, true);
            var ilGen = dynamicMethod.GetILGenerator();

            foreach (var constructorParameter in _customAttributeData.ConstructorArguments)
            {
                if (constructorParameter.ArgumentType.IsArray)
                {
                    ilGen.EmitArray(((IEnumerable<CustomAttributeTypedArgument>)constructorParameter.Value).
                        Select(x => x.Value).ToArray(),
                        constructorParameter.ArgumentType.GetTypeInfo().UnWrapArrayType());
                }
                else
                {
                    ilGen.EmitConstant(constructorParameter.Value, constructorParameter.ArgumentType);
                }
            }

            var attributeLocal = ilGen.DeclareLocal(_attributeType);

            ilGen.EmitNew(_customAttributeData.Constructor);

            ilGen.Emit(OpCodes.Stloc, attributeLocal);

            var attributeTypeInfo = _attributeType.GetTypeInfo();

            foreach (var namedArgument in _customAttributeData.NamedArguments)
            {
                ilGen.Emit(OpCodes.Ldloc, attributeLocal);
                if (namedArgument.TypedValue.ArgumentType.IsArray)
                {
                    ilGen.EmitArray(((IEnumerable<CustomAttributeTypedArgument>)namedArgument.TypedValue.Value).
                        Select(x => x.Value).ToArray(),
                        namedArgument.TypedValue.ArgumentType.GetTypeInfo().UnWrapArrayType());
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
            return (Func<Attribute>)dynamicMethod.CreateDelegate(typeof(Func<Attribute>));
        }

        private HashSet<RuntimeTypeHandle> GetAttrTokens(Type attributeType)
        {
            var tokenSet = new HashSet<RuntimeTypeHandle>();
            for (var attr = attributeType; attr != typeof(object); attr = attr.GetTypeInfo().BaseType)
            {
                tokenSet.Add(attr.TypeHandle);
            }
            return tokenSet;
        }

        public Attribute Invoke()
        {
            return _invoker();
        }

        public CustomAttributeData GetCustomAttributeData()
        {
            return _customAttributeData;
        }
    }
}