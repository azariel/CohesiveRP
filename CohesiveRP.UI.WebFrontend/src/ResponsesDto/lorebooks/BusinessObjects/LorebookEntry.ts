interface LorebookEntry {
    keys: string[] | null;
    content: string | null;
    enabled: boolean;
    insertionOrder: number;
    useRegex: boolean;
    constant: boolean;
}

export type {
    LorebookEntry
};