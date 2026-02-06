using System;
using System.Collections;
using System.Collections.Generic;

public sealed class VllmChatBackend : IChatBackend
{
    private readonly VllmChatClient _client;

    public VllmChatBackend(VllmChatClient client)
    {
        _client = client;
    }

    public IEnumerator ChatOnce(List<ChatMessage> messages, Action<string> onText, Action<string> onError)
    {
        yield return _client.SendChatOnce(messages, onText, onError);
    }

    public IEnumerator ChatStream(List<ChatMessage> messages, Action<string> onDelta, Action onComplete, Action<string> onError)
    {
        onError?.Invoke("vLLM streaming is disabled. Use ChatOnce + typewriter.");
        yield break;
    }
}
