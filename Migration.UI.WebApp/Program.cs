using System;
using Azure.Identity;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Migration.Shared;

namespace Migration.UI.WebApp
{
    public class Program
    {
        public const string SourceName = "MigrationUI";
        public const string WebAppUserAgentPrefix = "MigrationUI.MigrationMetadata";
        public const string SourceClientUserAgentPrefix = "MigrationUI.Source";

        public static void Main(string[] args)
        {
            try
            {
                EnvironmentConfig.Initialize();

                TelemetryConfiguration telemetryConfig = new TelemetryConfiguration(EnvironmentConfig.Singleton.AppInsightsInstrumentationKey);
                TelemetryHelper.Initilize(telemetryConfig, SourceName);
            }
            catch (Exception error)
            {
                Console.WriteLine(
                    "UNHANDLED EXCEPTION during initialization before TelemetryClient could be created: {0}",
                    error);

                throw;
            }

            try
            {
                _ = EnvironmentConfig.Singleton.TenantId;
                _ = EnvironmentConfig.Singleton.AllowedUsers;

#if DEBUG
                InteractiveBrowserCredentialOptions o = new InteractiveBrowserCredentialOptions { TenantId = "6faf7bab-26c2-4438-a1b5-b9d9590106b6" };
                KeyVaultHelper.Initialize(new Uri(EnvironmentConfig.Singleton.KeyVaultUri), new InteractiveBrowserCredential(o));
#else
                KeyVaultHelper.Initialize(new Uri(EnvironmentConfig.Singleton.KeyVaultUri), new DefaultAzureCredential());
#endif

                using (CosmosClient client =
                    KeyVaultHelper.Singleton.CreateCosmosClientFromKeyVault(
                        EnvironmentConfig.Singleton.MigrationMetadataCosmosAccountName,
                        WebAppUserAgentPrefix,
                        useBulk: false,
                        retryOn429Forever: true))
                {
                    MigrationConfigDal.Initialize(
                        client.GetContainer(
                            EnvironmentConfig.Singleton.MigrationMetadataDatabaseName,
                            EnvironmentConfig.Singleton.MigrationMetadataContainerName),
                        EnvironmentConfig.Singleton.DefaultSourceAccount,
                        EnvironmentConfig.Singleton.DefaultDestinationAccount);

                    CreateHostBuilder(args).Build().Run();
                }
            }
            catch (Exception unhandledException)
            {
                TelemetryHelper.Singleton.LogError(
                    "UNHANDLED EXCEPTION: {0}",
                    unhandledException);

                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
        }
    }
}