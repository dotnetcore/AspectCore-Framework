using System;
using System.Linq;

namespace AspectCore.Extensions.Configuration
{
   /// <summary>
    /// 配置元数据信息特性
    /// </summary>
    public abstract class ConfigurationMetadataAttribute : Attribute, IConfigurationMetadataProvider
    {
         /// <summary>
        /// 配置节
        /// </summary>
        public abstract string[] Sections { get; }

        /// <summary>
        /// 键
        /// </summary>
        public abstract string Key { get; }

        /// <summary>
        /// 配置绑定类型
        /// </summary>
        public abstract ConfigurationBindType Type { get; }

        /// <summary>
        /// 获取字符串格式的配置节
        /// </summary>
        /// <returns>字符串格式的配置节</returns>
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