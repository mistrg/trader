using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;

namespace Trader.Infrastructure
{
    public class KeyVaultCache
    {

        private string _keyVaultUrl = "https://drborkeyvault.vault.azure.net/";
        private SecretClient _KeyVaultClient = null;
        private ClientSecretCredential _credential = new ClientSecretCredential("3cb8a240-1dc1-48af-b2c4-d02b7a18a2ce","864520dd-bf36-4575-84a4-cb2857dbb751","Wj1VRousNxE.PPHaK09UOIJocsz0hwNUW1");



        private  Dictionary<string, string> SecretsCache = new Dictionary<string, string>();

        public string GetCachedSecret(string secretName)
        {
            if (!SecretsCache.ContainsKey(secretName))
            {
                if (_KeyVaultClient is null)
                {
                    _KeyVaultClient = new SecretClient(vaultUri: new Uri(_keyVaultUrl), _credential);
                }
                KeyVaultSecret secret = _KeyVaultClient.GetSecret(secretName);

                SecretsCache.Add(secretName, secret.Value);
            }

            return SecretsCache.ContainsKey(secretName) ? SecretsCache[secretName] : string.Empty;
        }
    }
}