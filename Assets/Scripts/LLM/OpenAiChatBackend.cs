using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAiChatBackend : IChatBackend
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly float _temperature;

    private const string Url = "https://api.openai.com/v1/chat/completions";

    [Serializable]
    private class OpenAiRequest
    {
        public string model;
        public OpenAiMsg[] messages;
        public float temperature;
        public bool stream;
    }

    [Serializable]
    private class OpenAiMsg
    {
        public string role;
        public string content;
    }
    private static readonly Regex JsonContentRegex =
        new Regex("\"content\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
            RegexOptions.Compiled | RegexOptions.Singleline);

    public OpenAiChatBackend(string apiKey, string model, float temperature)
    {
        _apiKey = apiKey;
        _model = model;
        _temperature = temperature;
    }

    public IEnumerator ChatOnce(List<ChatMessage> messages, Action<string> onText, Action<string> onError)
    {
        var sanitized = ChatMessageSanitizer.Sanitize(messages);

        var reqObj = new OpenAiRequest
        {
            model = _model,
            temperature = _temperature,
            stream = false,
            messages = ToOpenAiMsgs(sanitized)
        };

        string json = JsonUtility.ToJson(reqObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(Url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError ||
            req.result == UnityWebRequest.Result.ProtocolError)
        {
            onError?.Invoke($"OpenAI HTTP {req.responseCode}: {req.error}\n{req.downloadHandler?.text}");
            yield break;
        }

        string body = req.downloadHandler?.text ?? "";
        var content = ExtractLastContent(body);

        if (string.IsNullOrEmpty(content))
        {
            onError?.Invoke("OpenAI response parse failed.");
            yield break;
        }

        onText?.Invoke(content);
    }

    public IEnumerator ChatStream(List<ChatMessage> messages, Action<string> onDelta, Action onComplete, Action<string> onError)
    {
        onError?.Invoke("OpenAI streaming not implemented. Use non-stream + typewriter.");
        yield break;
    }

    private static OpenAiMsg[] ToOpenAiMsgs(List<ChatMessage> msgs)
    {
        var arr = new OpenAiMsg[msgs.Count];
        for (int i = 0; i < msgs.Count; i++)
        {
            arr[i] = new OpenAiMsg { role = msgs[i].role, content = msgs[i].content };
        }
        return arr;
    }

    private static string ExtractLastContent(string body)
    {
        Match last = null;
        foreach (Match m in JsonContentRegex.Matches(body))
            last = m;

        if (last == null) return null;

        return VllmStreamDeltaParser.JsonStringUnescape(last.Groups[1].Value);
    }
}
