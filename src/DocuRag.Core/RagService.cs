using Microsoft.Extensions.AI;

namespace DocuRag.Core;

public record AskResponse(
    string Answer,
    IReadOnlyList<SearchHit> Sources,
    long? InputTokens,
    long? OutputTokens);

public class RagService(IChatClient chat, Retriever retriever)
{
    private const string SystemPrompt = """
        You are a documentation assistant.
        Answer ONLY using the information in the CONTEXT.
        After each statement, cite the source as [file#section].
        If the answer is not in the CONTEXT, say: "I don't know based on the documentation."
        Be concise.
        """;

    public async Task<AskResponse> AskAsync(string question, int k = 5)
    {
        // 1. Retrieval
        var hits = await retriever.SearchAsync(question, k);

        // 2. Construir el contexto con su fuente
        var context = string.Join("\n\n---\n\n",
            hits.Select(h => $"[{h.File}#{h.Section}]\n{h.Text}"));

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, $"CONTEXT:\n{context}\n\nQUESTION: {question}")
        };

        // 3. Generación, temperatura 0 para respuestas estables
        var response = await chat.GetResponseAsync(
            messages, new ChatOptions { Temperature = 0f });

        return new AskResponse(
            response.Text, hits,
            response.Usage?.InputTokenCount, response.Usage?.OutputTokenCount);
    }
}