using System;

namespace AspectCore.Extensions.Configuration
{
    public class ConfigurationBinding : Attribute
    {
        public string Section { get; }

        public ConfigurationBinding(string section)
        {
            Section = section;
        }
    }
}