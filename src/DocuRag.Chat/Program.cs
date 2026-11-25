using Microsoft.Extensions.AI;
using OllamaSharp;

// 1. Creamos el cliente. Ollama escucha en el puerto 11434.
IChatClient client = new OllamaApiClient(
    new Uri("http://localhost:11434"), "qwen2.5:7b");

// 2. El historial empieza con las instrucciones de sistema.
var history = new List<ChatMessage>
{
    new(ChatRole.System, "Eres un asistente técnico. Responde en español y de forma breve.")
};

Console.WriteLine("Chat listo. Escribe 'salir' para terminar.");

while (true)
{
    Console.Write("\nTú: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input == "salir") break;

    // 3. Añadimos la pregunta al historial.
    history.Add(new ChatMessage(ChatRole.User, input));

    // 4. Pedimos la respuesta en streaming: llega por trozos.
    Console.Write("IA: ");
    var reply = "";
    await foreach (var update in client.GetStreamingResponseAsync(history))
    {
        Console.Write(update.Text);
        reply += update.Text;
    }

    // 5. Guardamos la respuesta para que el modelo "recuerde".
    history.Add(new ChatMessage(ChatRole.Assistant, reply));
    Console.WriteLine();
}