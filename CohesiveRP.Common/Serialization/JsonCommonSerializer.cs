using System.Text.Json;
using CohesiveRP.Common.Diagnostics;

namespace CohesiveRP.Common.Serialization
{
    public static class JsonCommonSerializer
    {
        // ********************************************************************
        //                            Public
        // ********************************************************************
        public static T DeserializeFromString<T>(string aSerializedObjectToDeserialize) // where T : ISerializable
        {
            try
            {
                return JsonSerializer.Deserialize<T>(aSerializedObjectToDeserialize, Settings);
            } catch (Exception _Ex)
            {
                LoggingManager.LogToFile("7738DC29-7E5D-4559-B6E3-C07D8B2A5717", $"Couldn't deserialize object [{typeof(T)}]", _Ex);
                throw;
            }
        }

        public static string SerializeToString<T>(T aObjectToSerialize) // where T : ISerializable
        {
            try
            {
                return JsonSerializer.Serialize(aObjectToSerialize, Settings);
            } catch (Exception _Ex)
            {
                LoggingManager.LogToFile("00967675-8EA2-48C8-9AEF-066156DD9D23", $"Couldn't serialize object [{typeof(T)}]", _Ex);
                throw;
            }
        }

        // ********************************************************************
        //                            Properties
        // ********************************************************************
        public static JsonSerializerOptions Settings => new()
        {
            AllowTrailingCommas = true,
        };
    }
}
