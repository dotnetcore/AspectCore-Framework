using System;

namespace AspectCore.Extensions.Configuration
{
    /// <summary>
    /// 基于对象绑定的配置绑定特性
    /// </summary>
    public class ConfigurationBindingAttribute : ConfigurationMetadataAttribute
    {
        /// <summary>
        /// 对应的配置节
        /// </summary>
        public override string[] Sections { get; }

        /// <summary>
        /// 键
        /// </summary>
        public override string Key { get; } = null;

        /// <summary>
        /// 配置绑定的类型
        /// </summary>
        public override ConfigurationBindType Type { get; } = ConfigurationBindType.Class;

        /// <summary>
        /// 基于对象绑定的配置绑定特性
        /// </summary>
        /// <param name="sections">配置节</param>
        public ConfigurationBindingAttribute(params string[] sections)
        {
            Sections = sections;
        }
    }
}