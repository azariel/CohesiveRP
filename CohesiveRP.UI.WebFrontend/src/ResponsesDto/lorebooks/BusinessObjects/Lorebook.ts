import type { LorebookEntry } from "./LorebookEntry";

interface Lorebook {
    lorebookId: string;
    name: string | null;
    lastActivityDateTime: Date | null;
    entries: LorebookEntry[];
}

export type {
    Lorebook
};