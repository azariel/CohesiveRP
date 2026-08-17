import type { CharacterSheet } from "../../../ResponsesDto/characters/characterSheets/CharacterSheet";

interface CharacterSheetInstanceRequestDto {
  characterId?: string | null;
  chatId?: string | null;
  characterSheetInstanceId?: string | null;
  characterSheet?: CharacterSheet | null;
}

export type {
    CharacterSheetInstanceRequestDto
};