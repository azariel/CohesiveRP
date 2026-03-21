using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards;
using CohesiveRP.Core.CharacterCards.Loaders;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
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

public class ImportNewCharacterWorkflow : IImportNewCharacterWorkflow
{
    private IStorageService storageService;
    private ILorebookDtoConverter lorebookDtoConverter;

    public ImportNewCharacterWorkflow(IStorageService storageService, ILorebookDtoConverter lorebookDtoConverter)
    {
        this.storageService = storageService;
        this.lorebookDtoConverter = lorebookDtoConverter;
    }

    public async Task<IWebApiResponseDto> ImportNewCharacterAsync(ImportNewCharacterRequestDto requestDto)
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

        if (characterCard is not CCv3CharacterCard ccv3CharacterCard)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Unsupported format. Character card was found, but format is unsupported at the moment."
            };
        }

        // Validate that the character doesn't already exists
        var allCharacters = await storageService.GetCharactersAsync();
        if (allCharacters.Any(a => a.Name == ccv3CharacterCard.Data.Name))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Found character with name [{ccv3CharacterCard.Data.Name}], but this character is already present in storage. Please remove that character before re-importing it."
            };
        }

        // Add the character
        AddCharacterQueryModel queryModel = new()
        {
            Name = ccv3CharacterCard.Data.Name,
            Creator = ccv3CharacterCard.Data.Creator,
            CreatorNotes = ccv3CharacterCard.Data.CreatorNotes,
            Description = ccv3CharacterCard.Data.Description,
            Tags = ccv3CharacterCard.Data.Tags,
            FirstMessage = ccv3CharacterCard.Data.FirstMessage,
            AlternateGreetings = ccv3CharacterCard.Data.AlternateGreetings,
        };

        CharacterDbModel importCharacterResult = await storageService.ImportNewCharacterAsync(queryModel);

        if (importCharacterResult == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Couldn't import character. Check server logs for mor information."
            };
        }

        // Save the image (avatar) on disk
        string directoryCharacter = Path.Combine(WebConstants.CharactersAvatarFilePath, importCharacterResult.CharacterId);
        if (!Directory.Exists(directoryCharacter))
        {
            Directory.CreateDirectory(directoryCharacter);
        }

        image?.Save(Path.Combine(directoryCharacter, WebConstants.AvatarFileName));

        // handle the embedded lorebook if any
        if (ccv3CharacterCard.Data.CharacterBook != null)
        {
            LorebookDbModel lorebookDbModel = lorebookDtoConverter.Convert(ccv3CharacterCard.Data.CharacterBook);
            LorebookDbModel resultLoreBook = await storageService.AddLorebookAsync(lorebookDbModel);

            if (resultLoreBook != null)
            {
                try
                {
                    // save the image as the default lorebook avatar on disk
                    string directorylorebook = Path.Combine(WebConstants.LorebooksAvatarFilePath, resultLoreBook.LorebookId);
                    if (!Directory.Exists(directorylorebook))
                    {
                        Directory.CreateDirectory(directorylorebook);
                    }

                    image?.Save(Path.Combine(directorylorebook, WebConstants.AvatarFileName));
                } catch (Exception)
                {
                    // ignore
                }
            }
        }

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new CharacterResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Character = new()
            {
                CharacterId = importCharacterResult.CharacterId,
                Name = importCharacterResult.Name,
                Creator = importCharacterResult.Creator,
                CreatorNotes = importCharacterResult.CreatorNotes,
                Description = importCharacterResult.Description,
                Tags = importCharacterResult.Tags,
                FirstMessage = importCharacterResult.FirstMessage,
                AlternateGreetings = importCharacterResult.AlternateGreetings,
                LastActivityAtUtc = importCharacterResult.LastActivityAtUtc,
            }
        };

        return responseDto;
    }
}
