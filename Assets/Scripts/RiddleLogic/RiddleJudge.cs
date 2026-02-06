using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiddleJudge
{
    private readonly IChatBackend _backend;

    private const string JudgeSystemPrompt = @"
You are a strict puzzle judge. You do NOT roleplay.

Output a JSON object wrapped in either:
1) <JSON> ... </JSON>
or
2) ```json ... ```

Schema:
{
  ""solved"": boolean,
  ""confidence"": number,
  ""reason"": string
}

Rules:
- solved=true only if the PLAYER'S LAST ANSWER satisfies the riddle acceptance criteria.
- Be strict. If unsure, solved=false.
";

    public RiddleJudge(IChatBackend backend)
    {
        _backend = backend;
    }

    public IEnumerator Evaluate(
        RiddleDefinition riddle,
        string playerLastAnswer,
        Action<JudgeResponse> onResult,
        Action<string> onError)
    {
        if (riddle == null)
        {
            onError?.Invoke("Judge called but CurrentRiddle is null (no riddle has been generated yet).");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(riddle.question) || string.IsNullOrWhiteSpace(riddle.acceptanceCriteria))
        {
            onError?.Invoke("Judge called with incomplete riddle (question/acceptanceCriteria missing).");
            yield break;
        }

        var userBlock =
            "RIDDLE:\n" +
            $"Question: {riddle.question}\n" +
            $"Acceptance criteria: {riddle.acceptanceCriteria}\n\n" +
            "PLAYER_LAST_ANSWER:\n" + playerLastAnswer;

        var messages = new List<ChatMessage>
        {
            new ChatMessage("system", JudgeSystemPrompt),
            new ChatMessage("user", userBlock)
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
            onError?.Invoke("Judge returned no JSON envelope.");
            yield break;
        }

        JudgeResponse parsed;
        try { parsed = JsonUtility.FromJson<JudgeResponse>(json); }
        catch { onError?.Invoke("Judge JSON parse failed."); yield break; }

        onResult?.Invoke(parsed);
    }
}
