using DocuRag.Core;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);

var ollamaUri = new Uri("http://localhost:11434");

// El chat y los embeddings se registran como interfaces (IChatClient, IEmbeddingGenerator).
builder.Services.AddChatClient(new OllamaApiClient(ollamaUri, "qwen2.5:7b"));

IEmbeddingGenerator<string, Embedding<float>> embedder =
    new OllamaApiClient(ollamaUri, "nomic-embed-text");
builder.Services.AddEmbeddingGenerator(embedder);

builder.Services.AddSingleton(new QdrantClient("localhost"));
builder.Services.AddSingleton<Retriever>();
builder.Services.AddSingleton<RagService>();

var app = builder.Build();

app.MapPost("/ask", async (AskRequest req, RagService rag) =>
    await rag.AskAsync(req.Question));

app.Run();

record AskRequest(string Question);