using CohesiveRP.Common.Exceptions;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.WebApi.ResponseDtos.Characters;
using CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace CohesiveRP.Core.WebApi.Workflows.Characters;

public class ExportCharacterCardWorkflow : IExportCharacterCardWorkflow
{
    private IStorageService storageService;

    public ExportCharacterCardWorkflow(IStorageService storageService)
    {
        this.storageService = storageService;
    }

    public async Task<IWebApiResponseDto> ExportCharacterCard(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Request to export CRPV1 was malformed or CharacterId was missing."
            };
        }

        // load the file
        var character = await storageService.GetCharacterByIdAsync(characterId);
        if (character == null)
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Character with ID [{characterId}] was not found."
            };
        }

        var characterSheet = await storageService.GetCharacterSheetByCharacterIdAsync(characterId);

        string directoryCharacterAvatar = Path.Combine(WebConstants.CharactersAvatarFilePath, character.Name.ToLowerInvariant());
        string fileName = Path.Combine(directoryCharacterAvatar, WebConstants.AvatarFileName);
        if (!File.Exists(fileName))
        {
            return new WebApiException
            {
                HttpResultCode = System.Net.HttpStatusCode.NotFound,
                Message = $"Missing Avatar for character [{characterId}]."
            };
        }

        using CancellationTokenSource tokenSource = new CancellationTokenSource(10000);
        using Image image = await Image.LoadAsync(fileName, tokenSource.Token);

        CohesiveRPv1CharacterCard characterCardToSaveCRPv1 = new()
        {
            Spec = "crp",
            SpecVersion = "1.0",
            Data = new CohesiveRPv1Data
            {
                Character = new Character
                {
                    Name = character.Name,
                    Creator = character.Creator,
                    CreatorNotes = character.CreatorNotes,
                    Tags = character.Tags,
                    FirstMessage = character.FirstMessage,
                    AlternateGreetings = character.AlternateGreetings,
                    Description = character.Description,
                },
                CharacterSheet = null,
            }
        };

        if (characterSheet != null)
        {
            characterCardToSaveCRPv1.Data.CharacterSheet = new CharacterSheet()
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
                PenisSize = characterSheet.CharacterSheet.PenisSize,
                GoalsForNextYear = characterSheet.CharacterSheet.GoalsForNextYear,
                HairColor = characterSheet.CharacterSheet.HairColor,
                HairStyle = characterSheet.CharacterSheet.HairStyle,
                Height = characterSheet.CharacterSheet.Height,
                Kinks = characterSheet.CharacterSheet.Kinks,
                LastName = characterSheet.CharacterSheet.LastName,
                Likes = characterSheet.CharacterSheet.Likes,
                LongTermGoals = characterSheet.CharacterSheet.LongTermGoals,
                Mannerisms = characterSheet.CharacterSheet.Mannerisms,
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
                PathfinderAttributesValues = characterSheet.CharacterSheet.PathfinderAttributesValues,
                PathfinderSkillsValues = characterSheet.CharacterSheet.PathfinderSkillsValues,
            };
        }

        CCv3CharacterCard characterCardToSaveCCv3 = new()
        {
            Spec = "ccv3",
            SpecVersion = "3.0",
            Data = new CCv3CharacterData
            {
                Name = character.Name,
                Creator = character.Creator,
                CreatorNotes = character.CreatorNotes,
                Tags = character.Tags,
                FirstMessage = character.FirstMessage,
                AlternateGreetings = character.AlternateGreetings,
                Description = character.Description,
                //CharacterBook = TODO
            },
        };

        var generatedImage = CohesiveRPv1CharacterCardLoader.TryWriteFormat(image, characterCardToSaveCRPv1, characterCardToSaveCCv3);
        using MemoryStream stream = new MemoryStream();
        generatedImage.Save(stream, new PngEncoder());

        var responseDto = new ExportCRPV1ResponseDto
        {
            HttpResultCode = System.Net.HttpStatusCode.OK,
            Image = stream.ToArray(),
        };

        return responseDto;
    }
}
