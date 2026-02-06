using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BackendHealthChecker : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private AiBackendConfig config;

    [Header("UI")]
    [SerializeField] private Button openAiButton;
    [SerializeField] private Button apertusButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Networking")]
    [Tooltip("Seconds before a backend check is considered failed due to timeout.")]
    [SerializeField] private int timeoutSeconds = 8;

    private void Awake()
    {
        SetButton(openAiButton, false);
        SetButton(apertusButton, false);

        if (statusText != null)
            statusText.text = "Checking connections…";
    }

    private void Start()
    {
        StartCoroutine(RunChecks());
    }

    private IEnumerator RunChecks()
    {
        bool openAiDone = false;
        bool apertusDone = false;

        HealthResult openAiResult = default;
        HealthResult apertusResult = default;

        StartCoroutine(CheckOpenAI(r => { openAiResult = r; openAiDone = true; }));
        StartCoroutine(CheckApertus(r => { apertusResult = r; apertusDone = true; }));

        while (!openAiDone || !apertusDone)
        {
            if (statusText != null)
            {
                statusText.text =
                    "Checking connections…\n" +
                    $"OpenAI: {(openAiDone ? ResultBadge(openAiResult.Ok) : "")}\n" +
                    $"Apertus: {(apertusDone ? ResultBadge(apertusResult.Ok) : "")}";
            }
            yield return null;
        }

        // Enable buttons ONLY when their backend is OK
        SetButton(openAiButton, openAiResult.Ok);
        SetButton(apertusButton, apertusResult.Ok);

        if (statusText != null)
        {
            statusText.text =
                $"{FormatLine("OpenAI", openAiResult)}\n" +
                $"{FormatLine("Apertus", apertusResult)}\n\n";
        }
    }

    private static void SetButton(Button button, bool enabled)
    {
        if (button == null) return;
        button.interactable = enabled;
    }

    // Keep your strings exactly; this was empty before, so keep it empty.
    private static string ResultBadge(bool ok) => ok ? "" : "";

    private static string FormatLine(string name, HealthResult r)
    {
        if (r.Ok) return $"{name}: {r.UserMessage}";
        if (r.HttpStatus != 0) return $"{name}:  {r.UserMessage} (HTTP {r.HttpStatus})";
        return $"{name}:  {r.UserMessage}";
    }

    private IEnumerator CheckOpenAI(Action<HealthResult> done)
    {
        if (config == null)
        {
            done(new HealthResult(false, "Config missing.", 0));
            yield break;
        }

        // ✅ Encapsulation fix: use property/getter (NOT private fields)
        string apiKey = config.OpenAiApiKey;

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("PASTE_OPENAI_KEY"))
        {
            done(new HealthResult(false, "API key not set.", 0));
            yield break;
        }

        const string url = "https://api.openai.com/v1/models";

        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = timeoutSeconds;
            req.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                done(new HealthResult(true, "Connected successfully.", (int)req.responseCode));
                yield break;
            }

            // Common cases
            int code = (int)req.responseCode;
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.DataProcessingError)
            {
                done(new HealthResult(false, "Network error (check internet connection).", code));
                yield break;
            }

            if (code == 401)
            {
                done(new HealthResult(false, "Unauthorized (check API key).", code));
                yield break;
            }

            if (code == 429)
            {
                done(new HealthResult(false, "Rate limited (try again later).", code));
                yield break;
            }

            done(new HealthResult(false, "Request failed.", code));
        }
    }

    private IEnumerator CheckApertus(Action<HealthResult> done)
    {
        if (config == null)
        {
            done(new HealthResult(false, "Config missing.", 0));
            yield break;
        }

        // ✅ Encapsulation fix: use properties/getters
        string baseUrlRaw = config.VllmBaseUrl;
        string vllmKey = config.VllmApiKey;

        if (string.IsNullOrWhiteSpace(baseUrlRaw))
        {
            done(new HealthResult(false, "Base URL not set.", 0));
            yield break;
        }

        string baseUrl = baseUrlRaw.TrimEnd('/');
        string url = baseUrl + "/v1/models";

        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = timeoutSeconds;

            if (!string.IsNullOrWhiteSpace(vllmKey) && !vllmKey.Contains("PASTE_VLLM_KEY"))
                req.SetRequestHeader("Authorization", $"Bearer {vllmKey}");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                done(new HealthResult(true, "Connected successfully.", (int)req.responseCode));
                yield break;
            }

            int code = (int)req.responseCode;

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.DataProcessingError)
            {
                done(new HealthResult(false, "Network error (MediaDock server not reachable. Check HSLU VPN connection.).", code));
                yield break;
            }

            if (code == 401)
            {
                done(new HealthResult(false, "Unauthorized (check API key).", code));
                yield break;
            }

            if (code == 404)
            {
                done(new HealthResult(false, "Endpoint not found (/v1/models).", code));
                yield break;
            }

            done(new HealthResult(false, "Request failed.", code));
        }
    }

    private readonly struct HealthResult
    {
        public readonly bool Ok;
        public readonly string UserMessage;
        public readonly int HttpStatus;

        public HealthResult(bool ok, string userMessage, int httpStatus)
        {
            Ok = ok;
            UserMessage = userMessage;
            HttpStatus = httpStatus;
        }
    }
}
