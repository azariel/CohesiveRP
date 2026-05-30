namespace CohesiveRP.Core.ComfyUI
{
    public class AvatarGenerationRequest
    {
        /// <summary>Used to prefix the saved filename on disk (node 63 string_a).</summary>
        public string CharacterId { get; set; } = string.Empty;

        public string PositivePrompt { get; set; } = string.Empty;

        /// <summary>When null, the checkpoint profile's DefaultNegativePrompt is used.</summary>
        public string? NegativePromptOverride { get; set; }

        /// <summary>When null, AvatarGenerationService assigns a random seed before injection.</summary>
        public long? Seed { get; set; }

        public int? WidthOverride { get; set; }
        public int? HeightOverride { get; set; }
        public string SourceAvatarFileName { get; set; }
        public string Expression { get; set; }
    }
}
