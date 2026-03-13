import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { CharacterResponse } from "./CharacterResponse";

interface CharacterResponseDto extends ServerApiResponseDto {
    character: CharacterResponse | null;
}

export type {
    CharacterResponseDto
};