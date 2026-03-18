import type { ServerApiResponseDto } from "../ServerApiResponseDto"
import type { Lorebook } from "./BusinessObjects/Lorebook";

interface LorebooksResponseDto extends ServerApiResponseDto {
    lorebooks: Lorebook[]
}

export type {
    LorebooksResponseDto
};