namespace AspectCore.Extensions.Configuration
{
    /// <summary>
    /// 提供配置绑定的元数据信息
    /// </summary>
    public interface IConfigurationMetadataProvider
    {
        /// <summary>
        /// 绑定的配置节
        /// </summary>
        string[] Sections { get; }
        
        /// <summary>
        /// 键，值类型的绑定可用
        /// </summary>
        string Key { get; }
        
        /// <summary>
        /// 配置绑定类型
        /// </summary>
        ConfigurationBindType Type { get; }
    }
}