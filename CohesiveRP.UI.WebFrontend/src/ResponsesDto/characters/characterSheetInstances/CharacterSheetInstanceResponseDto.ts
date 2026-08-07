import type { ServerApiResponseDto } from "../../ServerApiResponseDto";
import type { CharacterSheet } from "../characterSheets/CharacterSheet";

interface CharacterSheetInstanceResponseDto extends ServerApiResponseDto {
  characterSheetInstanceId?: string;
  characterSheetId?: string;   // the parent blueprint this was copied from
  characterId?: string;
  chatId?: string;
  lastActivityAtUtc?: Date;
  characterSheet?: CharacterSheet;
}

export type {
    CharacterSheetInstanceResponseDto
};