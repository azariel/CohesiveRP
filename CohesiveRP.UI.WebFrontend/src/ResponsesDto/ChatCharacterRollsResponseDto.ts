import type { ServerApiResponseDto } from "./ServerApiResponseDto";

// ── Business objects ──────────────────────────────────────────────────────────

export interface CharacterInSceneCounterRoll {
  attribute: string;
  value: number;
}

export interface ChatCharacterInSceneCounterRolls {
  characterId: string;
  characterName: string;
  characterInSceneCounterRoll: CharacterInSceneCounterRoll;
}

export interface SkillCheckReasoningGuide {
  reasoning: string;
  reactionFromOtherCharactersWhenFailingSkillCheck: string;
  reactionFromOtherCharactersWhenSucceedingSkillCheck: string;
}

export interface ChatCharacterRoll {
  actionCategory: string;
  charactersWhoCanResist: string[];
  guides: SkillCheckReasoningGuide[];
  bonus: number;
  value: number;
  charactersInSceneWithCounterRolls: ChatCharacterInSceneCounterRolls[];
}

export interface ChatCharacterRollResponse {
  characterId: string;
  characterName: string;
  rolls: ChatCharacterRoll[];
}

export interface ChatCharacterRollsResponseDto extends ServerApiResponseDto {
  rolls: ChatCharacterRollResponse[];
  playerDescription?: string;
}