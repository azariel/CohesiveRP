namespace CohesiveRP.Core.PromptContext.Utils
{

    /// <summary>
    /// Handles the following macros:
    /// {{user}} => Replaces with the user's name.
    /// {{char}} => Replaces with the character's name (first one tied to the chat).
    /// TODO
    /// {{// this is a comment }}
    /// {{ random:optionA:optionB:optionC:optionD }}
    /// {{setvar::roleplay::immersive reality}}    or     {{setvar::userfix::Very important that Celia must NEVER...}}
    /// {{roll: d20}}
    /// </summary>
    public static class MacrosPromptUtils
    {
        public static string InjectMacros(this string rawText, string userName = null, string characterName = null)
        {
            string outputText = rawText;

            if(string.IsNullOrWhiteSpace(rawText))
            {
                return outputText;
            }

            if(!string.IsNullOrWhiteSpace(userName))
            {
                outputText = outputText.Replace(Constants.USER_PLACEHOLDER, userName);
            }

            if(!string.IsNullOrWhiteSpace(characterName))
            {
                outputText = outputText.Replace(Constants.CHARACTER_PLACEHOLDER, characterName);
            }

            return outputText;
        }
    }
}
