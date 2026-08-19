using System.Text.RegularExpressions;

namespace CohesiveRP.Common.Utils.Parsers
{
    public static class ChatMessageParserUtils
    {
        public static string thinkingRegexPattern = @"(?s)<(think|thinking)>.*?</\1>";

        public static string ParseMessage(string rawMessage)
        {
            string message = rawMessage;

            //// remove <think></think>
            //message = Regex.Replace(message, @"(?s)<think>.*?</think>", "");

            //// remove <thinking></thinking>
            //message = Regex.Replace(message, @"(?s)<thinking>.*?</thinking>", "");

            // remove <think></think> and <thinking></thinking>
            message = Regex.Replace(message, thinkingRegexPattern, "");

            // normalize quotes
            message = message
                .Replace("“", "\"")
                .Replace("”", "\"")
                .Replace("„", "\"")
                .Replace("‟", "\"")
                .Replace("’", "'")
                .Replace("‘", "'")
                .Replace("‛", "'")
                .Replace("—", ",")
                .Replace(" ,", ",")
                .Replace(",", " , ")// To be used in concert with the one below, replace "test,blabla" with "test, blabla"
                .Replace(",  ", ", ")
                .Replace("  ,", " ,")
                .Replace(" ,", ",");

            // convert double single-quotes to double quotes
            message = Regex.Replace(message, @"''", "\"");

            // replace *** with **
            message = Regex.Replace(message, @"\*{3}", "**");

            // TODO: add regexes as needed, could also most likely allow custom regexes (from globalSettings, passed in params)

            return message;
        }

        public static string ParseThinking(string content)
        {
            var result = string.Concat(Regex.Matches(content, thinkingRegexPattern).Cast<Match>().Select(m => m.Value));
            return result;
        }
    }
}
