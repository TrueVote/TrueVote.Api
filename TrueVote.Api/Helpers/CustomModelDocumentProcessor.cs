using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.Processors;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers;

[ExcludeFromCodeCoverage]
public class CustomModelDocumentProcessor<T> : IDocumentProcessor where T : class
{
    public void Process(DocumentProcessorContext context)
    {
        // Generate the schema for the specified type and add it to the document
        var schema = context.SchemaGenerator.Generate(typeof(T), context.SchemaResolver);
        var schemaName = typeof(T).Name;

        // Add the schema to the document's definitions if it doesn't already exist
        if (!context.Document.Definitions.ContainsKey(schemaName))
        {
            context.Document.Definitions[schemaName] = schema;
        }
    }
}
