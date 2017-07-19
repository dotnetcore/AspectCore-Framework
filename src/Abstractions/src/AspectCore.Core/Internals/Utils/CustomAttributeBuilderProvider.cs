using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Core.Internal
{
    public sealed class CustomAttributeBuilderProvider
    {
        private readonly CustomAttributeData _customAttributeData;

        public CustomAttributeBuilderProvider(CustomAttributeData customAttributeData)
        {
            _customAttributeData = customAttributeData ?? throw new ArgumentNullException(nameof(customAttributeData));
        }

        public CustomAttributeBuilder CustomAttributeBuilder
        {
            get
            {
                if (_customAttributeData.NamedArguments != null)
                {
                    var attributeTypeInfo = _customAttributeData.AttributeType.GetTypeInfo();
                    var constructor = _customAttributeData.Constructor;
                    var constructorArgs = _customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray();
                    var namedProperties = _customAttributeData.NamedArguments
                            .Where(n => !n.IsField)
                            .Select(n => attributeTypeInfo.GetProperty(n.MemberName))
                            .ToArray();
                    var propertyValues = _customAttributeData.NamedArguments
                             .Where(n => !n.IsField)
                             .Select(n => n.TypedValue.Value)
                             .ToArray();
                    var namedFields = _customAttributeData.NamedArguments.Where(n => n.IsField)
                             .Select(n => attributeTypeInfo.GetField(n.MemberName))
                             .ToArray();
                    var fieldValues = _customAttributeData.NamedArguments.Where(n => n.IsField)
                             .Select(n => n.TypedValue.Value)
                             .ToArray();
                    return new CustomAttributeBuilder(_customAttributeData.Constructor, constructorArgs
                       , namedProperties
                       , propertyValues, namedFields, fieldValues);
                }
                else
                {
                    return new CustomAttributeBuilder(_customAttributeData.Constructor,
                        _customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray());
                }
            }
        }
    }
}
