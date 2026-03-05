namespace CohesiveRP.Storage.JsonConverters
{
    /// <summary>
    /// Marks an EF Core entity property for automatic JSON serialization/deserialization
    /// when persisting to the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class JsonValueConverterAttribute : Attribute
    {
    }
}
