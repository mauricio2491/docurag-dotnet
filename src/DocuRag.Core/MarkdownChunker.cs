using System.Text;

namespace DocuRag.Core;

public static class MarkdownChunker
{
    // ~2000 caracteres ≈ 500 tokens. Punto de partida: se ajustará midiendo con evaluaciones.
    public static IEnumerable<Chunk> Split(string file, string markdown, int maxChars = 2000)
    {
        markdown = markdown.Replace("\r\n", "\n");

        // Limpieza: los docs de Microsoft empiezan con metadatos entre "---".
        if (markdown.StartsWith("---"))
        {
            var end = markdown.IndexOf("\n---", 3);
            if (end > 0) markdown = markdown[(end + 4)..];
        }

        var section = "(inicio)";
        var buffer = new StringBuilder();
        var index = 0;
        string? lastParagraph = null;

        foreach (var raw in markdown.Split("\n\n"))
        {
            var paragraph = raw.Trim();
            if (paragraph.Length == 0) continue;

            // Un título cierra el chunk actual y empieza una sección nueva.
            if (paragraph.StartsWith('#'))
            {
                if (buffer.Length > 0)
                {
                    yield return new Chunk(file, section, index++, buffer.ToString());
                    buffer.Clear();
                }
                section = paragraph.Split('\n')[0].TrimStart('#').Trim();
                lastParagraph = null;
                buffer.AppendLine(paragraph).AppendLine();
                continue;
            }

            // Si no cabe, cerramos el chunk y empezamos otro
            // repitiendo el último párrafo (solapamiento).
            if (buffer.Length + paragraph.Length > maxChars && buffer.Length > 0)
            {
                yield return new Chunk(file, section, index++, buffer.ToString());
                buffer.Clear();
                if (lastParagraph != null) buffer.AppendLine(lastParagraph).AppendLine();
            }

            buffer.AppendLine(paragraph).AppendLine();
            lastParagraph = paragraph;
        }

        if (buffer.Length > 0)
            yield return new Chunk(file, section, index, buffer.ToString());
    }
}