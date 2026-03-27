using System.Text.Json.Serialization;
using CohesiveRP.Common;
using CohesiveRP.Core.LLMProviderProcessors.Pathfinder.SkillChecksInitiator.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheets.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects
{
    public class CharacterSheet
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }// e.g: Daphne

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }// e.g: Greengrass

        [JsonPropertyName("birthday")]
        public string BirthdayDate { get; set; }// e.g: 27 March 1980

        [JsonPropertyName("gender")]
        public string Gender { get; set; }// e.g: Female

        [JsonPropertyName("ageGroup")]
        public string AgeGroup { get; set; }// e.g: Young adult

        [JsonPropertyName("race")]
        public string Race { get; set; }// e.g: human, elf, ... + add blood purity (mixed, pureblood, etc) // human (Pure-blood witch)

        [JsonPropertyName("height")]
        public string Height { get; set; }// e.g: five foot four (162 cm)

        [JsonPropertyName("speechPattern")]
        public string SpeechPattern { get; set; }// e.g: feminine and low, controlled and detached. Conveys subtle authority through dry wit and intelligent sarcasm. Her tone is cold(icy) with people she dislikes. Her voice can turn into venom when annoyed. Daphne rarely raises her voice, preferring cold and detached tone over emotional outbursts.

        [JsonPropertyName("speechImpairment")]
        public string SpeechImpairment { get; set; }// e.g: could be something like stutters, crippling shyness, etc

        [JsonPropertyName("bodyType")]
        public string BodyType { get; set; }// e.g: lean and slender with attractive feminine aristocratic features

        [JsonPropertyName("hairColor")]
        public string HairColor { get; set; }// e.g: platinum blonde

        [JsonPropertyName("hairStyle")]
        public string HairStyle { get; set; }// e.g: long, straight, slicked back

        [JsonPropertyName("eyeColor")]
        public string EyeColor { get; set; }// e.g: emerald green

        [JsonPropertyName("earShape")]
        public string EarShape { get; set; }// e.g: normal // in fantasy scenario, pointy seems like a reasonable choice

        [JsonPropertyName("skinColor")]
        public string SkinColor { get; set; }// e.g: very pale

        [JsonPropertyName("genitals")]
        public string Genitals { get; set; }// e.g: female

        [JsonPropertyName("breastsSize")]
        public string BreastsSize { get; set; }// e.g: small

        [JsonPropertyName("sexuality")]
        public string Sexuality { get; set; }// e.g: bisexual

        // TODO: This may conflict with Dynamic Memory module!
        [JsonPropertyName("relationships")]
        public string[] Relationships { get; set; }

        [JsonPropertyName("profession")]
        public string Profession { get; set; }// e.g: Hogwarts student (Slytherin House)

        [JsonPropertyName("reputation")]
        public string Reputation { get; set; }// e.g: Daphne has the reputation of the 'Ice Princess' of Slytherin due to her cold demeanor and acid remarks to people she doesn't like. She is known to be discreet and independant. She is ostracised from other Slytherin students due to her cold attitude

        [JsonPropertyName("preferredCombatStyle")]
        public string PreferredCombatStyle { get; set; }// e.g: She often open the fight using darker curses that leave long-lasting or permanent marks on her opponent (cutting curses are her favorite. She also uses banishing charms, binding charms(ropes) and environmental charms(create some ice under her opponent's feet to make them trip) to get control over her enemy. She is wary of being hurt and will quickly shift on defense when overwhelmed.

        [JsonPropertyName("weaponsProficiency")]
        public string WeaponsProficiency { get; set; }// e.g: wand magic

        [JsonPropertyName("combatAffinityAttack")]
        public string CombatAffinityAttack { get; set; }// e.g: Medium-High (uses stupefy, binding enemy's body with magical ropes, transfiguration to bind her opponent's body, stinging jinx, trip jinx, Jelly-Legs Jinx, Reducto curse, Laceration Curse, Leg-Locker Curse, etc. Daphne will use unforgivable when her life is threatened)

        [JsonPropertyName("combatAffinityDefense")]
        public string CombatAffinityDefense { get; set; }// e.g: Medium (uses shield charm, but focus more on controlling her opponent body with magical ropes, shackles, binds, vines or using stupefaction spells)

        [JsonPropertyName("socialAnxiety")]
        public string SocialAnxiety { get; set; }// e.g: None // could be shy

        [JsonPropertyName("clothesPreference")]
        public string ClothesPreference { get; set; }// e.g: Impeccably tailored expensive silk Hogwarts robes that emphasize her aristocratic traits. Always perfectly groomed and accessorized, favoring elegant clothes that speak of old money and taste. Frequently wears very expensive perfume. She prefers lace silk bras and panties.

        [JsonPropertyName("mannerisms")]
        public string Mannerisms { get; set; }// e.g: Carries herself like a queen in exile, one hand often resting on hip bones. Movements are graceful and controlled, every gesture deliberate and self-conscious. Maintains perfect posture that conveys both distance and authority.

        [JsonPropertyName("behavior")]
        public string Behavior { get; set; }// e.g: Composed, intelligent, and quietly ambitious. Harbors an unflinching stoic mask of indifference to hide any vulnerabilities. Values control, subtlety, and loyalty, preferring influence through genuine alliances rather than superficial ones. Raised with pure-blood traditions, yet guided more by pragmatism than prejudice. Her care runs deep, but is expressed through quiet acts rather than words. Lash-out sarcasm when cornered. Daphne is very protective of her friends and of her younger sister Astoria. Daphne drops some part of her mask of indifference when alone with her friends.

        [JsonPropertyName("attractiveness")]
        public string Attractiveness { get; set; }// e.g: Very High (She's seen a skinny and petite aristocratic beauty, but inaccessible due to her personality)

        [JsonPropertyName("kinks")]
        public string[] Kinks { get; set; }// e.g:  Body Worship & Inspection (RECEIVE): A partner meticulously admiring and caressing every inch of her body, including her small breasts and the scar on her ribs. This transforms her perceived flaws into objects of desire, directly healing her deep-seated self-consciousness.

        [JsonPropertyName("secretKinks")]
        public string[] SecretKinks { get; set; }// e.g:  Pet Play & Collaring others (GIVE): Treating her partner or victim as a pet. Collaring them, leashing them, making them eat from a bowl, sleep at her feet, washing them herself and forcing them to walk behind her (either publicly collared and leashed or not). This allows her to enjoy the total control she has over another human being, enhancing the perception of her own superiority.

        [JsonPropertyName("skills")]
        public string[] Skills { get; set; }// e.g:  Competent duelist with elegant and precise spellwork based on her opponent body control

        [JsonPropertyName("weaknesses")]
        public string[] Weaknesses { get; set; }// e.g:  Unwillingness to directly escalate a conflict to a magical or physical fight. She will often simply descalate the argument instead.

        [JsonPropertyName("fears")]
        public string[] Fears { get; set; }// e.g:  She's secretly terrified that the blood malediction affecting her family manifests in herself

        [JsonPropertyName("likes")]
        public string[] Likes { get; set; }// e.g:  The feeling of complete control over a situation or a person

        [JsonPropertyName("dislikes")]
        public string[] Dislikes { get; set; }// e.g:  Chaos and unpredictability in any form

        [JsonPropertyName("secrets")]
        public string[] Secrets { get; set; }// e.g:  The Greengrass blood malediction that has manifested in her sister Astoria

        [JsonPropertyName("personalityTraits")]
        public string[] PersonalityTraits { get; set; }// e.g:  Composed, Intelligent, Quietly ambitious, Graceful, ...

        [JsonPropertyName("goalsForNextYear")]
        public string[] GoalsForNextYear { get; set; }// e.g:  Daphne wants to find contractual loopholes or coercive evidence to legally or politically nullify the betrothal to Draco Malfoy.

        [JsonPropertyName("longTermGoals")]
        public string[] LongTermGoals { get; set; }// e.g:  Daphne wants to recruits a loyal informant, healer, and a political figure bound to her by mutual benefit and quiet favors.

        [JsonPropertyName("pathfinderAttributes")]
        public PathfinderAttribute[] PathfinderAttributesValues { get; set; } = [
            // Default is AVERAGE human male attributes AKA 10 everywhere
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.Fortitude, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.Reflex, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.Willpower, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.Stamina, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.MagicalStamina, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.MagicalPower, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.Intelligence, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.Discernment, Value = 10 },
            new PathfinderAttribute{ AttributeType = PathfinderAttributes.Perception, Value = 10 },
        ];

        [JsonPropertyName("pathfinderSkills")]
        public PathfinderSkillAttributes[] PathfinderSkillsValues { get; set; } = [
            // Default is AVERAGE human male attributes AKA 10 everywhere
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Sex, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Acrobatics, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Athletics, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Deception, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Charisma, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Intimidation, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Medicine, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Performance, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Society, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Aristocracy, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Stealth, Value = 10 },
            new PathfinderSkillAttributes{ SkillType = PathfinderSkills.Thievery, Value = 10 },
        ];
    }
}
