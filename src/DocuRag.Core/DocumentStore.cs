using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DocuRag.Core;

public class DocumentStore(
    QdrantClient qdrant,
    IEmbeddingGenerator<string, Embedding<float>> embedder)
{
    public const string Collection = "docs";

    public async Task EnsureCollectionAsync()
    {
        if (!await qdrant.CollectionExistsAsync(Collection))
        {
            // 768 = tamaño del vector de nomic-embed-text. Coseno = similitud que viste antes.
            await qdrant.CreateCollectionAsync(Collection,
                new VectorParams { Size = 768, Distance = Distance.Cosine });
        }
    }

    public async Task UpsertAsync(IReadOnlyList<Chunk> chunks)
    {
        // nomic-embed-text recomienda el prefijo "search_document: " al indexar.
        var embeddings = await embedder.GenerateAsync(
            chunks.Select(c => "search_document: " + c.Text));

        var points = chunks.Select((c, i) => new PointStruct
        {
            Id = StableId(c.File, c.Index),   // mismo archivo + posición = mismo id → idempotente
            Vectors = embeddings[i].Vector.ToArray(),
            Payload =
            {
                ["file"] = c.File,
                ["section"] = c.Section,
                ["text"] = c.Text
            }
        }).ToList();

        await qdrant.UpsertAsync(Collection, points);
    }

    private static Guid StableId(string file, int index) =>
        new(MD5.HashData(Encoding.UTF8.GetBytes($"{file}#{index}")));
}