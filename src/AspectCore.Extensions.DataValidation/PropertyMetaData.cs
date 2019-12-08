using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.DataValidation
{
    public class PropertyMetaData
    {
        public Type Type { get; }

        public Attribute[] Attributes { get; }

        public object Value { get; }

        public string DisplayName { get; }

        public string MemberName { get; }

        public object Container { get; set; }

        public PropertyMetaData(PropertyInfo property, object container)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            Type = property.PropertyType;
            MemberName = property.Name;
            Attributes = property.GetReflector().GetCustomAttributes();
            var displayAttribute = Attributes.FirstOrDefault(x => x is DisplayAttribute) as DisplayAttribute;
            DisplayName = displayAttribute?.Name ?? MemberName;
            if (container != null)
                Value = property.GetReflector().GetValue(container);
            Container = container;
        }
    }
}