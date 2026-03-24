import type { ServerApiResponseDto } from "../../ServerApiResponseDto";
import type { CharacterSheet } from "./CharacterSheet";

interface CharacterSheetResponseDto extends ServerApiResponseDto {
  characterSheetId?: string;
  personaId?: string;
  characterId?: string;
  lastActivityAtUtc?: Date;
  characterSheet?: CharacterSheet;
}

export type {
    CharacterSheetResponseDto
};