using System.Text.RegularExpressions;

namespace CohesiveRP.Common.Utils.Parsers
{
    public static class ChatMessageParserUtils
    {
        public static string ParseMessage(string rawMessage)
        {
            string message = rawMessage;

            // remove <think></think>
            message = Regex.Replace(message, @"(?s)<think>.*?</think>", "");

            // remove <thinking></thinking>
            message = Regex.Replace(message, @"(?s)<thinking>.*?</thinking>", "");

            // TODO: add regexes as needed, could also most likely allow custom regexes (from globalSettings, passed in params)

            return message;
        }

        public static string ParseThinking(string content)
        {
            string message = content;
            var pattern = @"(?s)<think>(.*?)</think>";

            var insideText = string.Concat(
                Regex.Matches(message, pattern)
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
            );

            return message;
        }
    }
}
