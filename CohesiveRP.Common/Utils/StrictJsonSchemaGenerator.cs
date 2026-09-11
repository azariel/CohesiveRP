using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Common.Utils
{
    public static class StrictJsonSchemaGenerator
    {
        private static readonly ConcurrentDictionary<Type, JsonNode> Cache = new();

        private static readonly JsonSchemaExporterOptions ExporterOptions = new()
        {
            TreatNullObliviousAsNonNullable = true,
            TransformSchemaNode = (context, schema) =>
            {
                if (schema is not JsonObject obj)
                {
                    return schema;
                }

                var typeStr = obj.TryGetPropertyValue("type", out var typeNode) ? typeNode?.ToString() : null;

                // Only object nodes with a "properties" map are containers we can force fields on.
                if (typeStr != "object" || !obj.TryGetPropertyValue("properties", out var propsNode) || propsNode is not JsonObject propsObj)
                {
                    return schema;
                }

                obj["additionalProperties"] = false;

                // Start from whatever the exporter already inferred as required (e.g. non-nullable value types)...
                var requiredNames = new HashSet<string>(StringComparer.Ordinal);
                if (obj.TryGetPropertyValue("required", out var existingRequiredNode) && existingRequiredNode is JsonArray existingRequired)
                {
                    foreach (var item in existingRequired)
                    {
                        if (item?.ToString() is string existingName)
                        {
                            requiredNames.Add(existingName);
                        }
                    }
                }

                // ...then add anything explicitly opted in via [JsonSchemaRequired].
                foreach (var propertyInfo in context.TypeInfo.Properties)
                {
                    if (!propsObj.ContainsKey(propertyInfo.Name))
                    {
                        continue; // ignored/unmapped property, nothing to require
                    }

                    var isForcedRequired = propertyInfo.AttributeProvider?
                        .GetCustomAttributes(typeof(JsonSchemaRequiredAttribute), inherit: true)
                        .Any() == true;

                    if (isForcedRequired)
                    {
                        requiredNames.Add(propertyInfo.Name);
                    }
                }

                obj["required"] = new JsonArray(requiredNames.Select(n => JsonValue.Create(n)).ToArray());

                return schema;
            }
        };

        public static JsonElement Generate<T>()
        {
            var node = Cache.GetOrAdd(typeof(T), _ => JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(T), ExporterOptions));
            return JsonSerializer.SerializeToElement(node);
        }
    }
}