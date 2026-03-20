interface CharacterResponse {
    characterId : string,
    name : string,
    creator: string,
    creatorNotes: string,
    description: string,
    tags : string[],
    firstMessage : string,
    alternateGreetings : string[],
    createdAtUtc : string,
}

export type {
    CharacterResponse
};