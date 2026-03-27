interface CharacterSheet {
  firstName?: string;
  lastName?: string;
  birthday?: string | null;
  gender?: string | null;
  ageGroup?: string | null;
  race?: string;
  height?: string;
  speechPattern?: string;
  speechImpairment?: string;
  bodyType?: string;
  hairColor?: string;
  hairStyle?: string;
  eyeColor?: string;
  earShape?: string;
  skinColor?: string;
  genitals?: string | null;
  breastsSize?: string | null;
  penisSize?: string | null;
  sexuality?: string | null;
  relationships?: string[];
  profession?: string;
  reputation?: string;
  preferredCombatStyle?: string;
  weaponsProficiency?: string;
  combatAffinityAttack?: string;
  combatAffinityDefense?: string;
  socialAnxiety?: string;
  clothesPreference?: string;
  mannerisms?: string;
  behavior?: string;
  attractiveness?: string;
  kinks?: string[];
  secretKinks?: string[];
  skills?: string[];
  weaknesses?: string[];
  fears?: string[];
  likes?: string[];
  dislikes?: string[];
  secrets?: string[];
  personalityTraits?: string[];
  goalsForNextYear?: string[];
  longTermGoals?: string[];
  pathfinderAttributes?: PathfinderAttributeDto[];
  pathfinderSkills?: PathfinderSkillAttributeDto[];
}

interface PathfinderAttributeDto {
  attributeType: string;
  value: number;
}

interface PathfinderSkillAttributeDto {
  skillType: string;
  value: number;
}

export type {
    CharacterSheet,
    PathfinderAttributeDto,
    PathfinderSkillAttributeDto
};