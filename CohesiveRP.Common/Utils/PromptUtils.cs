using CohesiveRP.Core;

namespace CohesiveRP.Common.Utils
{
    public static class PromptUtils
    {
        public static string ReplacePromptBasicPlaceholders(this string rawText, string characterName, string userName)
        {
            if(string.IsNullOrWhiteSpace(rawText))
            {
                return rawText;
            }

            rawText = rawText.Trim()
                .Replace(Constants.USER_PLACEHOLDER, userName)
                .Replace(Constants.CHARACTER_PLACEHOLDER, characterName);

            return rawText;
        }
    }
}
