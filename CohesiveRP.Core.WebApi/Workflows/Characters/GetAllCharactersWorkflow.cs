using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters.BusinessObjects;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.WebApi.Workflows.Chat;

public class GetAllCharactersWorkflow : IGetAllCharactersWorkflow
{
    private IStorageService storageService;

    public GetAllCharactersWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetAllCharactersAsync()
    {
        CharacterDbModel[] characters = await storageService.GetCharactersAsync();

        // Convert DbModel to an acceptable web model (without sensitive information)
        var responseDto = new CharactersResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            // TODO: pagination instead of take(256)
            Characters = characters.Take(256).Select(s => new CharacterResponse
            {
                CharacterId = s.CharacterId,
                Name = s.Name,
                Creator = s.Creator,
                CreatorNotes = s.CreatorNotes,
                Description = s.Description,
                Tags = s.Tags,
                FirstMessage = s.FirstMessage,
                AlternateGreetings = s.AlternateGreetings,
                LastActivityAtUtc = s.LastActivityAtUtc,
            }).OrderByDescending(o => o.LastActivityAtUtc).ToList()
        };

        return responseDto;
    }
}
