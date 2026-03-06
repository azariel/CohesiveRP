namespace CohesiveRP.Storage.JsonConverters
{
     using System.Text.Json;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    /// <summary>
    /// EF Core value converter that serializes/deserializes a property as JSON in the database.
    /// </summary>
    /// <typeparam name="T">The CLR type to convert.</typeparam>
    public sealed class JsonValueConverter<T> : ValueConverter<T, string>
    {
        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public JsonValueConverter() : base(
            v => JsonSerializer.Serialize(v, options),
            v => JsonSerializer.Deserialize<T>(v, options))
        {
        }
    }
}
