using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos;
using CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ImportAndReplaceLorebookAvatarWorkflow : IImportAndReplaceLorebookAvatarWorkflow
{
    private IStorageService storageService;

    public ImportAndReplaceLorebookAvatarWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> ImportAvatarAsync(ImportAndReplaceLorebookAvatarRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.File);
        ArgumentNullException.ThrowIfNull(requestDto.LorebookId);

        // Save the image (avatar) on disk
        string directoryLorebook = Path.Combine(WebConstants.LorebooksAvatarFilePath, requestDto.LorebookId);
        if (!Directory.Exists(directoryLorebook))
        {
            Directory.CreateDirectory(directoryLorebook);
        }

        using Stream stream = requestDto.File.OpenReadStream();
        using Image image = await Image.LoadAsync(stream);
        image?.Save(Path.Combine(directoryLorebook, WebConstants.AvatarFileName));

        return new BasicResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };
    }
}
