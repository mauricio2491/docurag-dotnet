using Microsoft.Extensions.AI;
using Qdrant.Client;

namespace DocuRag.Core;

public record SearchHit(string File, string Section, string Text, float Score);

public class Retriever(
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    QdrantClient qdrant)
{
    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int k = 5)
    {
        // Para la pregunta, nomic-embed-text usa el prefijo "search_query: ".
        var vector = await embedder.GenerateVectorAsync("search_query: " + query);

        var results = await qdrant.SearchAsync(
            DocumentStore.Collection, vector, limit: (ulong)k);

        return results.Select(r => new SearchHit(
            r.Payload["file"].StringValue,
            r.Payload["section"].StringValue,
            r.Payload["text"].StringValue,
            r.Score)).ToList();
    }
}