using System.Text.RegularExpressions;

namespace CohesiveRP.Common.Utils
{
    public static class AvatarUtils
    {
        public static string GetSeedFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var match = Regex.Match(fileName, @"(?<=_s_)\d+(?=_)");

            if (match.Success)
            {
                return match.Value;
            }

            return null;
        }
    }
}
