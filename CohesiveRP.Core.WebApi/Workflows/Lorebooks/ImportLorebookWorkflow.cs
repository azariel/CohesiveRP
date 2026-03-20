using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards.Loaders;
using CohesiveRP.Core.DtoConverters.Abstractions;
using CohesiveRP.Core.Lorebooks;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos.Personas.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using SixLabors.ImageSharp;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ImportLorebookWorkflow : IImportLorebookWorkflow
{
    private IStorageService storageService;
    private ILorebookDtoConverter lorebookDtoConverter;

    public ImportLorebookWorkflow(IStorageService storageService, ILorebookDtoConverter lorebookDtoConverter)
    {
        this.storageService = storageService;
        this.lorebookDtoConverter = lorebookDtoConverter;
    }

    public async Task<IWebApiResponseDto> ImportAsync(ImportLorebookRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.File);

        bool saveImageOnDisk = false;
        using Stream stream = requestDto.File.OpenReadStream();
        ILorebook lorebook = null;
        try
        {
            using Image image = await Image.LoadAsync(stream);
            lorebook = LorebookLoader.LoadLoreBook(image);
            saveImageOnDisk = true;// Confirmation that we're handling an image, so we'll use that image as the default lorebook avatar
        } catch (Exception)
        {
            // ignore
        }

        if (lorebook == null)
        {
            // Try to load as raw file instead of an image
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            string fileContent = reader.ReadToEnd();
            lorebook = LorebookLoader.LoadLoreBook(fileContent);
        }

        if (lorebook == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"File to import did not contain a valid format embedding a lorebook."
            };
        }

        LorebookDbModel dbModel = lorebookDtoConverter.Convert(lorebook);
        LorebookDbModel result = await storageService.AddLorebookAsync(dbModel);

        if (result == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Failed to create lorebook in storage."
            };
        }

        if (saveImageOnDisk)
        {
            try
            {
                // save the image as the default lorebook avatar on disk
                string directorylorebook = Path.Combine($"../cohesiverp.ui.webfrontend/public", "lorebooks", result.LorebookId);
                if (!Directory.Exists(directorylorebook))
                {
                    Directory.CreateDirectory(directorylorebook);
                }

                stream.Seek(0, SeekOrigin.Begin);
                using Image image = await Image.LoadAsync(stream);
                image?.Save(Path.Combine(directorylorebook, "avatar.png"));
            } catch (Exception)
            {
                // ignore
            }
        }

        return new LorebookResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Lorebook = new LorebookResponse
            {
                LorebookId = result.LorebookId,
                Name = result.Name,
                LastActivityAtUtc = result.LastActivityAtUtc,
                Entries = result.Entries,
            }
        };
    }
}
