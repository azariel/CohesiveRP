using System;

namespace CohesiveRP.Common.Utils.BusinessObjects
{
    /// <summary>
    /// Marks a property (or field) as mandatory in the generated JSON schema, regardless of its
    /// CLR nullability. Properties marked with this attribute are added to the schema's "required"
    /// array so KoboldCPP's grammar constraint forces the model to always emit them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class JsonSchemaRequiredAttribute : Attribute
    {
    }
}