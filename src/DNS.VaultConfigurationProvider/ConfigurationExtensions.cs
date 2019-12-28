using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DNS.VaultConfigurationProvider
{
    public static class ConfigurationExtensions
    {
        public static IHostEnvironment Environment;

        public static void AddVaultSecrets(this IConfigurationBuilder builder, IHostEnvironment environment)
        {
            Environment = environment;
            builder.Add(new VaultConfigurationProvider());
        }
    }

}
