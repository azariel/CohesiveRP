using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Utils.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetCharacterByIdWorkflow : IGetCharacterByIdWorkflow
{
    private IStorageService storageService;

    public GetCharacterByIdWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetCharacterByIdAsync(string characterId)
    {
        CharacterDbModel character = await storageService.GetCharacterByIdAsync(characterId);

        if(character == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Character with id {characterId} was not found."
            };
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Character = new CharacterResponse
            {
                CharacterId = character.CharacterId,
                Name = character.Name,
                Creator = character.Creator,
                CreatorNotes = character.CreatorNotes,
                Description = character.Description,
                Tags = character.Tags,
                FirstMessage = character.FirstMessage,
                AlternateGreetings = character.AlternateGreetings,
                LastActivityAtUtc = character.LastActivityAtUtc,
                CreatedAtUtc = character.CreatedAtUtc,
                ImageGenerationConfiguration = new CharacterImageGenerationConfiguration()
                {
                    IllustratorTag = character.ImageGenerationConfiguration?.IllustratorTag,
                    IllustrationMapOutfits = character.ImageGenerationConfiguration?.IllustrationMapOutfits?.Select(imo => new IllustrationMapOutfit
                    {
                        IllustratorPromptInjection = imo?.IllustratorPromptInjection,
                        Outfit = imo?.Outfit ?? ClothingStateOfDress.Clothed,
                        SourceAvatars = CharacterAvatarsUtils.GetCharacterSourceAvatars(character, imo?.Outfit ?? ClothingStateOfDress.Clothed),
                    }).ToList()
                },
            }
        };

        return responseDto;
    }
}
