using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards;
using CohesiveRP.Core.CharacterCards.Loaders;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.QueryModels.Chat;
using SixLabors.ImageSharp;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ImportNewCharacterWorkflow : IImportNewCharacterWorkflow
{
    private IStorageService storageService;

    public ImportNewCharacterWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
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
        };

        var result = await storageService.ImportNewCharacterAsync(queryModel);

        if (result == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Couldn't import character. Check server logs for mor information."
            };
        }

        //// Convert DbModel to an acceptable web model (without sensitive information)
        //var responseDto = new MessageResponseDto
        //{
        //    HttpResultCode = System.Net.HttpStatusCode.OK,
        //    Message = new MessageDefinition
        //    {
        //        MessageId = message.MessageId,
        //        Summarized = message.Summarized,
        //        Content = message.Content,
        //    },
        //    MainQueryId = backgroundQuery.BackgroundQueryId,
        //};

        //return responseDto;
        return null;
    }
}
