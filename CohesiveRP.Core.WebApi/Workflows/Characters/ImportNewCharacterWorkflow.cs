using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards;
using CohesiveRP.Core.CharacterCards.Loaders;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using CohesiveRP.Core.DtoConverters.Abstractions;
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

        // Validate that the character doesn't already exists
        string characterName = characterCardCRPv1?.Data?.Character?.Name ?? characterCardCCv3?.Data?.Name ?? "[Unknown]";
        var allCharacters = await storageService.GetCharactersAsync();
        if (allCharacters.Any(a => a.Name == characterName))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Found character with name [{characterName}], but this character is already present in storage. Please remove that character before re-importing it."
            };
        }

        // Add the character
        AddCharacterQueryModel queryModel = null;

        if (characterCardCRPv1?.Data?.Character != null)
        {
            queryModel = new()
            {
                Name = characterCardCRPv1.Data.Character.Name.Trim(),
                Creator = characterCardCRPv1.Data.Character.Creator,
                CreatorNotes = characterCardCRPv1.Data.Character.CreatorNotes,
                Description = characterCardCRPv1.Data.Character.Description,
                Tags = characterCardCRPv1.Data.Character.Tags,
                FirstMessage = characterCardCRPv1.Data.Character.FirstMessage,
                AlternateGreetings = characterCardCRPv1.Data.Character.AlternateGreetings,
            };
        } else if (characterCardCCv3?.Data != null)
        {
            queryModel = new()
            {
                Name = characterCardCCv3.Data.Name.Trim(),
                Creator = characterCardCCv3.Data.Creator,
                CreatorNotes = characterCardCCv3.Data.CreatorNotes,
                Description = characterCardCCv3.Data.Description,
                Tags = characterCardCCv3.Data.Tags,
                FirstMessage = characterCardCCv3.Data.FirstMessage,
                AlternateGreetings = characterCardCCv3.Data.AlternateGreetings,
            };
        }

        CharacterDbModel importCharacterResult = await storageService.AddCharacterAsync(queryModel);
        if (importCharacterResult == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Couldn't import character. Check server logs for mor information."
            };
        }

        // Save the characterSheet if available
        if (characterCardCRPv1?.Data?.CharacterSheet != null)
        {
            var existingCharacterSheet = await storageService.GetCharacterSheetByCharacterIdAsync(importCharacterResult.CharacterId);

            if (existingCharacterSheet == null)
            {
                // Create a new characterSheet
                var newCharacterSheet = new CharacterSheetDbModel
                {
                    CharacterSheet = characterCardCRPv1.Data.CharacterSheet,
                    CharacterId = importCharacterResult.CharacterId,
                };

                await storageService.AddCharacterSheetAsync(newCharacterSheet);
            } else
            {
                // Update an existing characterSheet
                existingCharacterSheet.CharacterSheet = characterCardCRPv1.Data.CharacterSheet;
                await storageService.UpdateCharacterSheetAsync(existingCharacterSheet);
            }
        }

        // Save the image (avatar) on disk
        string directoryCharacter = Path.Combine(WebConstants.CharactersAvatarFilePath, importCharacterResult.Name.ToLowerInvariant().Trim());
        if (!Directory.Exists(directoryCharacter))
        {
           Directory.CreateDirectory(directoryCharacter);
        }

        image?.Save(Path.Combine(directoryCharacter, WebConstants.AvatarFileName));

        // handle the embedded lorebook if any
        if (characterCardCCv3?.Data?.CharacterBook != null)
        {
            LorebookDbModel lorebookDbModel = lorebookDtoConverter.Convert(characterCardCCv3.Data.CharacterBook);
            LorebookDbModel resultLoreBook = await storageService.AddLorebookAsync(lorebookDbModel);

            // Link that lorebook to the character inherent lorebooks (the lorebooks that will be tethered to this character automatically upon new chat)
            importCharacterResult.InherentLorebookIds ??= new List<string>();
            if (!importCharacterResult.InherentLorebookIds.Contains(resultLoreBook.LorebookId))
            {
                importCharacterResult.InherentLorebookIds.Add(resultLoreBook.LorebookId);
                await storageService.UpdateCharacterAsync(importCharacterResult);
            }

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
                CreatedAtUtc = importCharacterResult.CreatedAtUtc,
            }
        };

        return responseDto;
    }
}
