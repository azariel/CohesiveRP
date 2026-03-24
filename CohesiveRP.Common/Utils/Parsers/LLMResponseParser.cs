using System.Text.RegularExpressions;
using CohesiveRP.Common.Serialization;

namespace CohesiveRP.Common.Utils.Parsers
{
    public static class LLMResponseParser
    {
        public static T ParseOnlyJson<T>(string LLMrawResponse)
        {
            try
            {
                // Use a regex to find the array and remove everything that is inconsequential
                var match = Regex.Match(LLMrawResponse, @"\{.*\}", RegexOptions.Singleline);
                string result = match.Value;

                return JsonCommonSerializer.DeserializeFromString<T>(result);
            } catch (Exception)
            {
                return JsonCommonSerializer.DeserializeFromString<T>(LLMrawResponse);
            }
        }

        public static T[] ParseOnlyJsonArray<T>(string LLMrawResponse)
        {
            try
            {
                // Use a regex to find the array and remove everything that is inconsequential
                var match = Regex.Match(LLMrawResponse, @"\[.*\]", RegexOptions.Singleline);
                string result = match.Value;

                return JsonCommonSerializer.DeserializeFromString<T[]>(result);
            } catch (Exception)
            {
                return JsonCommonSerializer.DeserializeFromString<T[]>(LLMrawResponse);
            }
        }
    }
}
