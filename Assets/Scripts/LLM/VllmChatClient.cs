using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public sealed class VllmChatClient
{
    private readonly string _url;
    private readonly string _apiKey;      
    private readonly string _model;
    private readonly float _temperature;

    public VllmChatClient(string baseUrl, string chatPath, string apiKey, string model, float temperature)
    {
        _url = $"{baseUrl.TrimEnd('/')}{chatPath}";
        _apiKey = apiKey ?? "";
        _model = model;
        _temperature = temperature;
    }

    public VllmChatClient()
    {
        _url = "https://apertus.mediadock.space/v1/chat/completions";
        _apiKey = "";
        _model = "swiss-ai/Apertus-8B-Instruct-2509";
        _temperature = 0.7f;
    }

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public bool stream;
        public float temperature;
    }

    private static readonly Regex ContentRegex =
        new Regex("\"content\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
            RegexOptions.Compiled | RegexOptions.Singleline);

    public IEnumerator SendChatOnce(
        List<ChatMessage> messages,
        Action<string> onText,
        Action<string> onError)
    {
        if (messages == null || messages.Count == 0)
        {
            onError?.Invoke("No messages provided.");
            yield break;
        }
        var sanitized = ChatMessageSanitizer.Sanitize(messages);

        var reqObj = new ChatRequest
        {
            model = _model,
            messages = sanitized.ToArray(),
            stream = false,
            temperature = _temperature
        };

        string json = JsonUtility.ToJson(reqObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrWhiteSpace(_apiKey))
            req.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError ||
            req.result == UnityWebRequest.Result.ProtocolError)
        {
            string body = req.downloadHandler?.text ?? "";
            onError?.Invoke($"vLLM HTTP {req.responseCode}: {req.error}\n{Truncate(body, 1200)}");
            yield break;
        }

        string response = req.downloadHandler?.text ?? "";
        string content = ExtractLastContent(response);

        if (string.IsNullOrEmpty(content))
        {
            onError?.Invoke("vLLM response parse failed (no content).");
            yield break;
        }

        onText?.Invoke(content);
    }

    private static string ExtractLastContent(string body)
    {
        if (string.IsNullOrEmpty(body)) return null;

        Match last = null;
        foreach (Match m in ContentRegex.Matches(body))
            last = m;

        if (last == null) return null;

        return VllmStreamDeltaParser.JsonStringUnescape(last.Groups[1].Value);
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }
}
