using System.Text.Json.Serialization;

namespace CohesiveRP.Common.Utils.Parsers.BusinessObjects
{
    public class IdSentence
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("sentenceText")]
        public string SentenceText { get; set; }

        [JsonIgnore]
        public bool StartsNewParagraph { get; set; }

        [JsonIgnore]
        public bool IsLocked => SentenceText.IndexOfAny(new[] { '"', '“', '”' }) >= 0;
    }
}
