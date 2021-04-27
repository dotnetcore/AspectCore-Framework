using System;

namespace AspectCore.Extensions.Configuration
{
    /// <summary>
    /// 基于对象绑定的配置绑定特性
    /// </summary>
    public class ConfigurationBindingAttribute : ConfigurationMetadataAttribute
    {
        /// <summary>
        /// ��Ӧ�����ý�
        /// </summary>
        public override string[] Sections { get; }

        /// <summary>
        /// ��
        /// </summary>
        public override string Key { get; } = null;

        /// <summary>
        /// ���ð󶨵�����
        /// </summary>
        public override ConfigurationBindType Type { get; } = ConfigurationBindType.Class;

        /// <summary>
        /// ���ڶ���󶨵����ð�����
        /// </summary>
        /// <param name="sections">���ý�</param>
        public ConfigurationBindingAttribute(params string[] sections)
        {
            Sections = sections;
        }
    }
}