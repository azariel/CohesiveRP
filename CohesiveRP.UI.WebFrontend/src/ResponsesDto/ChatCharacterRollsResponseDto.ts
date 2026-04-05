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

export interface ChatCharacterRoll {
  actionCategory: string;
  reasonings: string[];
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
}
