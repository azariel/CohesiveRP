import { useState } from "react";
import type { CharacterSheet, PathfinderAttributeDto, PathfinderSkillAttributeDto } from "../../../../../../ResponsesDto/characters/characterSheets/CharacterSheet";

export const PATHFINDER_ATTR_KEYS = [
  "Fortitude", "Reflex", "Willpower", "Stamina",
  "MagicalStamina", "MagicalPower", "Intelligence", "Discernment", "Perception",
];

export const PATHFINDER_SKILL_KEYS = [
  "Sex", "Acrobatics", "Athletics", "Deception",
  "Charisma", "Intimidation", "Medicine", "Performance",
  "Society", "Aristocracy", "Stealth", "Thievery",
];

const defaultAttrs = (): PathfinderAttributeDto[] =>
  PATHFINDER_ATTR_KEYS.map((k) => ({ attributeType: k, value: 10 }));

const defaultSkills = (): PathfinderSkillAttributeDto[] =>
  PATHFINDER_SKILL_KEYS.map((k) => ({ skillType: k, value: 10 }));

export const emptyCharacterSheet = (): CharacterSheet => ({
  firstName: "", lastName: "", birthday: "", gender: "", ageGroup: "", ageGroupAppearance: "",
  race: "", height: "", bodyType: "", hairColor: "", hairStyle: "", eyeColor: "", earShape: "",
  skinColor: "", teethDetails: "", lipsDetails: "", eyebrows: "", nailsColor: "", nailsDetails: "",
  genitals: "", breastsSize: "", areolasSize: "", areolasDetails: "", areolasColor: "",
  penisSize: "", sexuality: "", attractiveness: "",
  speechPattern: "", speechImpairment: "", mannerisms: "", socialAnxiety: "", clothesPreference: "",
  profession: "", reputation: "", relationships: [],
  behavior: "", personalityTraits: [], likes: [], dislikes: [], fears: [], secrets: [],
  preferredCombatStyle: "", weaponsProficiency: "", combatAffinityAttack: "", combatAffinityDefense: "",
  skills: [], weaknesses: [],
  goalsForNextYear: [], longTermGoals: [],
  kinks: [], secretKinks: [],
  magicalEffects: [], bodyStatus: [], wounds: [],
  lastInteractionWithPlayer: "", latentMoodForNextInteractionWithPlayer: "",
  recentImportantEvents: [], arousal: 0,
  pathfinderAttributes: defaultAttrs(),
  pathfinderSkills: defaultSkills(),
});

export function useCharacterSheetFields() {
  const [sheet, setSheet] = useState<CharacterSheet>(emptyCharacterSheet());

  const updateField = <K extends keyof CharacterSheet>(key: K, value: CharacterSheet[K]) => {
    setSheet((prev) => ({ ...prev, [key]: value }));
  };

  const getAttr = (key: string) =>
    sheet.pathfinderAttributes?.find((a) => a.attributeType === key)?.value ?? 10;

  const setAttr = (key: string, value: number) => {
    setSheet((prev) => {
      const attrs = prev.pathfinderAttributes ? [...prev.pathfinderAttributes] : defaultAttrs();
      const idx = attrs.findIndex((a) => a.attributeType === key);
      if (idx >= 0) attrs[idx] = { ...attrs[idx], value };
      else attrs.push({ attributeType: key, value });
      return { ...prev, pathfinderAttributes: attrs };
    });
  };

  const getSkill = (key: string) =>
    sheet.pathfinderSkills?.find((s) => s.skillType === key)?.value ?? 10;

  const setSkill = (key: string, value: number) => {
    setSheet((prev) => {
      const sk = prev.pathfinderSkills ? [...prev.pathfinderSkills] : defaultSkills();
      const idx = sk.findIndex((s) => s.skillType === key);
      if (idx >= 0) sk[idx] = { ...sk[idx], value };
      else sk.push({ skillType: key, value });
      return { ...prev, pathfinderSkills: sk };
    });
  };

  /* Hydrate from a fetched/imported sheet — falls back to empty defaults for any
     field the incoming payload doesn't have, so partial JSON imports don't crash the form. */
  const hydrate = (incoming?: CharacterSheet | null) => {
    const base = emptyCharacterSheet();
    if (!incoming) {
      setSheet(base);
      return;
    }
    setSheet({
      ...base,
      ...incoming,
      pathfinderAttributes: incoming.pathfinderAttributes?.length ? incoming.pathfinderAttributes : base.pathfinderAttributes,
      pathfinderSkills: incoming.pathfinderSkills?.length ? incoming.pathfinderSkills : base.pathfinderSkills,
    });
  };

  return { sheet, setSheet, updateField, getAttr, setAttr, getSkill, setSkill, hydrate };
}

export type UseCharacterSheetFields = ReturnType<typeof useCharacterSheetFields>;