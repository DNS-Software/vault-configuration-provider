using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.GitHub;
using VaultSharp.V1.Commons;

namespace DNS.VaultConfigurationProvider
{
    public class VaultConfigurationProvider : ConfigurationProvider, IConfigurationSource
    {
        public override void Load()
        {
            var loader = new VaultSecretLoader();
            Data = Task.Run(async () => await loader.GetVaultSecrets()).Result;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new VaultConfigurationProvider();
        }

        private class VaultSecretLoader
        {
            private readonly IVaultClient _client;

            private Secret<ListInfo> _vaultKeyList;
            private Dictionary<string, Secret<SecretData>> _secretDataDictionary;
            private Dictionary<string, string> _configuration;

            public VaultSecretLoader()
            {
                var authMethod = ConfigurationExtensions.Environment.IsDevelopment()
                    ? new GitHubAuthMethodInfo(Environment.GetEnvironmentVariable("VAULT_GITHUB_AT"))
                    : new AppRoleAuthMethodInfo(Environment.GetEnvironmentVariable("VAULT_ID"),
                        Environment.GetEnvironmentVariable("VAULT_SECRET")) as IAuthMethodInfo;

                var vaultClientSettings = new VaultClientSettings
                (
                    Environment.GetEnvironmentVariable("VAULT_URL"),
                    authMethod
                );

                _client = new VaultClient(vaultClientSettings);
            }

            public async Task<Dictionary<string, string>> GetVaultSecrets()
            {
                _vaultKeyList = await _client.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync
                (
                    Environment.GetEnvironmentVariable("VAULT_APP")
                );

                _secretDataDictionary = new Dictionary<string, Secret<SecretData>>();

                foreach (var key in _vaultKeyList.Data.Keys)
                {
                    _secretDataDictionary.Add
                    (
                        key,
                        await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync
                        (
                            $"{Environment.GetEnvironmentVariable("VAULT_APP")}/{key}"
                        )
                    );
                }

                _configuration = new Dictionary<string, string>();

                foreach (var data in _secretDataDictionary)
                {
                    foreach (var secret in data.Value.Data.Data)
                    {
                        _configuration.Add($"{data.Key}:{secret.Key}", $"{secret.Value}");
                    }
                }

                return _configuration;
            }
        }
    }
}