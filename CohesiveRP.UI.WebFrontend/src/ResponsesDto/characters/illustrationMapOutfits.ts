import type { CharacterAvatar } from "./AvatarPath";

interface illustrationMapOutfits {
    illustratorPromptInjection?: string | null;
    outfit?: string | null;// clothed, underwear, naked
    sourceAvatars?: CharacterAvatar[] | null;
}

export type {
    illustrationMapOutfits
};