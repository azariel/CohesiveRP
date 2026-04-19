using System.Text.RegularExpressions;

namespace CohesiveRP.Common.Utils.Parsers
{
    public static class ChatMessageParserUtils
    {
        public static string ParseMessage(string rawMessage)
        {
            string message = rawMessage;

            //// remove <think></think>
            //message = Regex.Replace(message, @"(?s)<think>.*?</think>", "");

            //// remove <thinking></thinking>
            //message = Regex.Replace(message, @"(?s)<thinking>.*?</thinking>", "");

            // remove <think></think> and <thinking></thinking>
            message = Regex.Replace(message, @"(?s)<(think|thinking)>.*?</\1>", "");

            // normalize quotes
            message = message
                .Replace("“", "\"")
                .Replace("”", "\"")
                .Replace("„", "\"")
                .Replace("‟", "\"")
                .Replace("’", "'")
                .Replace("‘", "'")
                .Replace("‚", "'")
                .Replace("‛", "'");

            // convert double single-quotes to double quotes
            message = Regex.Replace(message, @"''", "\"");

            // replace *** with **
            message = Regex.Replace(message, @"\*{3}", "**");

            // TODO: add regexes as needed, could also most likely allow custom regexes (from globalSettings, passed in params)

            return message;
        }

        public static string ParseThinking(string content)
        {
            var pattern = @"(?s)<(think|thinking)>.*?</\1>";
            var result = string.Concat(Regex.Matches(content, pattern).Cast<Match>().Select(m => m.Value));
            return result;
        }
    }
}
