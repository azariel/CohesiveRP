import type { CharacterAvatar } from "./CharacterAvatar";

interface ChatMessage {
    messageId: string;
    messageIndex: number | null;
    content: string | null;
    thinkingContent: string | null;
    sourceType: number;
    createdAtUtc: string | null;
    summarized: boolean | null;
    characterId: string | null;
    characterName: string | null;
    personaId: string | null;
    personaName: string | null;
    characterAvatars: CharacterAvatar[] | null;
    // Injected fields
    startGenerationDateTimeUtc?: string;
    startFocusedGenerationDateTimeUtc?: string;
    endFocusedGenerationDateTimeUtc?: string;
}
export type {
    ChatMessage
};
