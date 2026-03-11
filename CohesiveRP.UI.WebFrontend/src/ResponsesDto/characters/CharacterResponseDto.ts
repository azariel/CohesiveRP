import type { ServerApiResponseDto } from "../ServerApiResponseDto";

interface CharacterResponseDto extends ServerApiResponseDto {
    characterId : string,
    createdAtUtc : string,
    name : string,
}

export type {
    CharacterResponseDto
};