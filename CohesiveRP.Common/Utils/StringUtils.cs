using System.Text.RegularExpressions;

namespace CohesiveRP.Common.Utils
{
    public static class StringUtils
    {
        public static string RemoveXmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Replace closing block-level tags with a newline before removing them
            string result = Regex.Replace(input, @"<(/?(br|hr|p|div|li|tr|h[1-6]|blockquote|pre|header|footer|section|article)(\s[^>]*)?)>",
                m =>
                {
                    string tagName = m.Groups[2].Value.ToLower();
                    // Self-closing or opening <br>/<hr> → newline
                    // Closing tags of block elements → newline
                    bool isSelfClosing = tagName is "br" or "hr";
                    bool isClosing = m.Value.StartsWith("</");
                    return (isSelfClosing || isClosing) ? "\n" : string.Empty;
                },
                RegexOptions.IgnoreCase);

            // Remove all remaining tags
            result = Regex.Replace(result, "<[^>]+>", string.Empty);

            // Decode common HTML entities
            result = result
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'");

            // Collapse multiple consecutive blank lines into one
            result = Regex.Replace(result, @"\n{3,}", "\n\n");

            // Trim trailing spaces on each line
            result = string.Join("\n", result.Split('\n').Select(line => line.Trim()));

            return result.Trim();
        }
    }
}
