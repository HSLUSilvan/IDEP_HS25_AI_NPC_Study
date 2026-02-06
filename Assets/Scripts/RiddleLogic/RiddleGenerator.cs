using System;
using System.Collections;
using System.Collections.Generic;

public class RiddleGenerator
{
    private readonly IChatBackend _backend;

    private const string GeneratorSystemPrompt = @"
You generate a single riddle for a fantasy puzzle game.

Output a JSON object in either:
1) <JSON> ... </JSON>
or
2) ```json ... ```

Schema:
{
  ""id"": string,
  ""question"": string,
  ""acceptanceCriteria"": string,
  ""hint"": string
}

Rules:
- Theme: an enchanted door guarding a castle.
- acceptanceCriteria should be short and explicit (e.g. ""echo"", ""a map"", ""time"").
";

    public RiddleGenerator(IChatBackend backend)
    {
        _backend = backend;
    }

    public IEnumerator Generate(
        string themeExtra,
        Action<RiddleDefinition> onRiddle,
        Action<string> onError)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage("system", GeneratorSystemPrompt),
            new ChatMessage("user", $"Extra theme constraints: {themeExtra}")
        };

        string raw = null;
        string err = null;

        yield return _backend.ChatOnce(messages, t => raw = t, e => err = e);

        if (!string.IsNullOrEmpty(err))
        {
            onError?.Invoke(err);
            yield break;
        }

        if (!JsonEnvelopeExtractor.TryExtract(raw, out var json))
        {
            onError?.Invoke("Generator returned no JSON envelope.");
            yield break;
        }

        RiddleGeneratorResponse parsed;
        try { parsed = UnityEngine.JsonUtility.FromJson<RiddleGeneratorResponse>(json); }
        catch { onError?.Invoke("Generator JSON parse failed."); yield break; }

        var riddle = new RiddleDefinition
        {
            id = string.IsNullOrWhiteSpace(parsed.id) ? Guid.NewGuid().ToString("N") : parsed.id,
            question = parsed.question,
            acceptanceCriteria = parsed.acceptanceCriteria,
            hint = parsed.hint
        };

        onRiddle?.Invoke(riddle);
    }
}
