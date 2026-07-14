using AspectCore.Extensions.Configuration;
using Microsoft.Extensions.Configuration;

namespace AspectCoreTest.Configuration.E2E
{
    /// <summary>
    /// Service that receives a configuration section via ConfigurationBinding.
    /// Used with the built-in container (no proxy needed for attribute injection).
    /// </summary>
    public interface IConfiguredAppService
    {
        string GetAppName();
        string GetVersion();
    }

    public class ConfiguredAppService : IConfiguredAppService
    {
        [ConfigurationBinding("app")]
        private readonly AppConfig _config = default!;

        public string GetAppName() => _config.Name;

        public string GetVersion() => _config.Version;
    }

    public class AppConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service that receives individual configuration values via ConfigurationValue.
    /// Used with the built-in container (no proxy needed for attribute injection).
    /// </summary>
    public interface IDbConfigService
    {
        string GetConnectionString();
        int GetTimeout();
    }

    public class DbConfigService : IDbConfigService
    {
        [ConfigurationValue("connectionString", "db")]
        private readonly string _connectionString = default!;

        [ConfigurationValue("timeout", "db")]
        private readonly int _timeout = default!;

        public string GetConnectionString() => _connectionString;
        public int GetTimeout() => _timeout;
    }

    /// <summary>
    /// Service that uses both ConfigurationBinding and ConfigurationValue.
    /// Used with the built-in container (no proxy needed for attribute injection).
    /// </summary>
    public interface IFeatureService
    {
        string GetName();
        int GetMaxRetries();
    }

    public class FeatureService : IFeatureService
    {
        [ConfigurationBinding("feature")]
        private readonly FeatureConfig _config = default!;

        [ConfigurationValue("maxRetries", "feature")]
        private readonly int _maxRetries = default!;

        public string GetName() => _config.Name;
        public int GetMaxRetries() => _maxRetries;
    }

    public class FeatureConfig
    {
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Service that receives configuration through constructor injection.
    /// Used with the MS DI container with proxy generation.
    /// </summary>
    public interface IProxiedConfigService
    {
        string GetAppName();
        string GetVersion();
        string GetConnectionString();
    }

    public class ProxiedConfigService : IProxiedConfigService
    {
        private readonly IConfiguration _configuration;

        public ProxiedConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetAppName() => _configuration["app:name"] ?? "unknown";

        public string GetVersion() => _configuration["app:version"] ?? "unknown";

        public string GetConnectionString() => _configuration["db:connectionString"] ?? "unknown";
    }
}
