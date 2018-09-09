using System;

namespace AspectCore.Extensions.Configuration
{
    public class ConfigurationValue :Attribute
    {
        public string Key { get; }

        public ConfigurationValue(string key)
        {
            Key = key;
        }
    }
}