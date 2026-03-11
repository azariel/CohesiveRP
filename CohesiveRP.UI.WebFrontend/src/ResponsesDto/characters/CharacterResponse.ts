interface CharacterResponse {
    characterId : string,
    createdAtUtc : string,
    name : string,
    tags : string[],
    description: string,
    creator: string,
    creatorNotes: string,
}

export type {
    CharacterResponse
};