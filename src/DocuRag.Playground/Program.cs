using System.Numerics.Tensors;
using Microsoft.Extensions.AI;
using OllamaSharp;

IEmbeddingGenerator<string, Embedding<float>> generator =
    new OllamaApiClient(new Uri("http://localhost:11434"), "nomic-embed-text");

string[] frases =
[
    "Política de días libres y permisos para empleados",
    "Cómo solicitar vacaciones en el portal interno",
    "Procedimiento para reportar gastos de viaje",
    "Configuración de la VPN en Windows",
    "Receta de pastel de chocolate",
    "El equipo de fútbol ganó el campeonato",
    "Que fue primero, el huevo o el pollo?",
    "Cómo pedir unos días de descanso a tu jefe de manera efectiva y profesional"
];

var pregunta = "¿Cómo pido unos días de descanso?";

// Un vector para la pregunta y uno para cada frase.
var vPregunta = await generator.GenerateVectorAsync(pregunta);
var vFrases = await generator.GenerateAsync(frases);

Console.WriteLine($"Cada vector tiene {vPregunta.Length} números.\n");
Console.WriteLine($"Pregunta: {pregunta}\n");

for (int i = 0; i < frases.Length; i++)
{
    var similitud = TensorPrimitives.CosineSimilarity(
        vPregunta.Span, vFrases[i].Vector.Span);
    Console.WriteLine($"{similitud:F3}  {frases[i]}");
}