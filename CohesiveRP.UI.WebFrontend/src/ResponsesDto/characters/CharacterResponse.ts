import type { CharacterAvatar } from "./AvatarPath";
import type { ImageGenerationConfiguration } from "./ImageGenerationConfiguration";

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
    imageGenerationConfiguration: ImageGenerationConfiguration | null,
    sourceAvatars?: CharacterAvatar[] | null;
}

export type {
    CharacterResponse
};