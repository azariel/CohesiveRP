import type { LorebookEntry } from "../../ResponsesDto/lorebooks/BusinessObjects/LorebookEntry";

interface LorebookUpdateRequestDto  {
    name: string;
    entries: LorebookEntry[];
}

export type {
    LorebookUpdateRequestDto
};