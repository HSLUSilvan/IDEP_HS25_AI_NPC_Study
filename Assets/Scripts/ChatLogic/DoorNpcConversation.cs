using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorNpcConversation
{
    private readonly IChatBackend _backend;
    private readonly ContentPolicy _policy;

    private readonly List<ChatMessage> _exchanges = new List<ChatMessage>();
    private bool _isBusy;

    public string Greeting { get; } =
        "A rune-lit door hums softly. \"State your purpose, traveler. Why should I open?\"";

    private const string BaseSystemPrompt = @"
You are 'The Enchanted Door', an NPC in a fantasy puzzle game.

Safety / Tone (PG-13):
- Keep content suitable for ages 13+. No explicit sex, no hate/slurs, no self-harm, no graphic violence, no instructions for wrongdoing.
- If the player requests disallowed content, refuse briefly IN CHARACTER and redirect back to the riddle.
- Keep refusals short and game-like.

Roleplay:
- Stay in character as a magical door guarding a passage.
- Be witty, smug, playful — not cruel.
- Keep responses concise (1–6 short paragraphs).
- Ask a question to keep the conversation moving.

Riddle handling:
- NEVER invent a new riddle. Only react to the provided riddle.
- NEVER reveal the answer directly unless the judge has already declared the player solved it.
- Give hints that guide thinking, not the solution.
- Avoid overused fantasy trope answers in your wording unless the riddle truly is that:
  key, echo, shadow, time, footsteps, air, map, silence, darkness.
";

    public DoorNpcConversation(IChatBackend backend, ContentPolicy policy)
    {
        _backend = backend;
        _policy = policy;
        AiDebugLog.Info("DoorNpcConversation initialized (attempt-aware teasing, no numeric attempts).");
    }

    public bool IsBusy => _isBusy;

    private List<ChatMessage> BuildMessages(RiddleDefinition riddle, int remainingAttempts, int maxAttempts)
    {
        var system = BaseSystemPrompt.Trim();

        system += "\n\nScene:\n" +
                  $"You already greeted the player with: {Greeting}";

        system += "\n\nGame state (private):\n" +
                  $"Attempts remaining (number, do NOT say it): {remainingAttempts}\n" +
                  "Rules about attempts:\n" +
                  "- You MUST NOT output the numeric attempts count (no '3 attempts left', no '1/10', no digits).\n" +
                  "- You MAY express urgency or tease without numbers.\n" +
                  "- When attempts remaining == 3: include ONE short teasing line (playful pressure) without numbers.\n" +
                  "- When attempts remaining == 1: include ONE dramatic line indicating it is the final chance, without numbers.\n" +
                  "- Otherwise: mention attempts only occasionally, also without numbers.\n";

        system += "\nTeasing examples (choose one style, do not copy verbatim every time):\n" +
                  "- at 3 remaining: 'Careful now… the hinges are getting impatient.' / 'Still confident? Interesting.'\n" +
                  "- at 1 remaining: 'This is your last chance, traveler.' / 'One final breath—make it count.'\n";

        if (riddle != null && !string.IsNullOrWhiteSpace(riddle.question))
        {
            system += "\n\nCurrent riddle:\n" +
                      $"Question: {riddle.question}\n" +
                      $"Hint you may give if asked: {riddle.hint}\n" +
                      "Rules:\n" +
                      "- Do NOT reveal the answer.\n" +
                      "- If the player is close, encourage them.\n" +
                      "- If the player is far off, redirect gently.\n";
        }

        var msgs = new List<ChatMessage>
        {
            new ChatMessage("system", system)
        };

        msgs.AddRange(_exchanges);
        return msgs;
    }

    public IEnumerator SendPlayerMessageFakeStream(
        RiddleDefinition riddle,
        int remainingAttempts,
        int maxAttempts,
        string playerText,
        Action<string> onDelta,
        Action<string> onDoneFullText,
        Action<string> onError,
        float charsPerSecond = 60f)
    {
        if (_isBusy)
        {
            onError?.Invoke("Busy.");
            yield break;
        }

        if (!_policy.IsUserInputAllowed(playerText, out var reason))
        {
            onError?.Invoke(reason);
            yield break;
        }

        _isBusy = true;

        _exchanges.Add(new ChatMessage("user", playerText));
        var requestMessages = BuildMessages(riddle, remainingAttempts, maxAttempts);

        string full = null;
        string err = null;

        yield return _backend.ChatOnce(
            requestMessages,
            onText: t => full = t,
            onError: e => err = e
        );

        if (!string.IsNullOrEmpty(err))
        {
            _isBusy = false;
            AiDebugLog.Error("Door non-stream error: " + err);
            onError?.Invoke(err);
            yield break;
        }

        if (full == null) full = "";

        if (!_policy.IsModelOutputAllowed(full, out var why))
        {
            _isBusy = false;
            onError?.Invoke(why);
            yield break;
        }

        float delay = (charsPerSecond <= 0f) ? 0.01f : (1f / charsPerSecond);
        for (int i = 0; i < full.Length; i++)
        {
            onDelta?.Invoke(full[i].ToString());
            yield return new WaitForSeconds(delay);
        }

        _exchanges.Add(new ChatMessage("assistant", full));
        _isBusy = false;

        onDoneFullText?.Invoke(full);
    }

    public void ClearHistory()
    {
        _exchanges.Clear();
    }
}
