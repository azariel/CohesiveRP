import type { CharacterSheet } from "../../../ResponsesDto/characters/characterSheets/CharacterSheet";

interface CharacterSheetRequestDto {
  characterId?: string | null;
  personaId?: string | null;
  characterSheetId?: string | null;
  characterSheet?: CharacterSheet | null;
}

export type {
    CharacterSheetRequestDto
};