import type { ServerApiResponseDto } from "../ServerApiResponseDto";
import type { Lorebook } from "./BusinessObjects/Lorebook";

interface LorebookResponseDto extends ServerApiResponseDto  {
    lorebook: Lorebook;
}

export type {
    LorebookResponseDto
};