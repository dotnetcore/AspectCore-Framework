using System;

namespace AspectCore.Extensions.Configuration
{
    public class ConfigurationBindingAttribute : ConfigurationMetadataAttribute
    {
        public override string[] Sections { get; }

        public override string Key { get; } = null;

        public override ConfigurationBindType Type { get; } = ConfigurationBindType.Class;

        public ConfigurationBindingAttribute(params string[] sections)
        {
            Sections = sections;
        }
    }
}