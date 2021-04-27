namespace AspectCore.Extensions.Configuration
{
    /// <summary>
    /// 配置元数据提供者
    /// </summary>
    public interface IConfigurationMetadataProvider
    {
        /// <summary>
        /// 配置节
        /// </summary>
        string[] Sections { get; }
        
        /// <summary>
        /// 键
        /// </summary>
        string Key { get; }
        
        /// <summary>
        /// 配置绑定类型
        /// </summary>
        ConfigurationBindType Type { get; }
    }
}