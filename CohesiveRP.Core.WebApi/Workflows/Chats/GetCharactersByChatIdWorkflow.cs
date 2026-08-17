using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.Utils;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Utils.Characters;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters.BusinessObjects;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Chats.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Core.WebApi.Workflows.Chats;

public class GetCharactersByChatIdWorkflow : IGetCharactersByChatIdWorkflow
{
    private IStorageService storageService;

    public GetCharactersByChatIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetCharactersByChatIdAsync(string chatId)
    {
        var chat = await storageService.GetChatAsync(chatId);

        if (chat == null)
        {
            LoggingManager.LogToFile("7b73209e-2153-44bf-b1eb-968e4d75724d", $"Couldn't get chat from id [{chatId}]. Chat was not found.");
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Chat not found."
            };
        }

        var allCharacters = await storageService.GetCharactersAsync();
        var charactersTiedToChat = allCharacters.Where(w => chat.CharacterIds != null && chat.CharacterIds.Contains(w.CharacterId)).ToList();

        if (charactersTiedToChat == null || charactersTiedToChat.Count <= 0)
        {
            return new CharactersResponseDto
            {
                HttpResultCode = System.Net.HttpStatusCode.OK,
                Characters = new List<CharacterResponse>()
            };
        }

        var responseDto = new CharactersResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            // TODO: pagination instead of take(512)
            Characters = charactersTiedToChat.Take(512).Select(s => new CharacterResponse
            {
                CharacterId = s.CharacterId,
                Name = s.Name,
                CreatedAtUtc = s.CreatedAtUtc,
                Creator = s.Creator,
                CreatorNotes = s.CreatorNotes,
                Description = s.Description,
                IncludeDescriptionInPrompt = s.IncludeDescriptionInPrompt,
                Tags = s.Tags,
                FirstMessage = s.FirstMessage,
                AlternateGreetings = s.AlternateGreetings,
                LastActivityAtUtc = s.LastActivityAtUtc,
                ImageGenerationConfiguration = new CharacterImageGenerationConfiguration()
                {
                    IllustratorTag = s.ImageGenerationConfiguration?.IllustratorTag,
                    IllustrationMapOutfits = s.ImageGenerationConfiguration?.IllustrationMapOutfits?.Select(imo => new IllustrationMapOutfit
                    {
                        IllustratorPromptInjection = imo?.IllustratorPromptInjection,
                        Outfit = imo?.Outfit ?? ClothingStateOfDress.Clothed,
                        SourceAvatars = CharacterAvatarsUtils.GetCharacterSourceAvatars(s, imo?.Outfit ?? ClothingStateOfDress.Clothed),
                        ExpressionAvatars = CharacterAvatarsUtils.GetCharacterExpressionAvatars(s, imo?.Outfit ?? ClothingStateOfDress.Clothed),
                    }).ToList()
                },
            }).OrderByDescending(o => o.LastActivityAtUtc).ToList()
        };

        return responseDto;
    }
}
