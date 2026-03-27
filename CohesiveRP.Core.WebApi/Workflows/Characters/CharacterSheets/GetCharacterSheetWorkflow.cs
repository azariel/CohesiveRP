using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets;

public class GetCharacterSheetWorkflow : IGetCharacterSheetWorkflow
{
    private IStorageService storageService;

    public GetCharacterSheetWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> GetCharacterSheetByCharacterIdAsync(string characterId)
    {
        CharacterSheetDbModel characterSheet = await storageService.GetCharacterSheetByCharacterIdAsync(characterId);

        if(characterSheet == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"CharacterSheet linked to characterId {characterId} was not found."
            };
        }

        return new GetCharacterSheetResponseDto
        {
            CharacterSheetId = characterSheet.CharacterSheetId,
            CharacterId = characterSheet.CharacterId,
            PersonaId = characterSheet.PersonaId,
            LastActivityAtUtc = characterSheet.LastActivityAtUtc,
            CharacterSheet = new CharacterSheet
            {
                AgeGroup = characterSheet.CharacterSheet.AgeGroup,
                Attractiveness = characterSheet.CharacterSheet.Attractiveness,
                Behavior = characterSheet.CharacterSheet.Behavior,
                BirthdayDate = characterSheet.CharacterSheet.BirthdayDate,
                BodyType = characterSheet.CharacterSheet.BodyType,
                BreastsSize = characterSheet.CharacterSheet.BreastsSize,
                ClothesPreference = characterSheet.CharacterSheet.ClothesPreference,
                CombatAffinityAttack = characterSheet.CharacterSheet.CombatAffinityAttack,
                CombatAffinityDefense = characterSheet.CharacterSheet.CombatAffinityDefense,
                Dislikes = characterSheet.CharacterSheet.Dislikes,
                EarShape = characterSheet.CharacterSheet.EarShape,
                EyeColor = characterSheet.CharacterSheet.EyeColor,
                Fears = characterSheet.CharacterSheet.Fears,
                FirstName = characterSheet.CharacterSheet.FirstName,
                Gender = characterSheet.CharacterSheet.Gender,
                Genitals = characterSheet.CharacterSheet.Genitals,
                GoalsForNextYear = characterSheet.CharacterSheet.GoalsForNextYear,
                HairColor = characterSheet.CharacterSheet.HairColor,
                HairStyle = characterSheet.CharacterSheet.HairStyle,
                Height = characterSheet.CharacterSheet.Height,
                Kinks = characterSheet.CharacterSheet.Kinks,
                LastName = characterSheet.CharacterSheet.LastName,
                Likes = characterSheet.CharacterSheet.Likes,
                LongTermGoals = characterSheet.CharacterSheet.LongTermGoals,
                Mannerisms = characterSheet.CharacterSheet.Mannerisms,
                PathfinderAttributesValues = characterSheet.CharacterSheet.PathfinderAttributesValues,
                PathfinderSkillsValues = characterSheet.CharacterSheet.PathfinderSkillsValues,
                PersonalityTraits = characterSheet.CharacterSheet.PersonalityTraits,
                PreferredCombatStyle = characterSheet.CharacterSheet.PreferredCombatStyle,
                Profession = characterSheet.CharacterSheet.Profession,
                Race = characterSheet.CharacterSheet.Race,
                Relationships = characterSheet.CharacterSheet.Relationships,
                Reputation = characterSheet.CharacterSheet.Reputation,
                SecretKinks = characterSheet.CharacterSheet.SecretKinks,
                Secrets = characterSheet.CharacterSheet.Secrets,
                Sexuality = characterSheet.CharacterSheet.Sexuality,
                Skills = characterSheet.CharacterSheet.Skills,
                SkinColor = characterSheet.CharacterSheet.SkinColor,
                SocialAnxiety = characterSheet.CharacterSheet.SocialAnxiety,
                SpeechImpairment = characterSheet.CharacterSheet.SpeechImpairment,
                SpeechPattern = characterSheet.CharacterSheet.SpeechPattern,
                Weaknesses = characterSheet.CharacterSheet.Weaknesses,
                WeaponsProficiency = characterSheet.CharacterSheet.WeaponsProficiency,
            },
        };
    }

    public async Task<IWebApiResponseDto> GetCharacterSheetByPersonaIdAsync(string personaId)
    {
        var characterSheets = await storageService.GetCharacterSheetsByFuncAsync(f=>f.PersonaId == personaId);
        var characterSheet = characterSheets?.FirstOrDefault();

        if(characterSheet == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"CharacterSheet linked to characterId {personaId} was not found."
            };
        }

        return new GetCharacterSheetResponseDto
        {
            CharacterSheetId = characterSheet.CharacterSheetId,
            CharacterId = characterSheet.CharacterId,
            PersonaId = characterSheet.PersonaId,
            LastActivityAtUtc = characterSheet.LastActivityAtUtc,
            CharacterSheet = new CharacterSheet
            {
                AgeGroup = characterSheet.CharacterSheet.AgeGroup,
                Attractiveness = characterSheet.CharacterSheet.Attractiveness,
                Behavior = characterSheet.CharacterSheet.Behavior,
                BirthdayDate = characterSheet.CharacterSheet.BirthdayDate,
                BodyType = characterSheet.CharacterSheet.BodyType,
                BreastsSize = characterSheet.CharacterSheet.BreastsSize,
                ClothesPreference = characterSheet.CharacterSheet.ClothesPreference,
                CombatAffinityAttack = characterSheet.CharacterSheet.CombatAffinityAttack,
                CombatAffinityDefense = characterSheet.CharacterSheet.CombatAffinityDefense,
                Dislikes = characterSheet.CharacterSheet.Dislikes,
                EarShape = characterSheet.CharacterSheet.EarShape,
                EyeColor = characterSheet.CharacterSheet.EyeColor,
                Fears = characterSheet.CharacterSheet.Fears,
                FirstName = characterSheet.CharacterSheet.FirstName,
                Gender = characterSheet.CharacterSheet.Gender,
                Genitals = characterSheet.CharacterSheet.Genitals,
                GoalsForNextYear = characterSheet.CharacterSheet.GoalsForNextYear,
                HairColor = characterSheet.CharacterSheet.HairColor,
                HairStyle = characterSheet.CharacterSheet.HairStyle,
                Height = characterSheet.CharacterSheet.Height,
                Kinks = characterSheet.CharacterSheet.Kinks,
                LastName = characterSheet.CharacterSheet.LastName,
                Likes = characterSheet.CharacterSheet.Likes,
                LongTermGoals = characterSheet.CharacterSheet.LongTermGoals,
                Mannerisms = characterSheet.CharacterSheet.Mannerisms,
                PathfinderAttributesValues = characterSheet.CharacterSheet.PathfinderAttributesValues,
                PathfinderSkillsValues = characterSheet.CharacterSheet.PathfinderSkillsValues,
                PersonalityTraits = characterSheet.CharacterSheet.PersonalityTraits,
                PreferredCombatStyle = characterSheet.CharacterSheet.PreferredCombatStyle,
                Profession = characterSheet.CharacterSheet.Profession,
                Race = characterSheet.CharacterSheet.Race,
                Relationships = characterSheet.CharacterSheet.Relationships,
                Reputation = characterSheet.CharacterSheet.Reputation,
                SecretKinks = characterSheet.CharacterSheet.SecretKinks,
                Secrets = characterSheet.CharacterSheet.Secrets,
                Sexuality = characterSheet.CharacterSheet.Sexuality,
                Skills = characterSheet.CharacterSheet.Skills,
                SkinColor = characterSheet.CharacterSheet.SkinColor,
                SocialAnxiety = characterSheet.CharacterSheet.SocialAnxiety,
                SpeechImpairment = characterSheet.CharacterSheet.SpeechImpairment,
                SpeechPattern = characterSheet.CharacterSheet.SpeechPattern,
                Weaknesses = characterSheet.CharacterSheet.Weaknesses,
                WeaponsProficiency = characterSheet.CharacterSheet.WeaponsProficiency,
            },
        };
    }
}
