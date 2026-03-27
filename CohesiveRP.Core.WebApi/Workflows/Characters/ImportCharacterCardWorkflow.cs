using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards;
using CohesiveRP.Core.CharacterCards.Loaders;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using CohesiveRP.Core.DtoConverters.Abstractions;
using CohesiveRP.Core.Lorebooks;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Chat;
using SixLabors.ImageSharp;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ImportCharacterCardWorkflow : IImportCharacterCardWorkflow
{
    private IStorageService storageService;
    private ILorebookDtoConverter lorebookDtoConverter;

    public ImportCharacterCardWorkflow(IStorageService storageService, ILorebookDtoConverter lorebookDtoConverter)
    {
        this.storageService = storageService;
        this.lorebookDtoConverter = lorebookDtoConverter;
    }

    public async Task<IWebApiResponseDto> ImportCharacterAsync(string characterId, ImportNewCharacterRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.File);

        using Stream stream = requestDto.File.OpenReadStream();
        using Image image = await Image.LoadAsync(stream);
        ICharacterCard characterCard = CharacterCardLoader.LoadCharacterCard(image);

        if (characterCard == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"File to import did not contain a valid format embedding a character card."
            };
        }

        var characterCardCCv3 = characterCard as CCv3CharacterCard;
        var characterCardCRPv1 = characterCard as CohesiveRPv1CharacterCard;
        if (characterCardCCv3 == null && characterCardCRPv1 == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Unsupported format. Character card was found, but format is unsupported at the moment."
            };
        }

        // Validate that the character already exists
        var character = await storageService.GetCharacterByIdAsync(characterId);
        if (character == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Character [{character}] to update wasn't found in storage. Couldn't replace that character with the provided characterCard."
            };
        }

        // Update the character
        if (characterCardCRPv1 != null)
        {
            character.FirstMessage = characterCardCRPv1?.Data?.Character?.FirstMessage;
            character.Creator = characterCardCRPv1?.Data?.Character?.Creator;
            character.CreatorNotes = characterCardCRPv1?.Data?.Character?.CreatorNotes;
            character.AlternateGreetings = characterCardCRPv1?.Data?.Character?.AlternateGreetings;
            character.Description = characterCardCRPv1?.Data?.Character?.Description;
            character.Name = characterCardCRPv1?.Data?.Character?.Name;
            character.Tags = characterCardCRPv1?.Data?.Character?.Tags;
        } else if (characterCardCCv3 != null)
        {
            character.FirstMessage = characterCardCCv3?.Data?.FirstMessage;
            character.Creator = characterCardCCv3?.Data?.Creator;
            character.CreatorNotes = characterCardCCv3?.Data?.CreatorNotes;
            character.AlternateGreetings = characterCardCCv3?.Data?.AlternateGreetings;
            character.Description = characterCardCCv3?.Data?.Description;
            character.Name = characterCardCCv3?.Data?.Name;
            character.Tags = characterCardCCv3?.Data?.Tags;
        }

        var updateCharacterResult = await storageService.UpdateCharacterAsync(character);
        if (!updateCharacterResult)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Couldn't update character with CharacterCard. Check server logs for mor information."
            };
        }

        // Update the CharacterSheet
        if (characterCardCRPv1?.Data?.CharacterSheet != null)
        {
            var existingCharacterSheet = await storageService.GetCharacterSheetByCharacterIdAsync(character.CharacterId);

            if (existingCharacterSheet == null)
            {
                // Create a new characterSheet
                var newCharacterSheet = new CharacterSheetDbModel
                {
                    CharacterSheet = characterCardCRPv1.Data.CharacterSheet,
                };

                await storageService.AddCharacterSheetAsync(newCharacterSheet);
            } else
            {
                // Update an existing characterSheet
                existingCharacterSheet.CharacterSheet = characterCardCRPv1.Data.CharacterSheet;
                await storageService.UpdateCharacterSheetAsync(existingCharacterSheet);
            }
        }

        // Override the image (avatar) on disk
        string directoryCharacter = Path.Combine(WebConstants.CharactersAvatarFilePath, character.CharacterId);
        if (!Directory.Exists(directoryCharacter))
        {
            Directory.CreateDirectory(directoryCharacter);
        }

        image?.Save(Path.Combine(directoryCharacter, WebConstants.AvatarFileName));

        // Ignore lorebook

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Character = new()
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
            }
        };

        return responseDto;
    }
}
