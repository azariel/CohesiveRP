//using CohesiveRP.Core.PromptContext.Abstractions;
//using CohesiveRP.Core.Services;
//using CohesiveRP.Storage.DataAccessLayer.AIQueries;
//using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
//using CohesiveRP.Storage.DataAccessLayer.Chats;

//namespace CohesiveRP.Core.PromptContext.Builders.Pathfinder
//{
//    public class CharacterSheetWithoutChatBuilder : IPromptContextElementBuilder
//    {
//        private IStorageService storageService;
//        private PromptContextFormatElement promptContextFormatElement;
//        private ChatDbModel chatDbModel;
//        private string characterSheetId;
//        private BackgroundQueryDbModel backgroundQuery;

//        public CharacterSheetWithoutChatBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, string characterSheetId, BackgroundQueryDbModel backgroundQuery)
//        {
//            this.storageService = storageService;
//            this.promptContextFormatElement = promptContextFormatElement;
//            this.chatDbModel = chatDbModel;
//            this.characterSheetId = characterSheetId;
//            this.backgroundQuery = backgroundQuery;
//        }

//        public async Task<(string, IShareableContextLink)> BuildAsync()
//        {
//            var characterSheets = await storageService.GetCharacterSheetsByFuncAsync(f => f.CharacterSheetId == characterSheetId);
//            var characterSheet = characterSheets?.FirstOrDefault();

//            if (characterSheet == null)
//            {
//                return (null, new ShareableContextLink { LinkedBuilder = this, });
//            }

//            return ($"",
//                new ShareableContextLink
//                {
//                    LinkedBuilder = this,
//                });
//        }
//    }
//}
