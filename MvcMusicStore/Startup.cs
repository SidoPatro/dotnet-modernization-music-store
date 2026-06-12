using Microsoft.Extensions.Configuration;

namespace MvcMusicStore
{
    /// <summary>
    /// Static configuration manager retained for legacy controller access to IConfiguration.
    /// Populated during application startup in Program.cs.
    /// </summary>
    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; }
    }
}
