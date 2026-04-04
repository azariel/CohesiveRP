using System.Text.RegularExpressions;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils.Parsers.BusinessObjects;

namespace CohesiveRP.Common.Utils.Parsers
{
    public static class LLMResponseParser
    {
        public static string ParseOnlyJson(string LLMrawResponse)
        {
            try
            {
                // Use a regex to find the array and remove everything that is inconsequential
                var match = Regex.Match(LLMrawResponse, @"\{(?:[^{}]|\{(?:[^{}]|\{[^{}]*\})*\})*\}", RegexOptions.Singleline);
                string result = match.Value;

                return result;
            } catch (Exception)
            {
                return LLMrawResponse;
            }
        }

        public static T ParseOnlyJson<T>(string LLMrawResponse)
        {
            try
            {
                // Use a regex to find the array and remove everything that is inconsequential
                var match = Regex.Match(LLMrawResponse, @"\{(?:[^{}]|\{(?:[^{}]|\{[^{}]*\})*\})*\}", RegexOptions.Singleline);
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
                var match = Regex.Match(LLMrawResponse, @"\[(?:[^{}]|\{(?:[^{}]|\{[^{}]*\})*\})*\]", RegexOptions.Singleline);
                string result = match.Value;

                return JsonCommonSerializer.DeserializeFromString<T[]>(result);
            } catch (Exception)
            {
                return JsonCommonSerializer.DeserializeFromString<T[]>(LLMrawResponse);
            }
        }

        /// <summary>
        /// Handles API responses where the payload is an array of role/content
        /// messages and the actual JSON lives inside the "content" string value
        /// (i.e. double-serialized). Extracts the first assistant message's
        /// content, then delegates to ParseOnlyJson.
        /// </summary>
        public static T ParseFromApiMessageContent<T>(string aApiResponse)
        {
            try
            {
                // Unwrap the outer message array to get the raw content string
                var messages = JsonCommonSerializer.DeserializeFromString<List<ApiMessage>>(aApiResponse);

                ApiMessage target = messages?.FirstOrDefault();

                if (target == null || string.IsNullOrWhiteSpace(target.Content))
                    throw new InvalidOperationException("No valid assistant message found in API response.");

                return ParseOnlyJson<T>(target.Content);
            } catch (Exception e)
            {
                // ignore
            }

            return ParseOnlyJson<T>(aApiResponse);
        }
    }
}
