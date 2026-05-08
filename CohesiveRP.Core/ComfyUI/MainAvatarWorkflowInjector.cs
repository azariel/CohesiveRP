using System.Text.Json.Nodes;

namespace CohesiveRP.Core.ComfyUI
{
    public class MainAvatarWorkflowInjector
    {
        private static class Nodes
        {
            public const string Checkpoint = "6";   // CheckpointLoaderSimple → ckpt_name
            public const string ClipLayer = "3";   // CLIPSetLastLayer       → stop_at_clip_layer
            public const string PositiveText = "34";  // Text _O                → text
            public const string NegativeText = "9";   // CLIPTextEncode         → text
            public const string MasterSeed = "64";  // Random Number          → seed (fans out to KSampler + FaceDetailer)
            public const string SegsDetailer = "40";  // SEGSDetailer           → seed (was hardcoded, inject for determinism)
            public const string LatentImage = "7";   // EmptyLatentImage       → width, height
            public const string FilenamePrefix = "63";  // StringConcatenate      → string_a
        }

        private readonly string _templateJson;

        public MainAvatarWorkflowInjector(string templateJson)
        {
            _templateJson = templateJson;
        }

        public string Inject(AvatarGenerationRequest request)
        {
            if (request.Seed is null)
                throw new ArgumentException("Seed must be resolved before injection.", nameof(request));

            //if (request.CheckpointProfile is null)
            //    throw new ArgumentException("CheckpointProfile must be set before injection.", nameof(request));

            var workflow = JsonNode.Parse(_templateJson)!.AsObject();

            //SetInput(workflow, Nodes.Checkpoint, "ckpt_name", request.CheckpointProfile.CheckpointName);
            //SetInput(workflow, Nodes.ClipLayer, "stop_at_clip_layer", request.CheckpointProfile.ClipLastLayer);
            SetInput(workflow, Nodes.PositiveText, "text", request.PositivePrompt);
            SetInput(workflow, Nodes.NegativeText, "text", request.NegativePromptOverride);
            SetInput(workflow, Nodes.MasterSeed, "seed", request.Seed.Value);
            SetInput(workflow, Nodes.SegsDetailer, "seed", request.Seed.Value);
            SetInput(workflow, Nodes.LatentImage, "width", request.WidthOverride);
            SetInput(workflow, Nodes.LatentImage, "height", request.HeightOverride);
            SetInput(workflow, Nodes.FilenamePrefix, "string_a", $"{request.CharacterId}_s_");

            return workflow.ToJsonString();
        }

        private static void SetInput(JsonObject workflow, string nodeId, string inputKey, object value)
        {
            if (workflow[nodeId]?["inputs"] is not JsonObject inputs)
                throw new InvalidOperationException(
                    $"[{nameof(MainAvatarWorkflowInjector)}] Node [{nodeId}] or its [inputs] block " +
                    $"was not found in the workflow template.");

            inputs[inputKey] = JsonValue.Create(value);
        }
    }
}
