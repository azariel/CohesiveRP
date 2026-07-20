namespace CohesiveRP.Core
{
    public class WebConstants
    {
        public const string WebAppPublicFolder = "..\\CohesiveRP.UI.WebFrontend\\public";
        public const string CharactersAvatarFilePath = $"{WebAppPublicFolder}\\characters";
        public const string LorebooksAvatarFilePath = $"{WebAppPublicFolder}\\lorebooks";
        public const string PersonasAvatarFilePath = $"{WebAppPublicFolder}\\personas";
        public const string ChatsAvatarFilePath = $"{WebAppPublicFolder}\\chats";

        // Root avatar
        public const string AvatarFileName = "avatar.png";

        // Avatars with the original, raw, source images to use to build expressions
        public const string SourceAvatarFolder = "raws";
        public const string SourceAvatarClothedFolder = "clothed";
        public const string SourceAvatarUnderwearFolder = "underwear";
        public const string SourceAvatarNakedFolder = "naked";

        // Avatars with expressions
        public const string ExpressiveAvatarFolder = "expressions";
        public const string ExpressiveAvatarClothedFolder = "clothed";
        public const string ExpressiveAvatarUnderwearFolder = "underwear";
        public const string ExpressiveAvatarNakedFolder = "naked";
    }
}
