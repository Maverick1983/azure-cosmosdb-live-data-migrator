using System;

namespace Migration.Shared
{
    public class EnvironmentConfig
    {
        private static EnvironmentConfig singletonInstance;

        private readonly Lazy<string> tenantId;
        private readonly Lazy<string> allowedUsers;

        private EnvironmentConfig()
        {
#if DEBUG
            this.KeyVaultUri = @"https://movecosmokv5fpcjomzrqhm2.vault.azure.net/";
            this.MigrationMetadataCosmosAccountName = "movecosmonull01-cdb";
            this.DeadLetterAccountName = "movecosblob5fpcjomzrqhm2";
            this.MigrationMetadataDatabaseName = "MigrationServiceDB";
            this.MigrationMetadataContainerName = "MigrationStatus";
            this.MigrationLeasesContainerName = "Leases";
            this.AppInsightsInstrumentationKey = "f47d3896-ac7a-4a0a-ac6f-971f5de65805";
            this.DefaultSourceAccount = "eusdppcdb010";
            this.DefaultDestinationAccount = "eusdpocpcdb010";
            this.tenantId = new Lazy<string>(() => "6faf7bab-26c2-4438-a1b5-b9d9590106b6");
            this.allowedUsers = new Lazy<string>(() => "davide.sereno@external.cnhind.com | andrea.franchiolo@external.cnhind.com | luca.scida@external.cnhind.com");
#else
            this.KeyVaultUri = GetRequiredEnvironmentVariable("keyvaulturi");
            this.MigrationMetadataCosmosAccountName = GetRequiredEnvironmentVariable("cosmosdbaccount");
            this.DeadLetterAccountName = GetRequiredEnvironmentVariable("deadletteraccount");
            this.MigrationMetadataDatabaseName = GetRequiredEnvironmentVariable("cosmosdbdb");
            this.MigrationMetadataContainerName = GetRequiredEnvironmentVariable("cosmosdbcollection");
            this.MigrationLeasesContainerName = GetRequiredEnvironmentVariable("cosmosdbleasescollection");
            this.AppInsightsInstrumentationKey = GetRequiredEnvironmentVariable("appinsightsinstrumentationkey");
            this.DefaultSourceAccount = Environment.GetEnvironmentVariable("defaultsourceaccount");
            this.DefaultDestinationAccount = Environment.GetEnvironmentVariable("defaultdestinationaccount");
            this.tenantId = new Lazy<string>(() => GetRequiredEnvironmentVariable("tenantid"));
            this.allowedUsers = new Lazy<string>(() => GetRequiredEnvironmentVariable("allowedusers"));
#endif
        }

        public static EnvironmentConfig Singleton
        {
            get
            {
                if (singletonInstance == null)
                {
                    throw new InvalidOperationException("EnvironmentConfig has not yet been initialized.");
                }

                return singletonInstance;
            }
        }

        public static void Initialize()
        {
            singletonInstance = new EnvironmentConfig();
        }

        public string KeyVaultUri { get; }
        public string MigrationMetadataCosmosAccountName { get; }
        public string DeadLetterAccountName { get; }
        public string MigrationMetadataDatabaseName { get; }
        public string MigrationMetadataContainerName { get; }
        public string MigrationLeasesContainerName { get; }
        public string AppInsightsInstrumentationKey { get; }
        public string DefaultSourceAccount { get; }
        public string DefaultDestinationAccount { get; }
        public string TenantId => this.tenantId.Value;
        public string AllowedUsers => this.allowedUsers.Value;

        private static string GetRequiredEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? throw new ArgumentNullException(
                nameof(name), $"Environment variable '{name}' has not been defined.");
        }
    }
}