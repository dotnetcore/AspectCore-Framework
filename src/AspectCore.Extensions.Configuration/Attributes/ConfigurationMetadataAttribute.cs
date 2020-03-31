using System;
using System.Linq;

namespace AspectCore.Extensions.Configuration
{
    public abstract class ConfigurationMetadataAttribute : Attribute, IConfigurationMetadataProvider
    {
        public abstract string[] Sections { get; }
        public abstract string Key { get; }
        public abstract ConfigurationBindType Type { get; }
        
        public string GetSection()
        {
            if (Sections == null || Sections.Length == 0)
            {
                return null;
            }

            if (Sections.Length ==1)
            {
                return Sections[0];
            }

            return Sections.Aggregate((x, y) => x + ":" + y);
        }
    }
}