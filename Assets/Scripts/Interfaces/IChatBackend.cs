using System;
using System.Collections;
using System.Collections.Generic;

public interface IChatBackend
{
    IEnumerator ChatOnce(
        List<ChatMessage> messages,
        Action<string> onText,
        Action<string> onError);

    IEnumerator ChatStream(
        List<ChatMessage> messages,
        Action<string> onDelta,
        Action onComplete,
        Action<string> onError);
}
