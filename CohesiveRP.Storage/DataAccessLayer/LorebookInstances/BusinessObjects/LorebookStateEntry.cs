using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects
{
    public class LorebookStateEntry
    {
        [JsonPropertyName("lorebookEntryId")]
        public string LorebookEntryId { get; init; }

        /// <summary>
        /// Remaining sticky amount in messages.
        /// </summary>
        [JsonPropertyName("remainingStickeyAmount")]
        public int RemainingStickeyAmount { get; set; }

        [JsonPropertyName("remainingCooldownAmount")]
        public int RemainingCooldownAmount { get; set; }

        [JsonPropertyName("linkedMessageId")]
        public string LinkedMessageId { get; set; }
    }
}
