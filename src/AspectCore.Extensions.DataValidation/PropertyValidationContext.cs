using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    public sealed class PropertyValidationContext
    {
        public PropertyMetaData PropertyMetaData { get; }

        public AspectContext AspectContext { get; }

        public PropertyValidationContext(PropertyMetaData propertyMetaData, AspectContext aspectContext)
        {
            PropertyMetaData = propertyMetaData;
            AspectContext = aspectContext;
        }
    }
}