using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;
using CohesiveRP.Core.WebApi.ResponseDtos;
using CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class ImportAndReplacePersonaAvatarWorkflow : IImportAndReplacePersonaAvatarWorkflow
{
    private IStorageService storageService;

    public ImportAndReplacePersonaAvatarWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> ImportAvatarAsync(ImportAndReplacePersonaAvatarRequestDto requestDto)
    {
        requestDto = requestDto ?? throw new ArgumentNullException(nameof(requestDto));
        ArgumentNullException.ThrowIfNull(requestDto.File);
        ArgumentNullException.ThrowIfNull(requestDto.PersonaId);

        // Save the image (avatar) on disk
        string directoryPersona = Path.Combine($"../CohesiveRP.UI.WebFrontend/public", "personas", requestDto.PersonaId);
        if (!Directory.Exists(directoryPersona))
        {
            Directory.CreateDirectory(directoryPersona);
        }

        using Stream stream = requestDto.File.OpenReadStream();
        using Image image = await Image.LoadAsync(stream);
        image?.Save(Path.Combine(directoryPersona, "avatar.png"));

        return new BasicResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
        };
    }
}
