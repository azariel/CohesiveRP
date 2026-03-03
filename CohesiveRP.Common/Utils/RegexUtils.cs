using System.Text.RegularExpressions;

namespace CohesiveRP.Common.Utils
{
    public static class RegexUtils
    {
        // ********************************************************************
        //                            Public
        // ********************************************************************
        public static string GetWebSiteDomain(string completeUrl)
        {
            if (string.IsNullOrWhiteSpace(completeUrl))
                return "";

            return Regex.Match(completeUrl, "^(?:https?:\\/\\/)?(?:[^@\\n]+@)?(?:www\\.)?([^:\\/\\n?]+)").Value
                .Replace("http://www.", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("https://www.", "", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
