interface SceneTracker {
    chatId: string | null;
    content: string | null;
    sceneTrackerId: string | null;
    createdAtUtc: Date | null;
    linkMessageId: string | null;
}

export type {
    SceneTracker
};