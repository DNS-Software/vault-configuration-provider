using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DNS.VaultConfigurationProvider
{
    public static class ConfigurationExtensions
    {
        public static IHostingEnvironment Environment;

        public static void AddVaultSecrets(this IConfigurationBuilder builder, IHostingEnvironment environment)
        {
            Environment = environment;
            builder.Add(new VaultConfigurationProvider());
        }
    }

}
