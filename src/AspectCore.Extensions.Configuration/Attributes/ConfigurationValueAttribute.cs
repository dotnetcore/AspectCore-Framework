using System;

namespace AspectCore.Extensions.Configuration
{
    /// <summary>
    /// 基于值绑定的配置绑定特性
    /// </summary>
    public class ConfigurationValueAttribute : ConfigurationMetadataAttribute
    {
        /// <summary>
        /// 键
        /// </summary>
        public override string Key { get; }

        /// <summary>
        /// 配置绑定类型
        /// </summary>
        public override ConfigurationBindType Type { get; } = ConfigurationBindType.Value;

        /// <summary>
        /// 配置节
        /// </summary>
        public override string[] Sections { get; }

        /// <summary>
        /// 基于值绑定的配置绑定特性
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="sections">配置节</param>
        public ConfigurationValueAttribute(string key, params string[] sections)
        {
            Key = key;
            Sections = sections;
        }
    }
}