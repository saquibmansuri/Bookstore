using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Amazon;
using System.Text.Json;
using Acme.BookStore.DataTransferObjects;

namespace Acme.BookStore.SharedServices
{
    public class AwsSecretsManagerService
    {
        public async Task<string> GetSecretAsync(IConfiguration configuration)
        {
            var environment = Environment.GetEnvironmentVariable("RUNNING_ENVIRONMENT");
            if (!string.IsNullOrEmpty(environment) && environment.Equals("Local", StringComparison.InvariantCultureIgnoreCase))
            {
                return configuration.GetConnectionString("Default") ?? "";
            }
            string secretName = "Database-Secret";
            string region = "us-east-1";
            using IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };
            GetSecretValueResponse response = await client.GetSecretValueAsync(request);
            var secretString = response.SecretString;
            var secrets = JsonSerializer.Deserialize<DatabaseSecrets>(secretString!);
            await Console.Out.WriteLineAsync(secretString);
            await Console.Out.WriteLineAsync(secrets!.GetConnectionString());
            return secrets!.GetConnectionString();
        }
    }
}
