using System.Text;
using System.Text.Json;

namespace CohesiveRP.Common.Serialization
{
    public static class JsonCommonSerializer
    {
        public static string EscapeControlCharsInJsonStrings(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            var sb = new StringBuilder(json.Length + 16);
            bool inString = false, escaped = false;

            foreach (char c in json)
            {
                if (!inString)
                {
                    if (c == '"') inString = true;
                    sb.Append(c);
                    continue;
                }

                if (escaped) { escaped = false; sb.Append(c); continue; }
                if (c == '\\') { escaped = true; sb.Append(c); continue; }
                if (c == '"') { inString = false; sb.Append(c); continue; }

                if (c < 0x20)
                {
                    switch (c)
                    {
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default: sb.Append($"\\u{(int)c:X4}"); break;
                    }
                } else sb.Append(c);
            }
            return sb.ToString();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public static T DeserializeFromString<T>(string aSerializedObjectToDeserialize) // where T : ISerializable
        {
            try
            {
                return TryDeserializeLenient<T>(aSerializedObjectToDeserialize, out var result) ? result : default;
                //return JsonSerializer.Deserialize<T>(aSerializedObjectToDeserialize, Settings);
            } catch (Exception _Ex)
            {
                //LoggingManager.LogToFile("7738DC29-7E5D-4559-B6E3-C07D8B2A5717", $"Couldn't deserialize object [{typeof(T)}]", _Ex);
                throw;
            }
        }

        public static string SerializeToString<T>(T aObjectToSerialize) // where T : ISerializable
        {
            try
            {
                return JsonSerializer.Serialize(aObjectToSerialize, aObjectToSerialize?.GetType() ?? typeof(object), Settings);
            } catch (Exception _Ex)
            {
                //LoggingManager.LogToFile("00967675-8EA2-48C8-9AEF-066156DD9D23", $"Couldn't serialize object [{typeof(T)}]", _Ex);
                throw;
            }
        }

        public static bool TryDeserializeLenient<T>(string json, out T result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(json, Settings);
                return true;
            } catch (JsonException)
            {
                try
                {
                    result = JsonSerializer.Deserialize<T>(EscapeControlCharsInJsonStrings(json), Settings);
                    return true;
                } catch (JsonException)
                {
                    result = default;
                    return false;
                }
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
