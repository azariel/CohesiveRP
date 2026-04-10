using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCharacterRolls.BusinessObjects;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets;
using CohesiveRP.Core.WebApi.Workflows.ChatCharacterRolls.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.ChatCharacterRolls;

public class GetChatCharacterRollsWorkflow : IChatCharacterRollsWorkflow
{
    private IStorageService storageService;

    public GetChatCharacterRollsWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetChatCharacterRolls(string chatId)
    {
        var rolls = await storageService.GetChatCharactersRollsByIdAsync(chatId);

        if (rolls == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Chat characters rolls for chatId {chatId} was not found."
            };
        }

        var responseDto = new ChatCharacterRollsResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Rolls = [],
        };

        if (rolls?.ChatCharactersRolls != null)
        {
            var characterSheetInstances = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatId);
            foreach (var characterRollObj in rolls.ChatCharactersRolls)
            {
                var characterSheetInstance = characterSheetInstances?.CharacterSheetInstances?.FirstOrDefault(c => c.CharacterSheetInstanceId == characterRollObj.CharacterSheetInstanceId);
                if (characterSheetInstance == null)
                    continue;

                ChatCharacterRollResponse rollResponse = new ChatCharacterRollResponse
                {
                    CharacterId = characterSheetInstance.CharacterId,
                    CharacterName = $"{characterSheetInstance.CharacterSheet?.FirstName} {characterSheetInstance.CharacterSheet?.LastName}".Trim(),
                    Rolls = [],
                };

                foreach (var roll in characterRollObj.Rolls)
                {
                    var newRoll = new ChatCharacterRoll
                    {
                        Value = roll.Value,
                        Reasonings = roll.Reasonings,
                        ActionCategory = roll.ActionCategory,
                        CharactersInSceneWithCounterRolls = [],
                    };

                    foreach (var charactersWithCounterRoll in roll.CharactersInScene)
                    {
                        if (charactersWithCounterRoll?.CharacterInSceneCounterRoll == null)
                            continue;

                        var otherCharacterSheetInstance = characterSheetInstances?.CharacterSheetInstances?.FirstOrDefault(c => c.CharacterSheetInstanceId == charactersWithCounterRoll.CharacterSheetInstanceId);
                        if (otherCharacterSheetInstance == null)
                            continue;

                        var characterInSceneRoll = new ChatCharacterInSceneCounterRolls
                        {
                            CharacterId = otherCharacterSheetInstance.CharacterId,
                            CharacterName = $"{otherCharacterSheetInstance.CharacterSheet?.FirstName} {otherCharacterSheetInstance.CharacterSheet?.LastName}".Trim(),
                            CharacterInSceneCounterRoll = new()
                            {
                                Attribute = charactersWithCounterRoll.CharacterInSceneCounterRoll.Attribute,
                                Value = charactersWithCounterRoll.CharacterInSceneCounterRoll.Value,
                            }
                        };

                        newRoll.CharactersInSceneWithCounterRolls.Add(characterInSceneRoll);
                    }

                    rollResponse.Rolls.Add(newRoll);
                }

                responseDto.Rolls = responseDto.Rolls.Append(rollResponse).ToArray();
            }
        }

        return responseDto;
    }
}
