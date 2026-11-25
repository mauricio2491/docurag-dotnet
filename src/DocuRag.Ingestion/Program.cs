using DocuRag.Core;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Qdrant.Client;

var folder = args.Length > 0 ? args[0] : "docs";

IEmbeddingGenerator<string, Embedding<float>> embedder =
    new OllamaApiClient(new Uri("http://localhost:11434"), "nomic-embed-text");

// QdrantClient usa el puerto 6334 (gRPC) por defecto.
var store = new DocumentStore(new QdrantClient("localhost"), embedder);
await store.EnsureCollectionAsync();

var files = Directory.GetFiles(folder, "*.md", SearchOption.AllDirectories);
var total = 0;

foreach (var path in files)
{
    var name = Path.GetRelativePath(folder, path).Replace('\\', '/');
    var chunks = MarkdownChunker.Split(name, File.ReadAllText(path)).ToList();
    if (chunks.Count == 0) continue;

    await store.UpsertAsync(chunks);
    total += chunks.Count;
    Console.WriteLine($"{chunks.Count,3} chunks  {name}");
}

Console.WriteLine($"\nListo: {files.Length} archivos, {total} chunks.");