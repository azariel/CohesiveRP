using System.Text.Json.Nodes;

namespace CohesiveRP.Core.ComfyUI
{
    public class ExpressionWorkflowInjector
    {
        private static class Nodes
        {
            public const string SourceAvatarImage = "2";   // LoadImage — face reference for IPAdapter
            public const string Checkpoint        = "6";
            public const string ClipLayer         = "10";
            public const string PositiveText      = "51";  // Text Multiline — feeds both CLIPTextEncode nodes
            public const string NegativeText      = "267"; // StringConcatenate string_a
            public const string MasterSeed        = "29";  // PrimitiveInt
            public const string LatentImage       = "21";
            public const string FaceDetailerSeed  = "17";
            public const string FilenamePrefix    = "297"; // StringConcatenate string_a
        }

        private readonly string _templateJson;

        public static string NodeSaveImage { get; set; } = "23";

        public ExpressionWorkflowInjector(string templateJson)
        {
            _templateJson = templateJson;
        }

        public string Inject(AvatarGenerationRequest request)
        {
            if (request.Seed is null)
                throw new ArgumentException("Seed must be resolved before injection.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.SourceAvatarFileName))
                throw new ArgumentException("SourceAvatarFileName must be set — the workflow needs a face reference image.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.Expression))
                throw new ArgumentException("Expression must be set — the workflow needs an expression to generate.", nameof(request));

            var workflow = JsonNode.Parse(_templateJson)!.AsObject();

            // Face reference image for IPAdapter identity locking
            SetInput(workflow, Nodes.SourceAvatarImage, "image",  request.SourceAvatarFileName);

            //SetInput(workflow, Nodes.Checkpoint, "ckpt_name",        request.CheckpointProfile.CheckpointName);
            //SetInput(workflow, Nodes.ClipLayer,  "stop_at_clip_layer", request.CheckpointProfile.ClipLastLayer);

            SetInput(workflow, Nodes.PositiveText,   "text",     request.PositivePrompt);
            SetInput(workflow, Nodes.NegativeText,   "string_a", request.NegativePromptOverride);
            SetInput(workflow, Nodes.MasterSeed,     "value",    request.Seed.Value);
            SetInput(workflow, Nodes.FaceDetailerSeed,  "seed",     request.Seed.Value);
            SetInput(workflow, Nodes.LatentImage,    "width",    request.WidthOverride);
            SetInput(workflow, Nodes.LatentImage,    "height",   request.HeightOverride);
            SetInput(workflow, Nodes.FilenamePrefix, "string_a", $"{request.CharacterId}_e_{request.Expression}_");

            return workflow.ToJsonString();
        }

        private static void SetInput(JsonObject workflow, string nodeId, string inputKey, object value)
        {
            if (workflow[nodeId]?["inputs"] is not JsonObject inputs)
                throw new InvalidOperationException(
                    $"[{nameof(ExpressionWorkflowInjector)}] Node [{nodeId}] or its [inputs] block " +
                    $"was not found in the workflow template.");

            inputs[inputKey] = JsonValue.Create(value);
        }
    }
}