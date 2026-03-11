import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { CharacterResponse } from "./CharacterResponse";

interface CharactersResponseDto extends ServerApiResponseDto {
    characters : CharacterResponse[] | null,
}

export type {
    CharactersResponseDto
};