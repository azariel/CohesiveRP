namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations.BusinessObjects
{
    public static class CharacterStatusUpdateConstants
    {
        /// <summary>
        /// Shared "recent activity is still unstable" window, used in two units for the same underlying idea:
        /// - As a RAW MESSAGE count: how many of the most recent messages are excluded from status-update analysis,
        ///   since the User can still edit or delete them at any time.
        /// - As a SCENE-TRACKER CYCLE count: how many consecutive absent cycles are required before a character's
        ///   departure is treated as confirmed rather than sceneTracker top-N flicker.
        /// Using the same value for both means that by the time a departure is confirmed, that character's
        /// presence-session messages have typically already cleared the mutable tail.
        /// </summary>
        public const int RECENT_ACTIVITY_STABILITY_WINDOW = 5;// TODO: make configurable

        /// <summary>
        /// Minimum number of new STABLE (non-buffered) messages a present character must accumulate since their
        /// last status check before a status update is queued.
        /// </summary>
        public const int CHARACTER_STATUS_UPDATE_MESSAGE_THRESHOLD = 10;// TODO: make configurable
    }
}