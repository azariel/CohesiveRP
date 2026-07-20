namespace CohesiveRP.Core.LLMProviderProcessors.Pathfinder.CharactersMutations.BusinessObjects
{
    public class CharacterStatusUpdateLinks
    {
        public List<CharacterStatusUpdateTarget> Targets { get; set; }
    }

    public class CharacterStatusUpdateTarget
    {
        public string CharacterSheetInstanceId { get; set; }

        /// <summary>
        /// The messageId of the last time this character's status was actually processed (if ever).
        /// Captured at queue time, before this cycle's mutations.
        /// </summary>
        public string LastStatusCheckMessageId { get; set; }

        /// <summary>
        /// The messageId of the last time this character was confirmed NOT present in the scene, i.e. the
        /// boundary marking the start of their current (or just-ended) presence session.
        /// Captured at queue time, before this cycle's mutations.
        /// </summary>
        public string LastConfirmedAbsentMessageId { get; set; }
    }
}