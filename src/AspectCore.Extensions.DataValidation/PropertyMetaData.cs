using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 用于属性校验的元数据信息
    /// </summary>
    public class PropertyMetaData
    {
        /// <summary>
        /// 属性类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 标注在属性上的特性
        /// </summary>
        public Attribute[] Attributes { get; }

        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// 实例
        /// </summary>
        public object Container { get; set; }

        /// <summary>
        /// 用于属性校验的元数据信息
        /// </summary>
        /// <param name="property">属性对象</param>
        /// <param name="container">实例</param>
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