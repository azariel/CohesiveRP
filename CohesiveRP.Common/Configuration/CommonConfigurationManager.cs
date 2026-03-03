using CohesiveRP.Common.Serialization;

namespace CohesiveRP.Common.Configuration
{
    public static class CommonConfigurationManager
    {
        // ********************************************************************
        //                            Constants
        // ********************************************************************
        private const string PROJECT_CONFIG_FILE_NAME = "CohesiveRPConfig.json";

        // ********************************************************************
        //                            Private
        // ********************************************************************
        private static CohesiveRPCommonConfig fRadiantConfig;

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public static CohesiveRPCommonConfig GetConfigFromMemory() => fRadiantConfig ?? ReloadConfig();

        public static CohesiveRPCommonConfig ReloadConfig()
        {
            // Create default config if doesn't exists
            if (!File.Exists(PROJECT_CONFIG_FILE_NAME))
                SaveConfigInMemoryToDisk();

            string _ConfigFileContent = File.ReadAllText(PROJECT_CONFIG_FILE_NAME);
            fRadiantConfig = JsonCommonSerializer.DeserializeFromString<CohesiveRPCommonConfig>(_ConfigFileContent);
            return fRadiantConfig;
        }

        public static void SetConfigInMemory(CohesiveRPCommonConfig aRadiantConfig) => fRadiantConfig = aRadiantConfig;

        public static void SaveConfigInMemoryToDisk()
        {
            fRadiantConfig ??= new CohesiveRPCommonConfig();

            string _SerializedConfig = JsonCommonSerializer.SerializeToString(fRadiantConfig);
            File.WriteAllText(PROJECT_CONFIG_FILE_NAME, _SerializedConfig);
        }
    }
}
