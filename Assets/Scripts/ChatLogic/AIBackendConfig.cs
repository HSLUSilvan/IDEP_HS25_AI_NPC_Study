using UnityEngine;

public class AiBackendConfig : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool useOpenAi = false;

    [Header("Common")]
    [SerializeField] private float temperature = 0.7f;

    [Header("OpenAI")]
    [SerializeField] private string openAiApiKey = "";
    [SerializeField] private string openAiModel = "gpt-4o-mini";

    [Header("vLLM / Apertus")]
    [SerializeField] private string vllmBaseUrl = "https://apertus.mediadock.space";
    [SerializeField] private string vllmChatPath = "/v1/chat/completions";
    [SerializeField] private string vllmApiKey = "";
    [SerializeField] private string vllmModel = "swiss-ai/Apertus-8B-Instruct-2509";

    public bool UseOpenAi => useOpenAi;
    public float Temperature => temperature;

    public string OpenAiApiKey => openAiApiKey;
    public string OpenAiModel => openAiModel;

    public string VllmBaseUrl => vllmBaseUrl;
    public string VllmChatPath => vllmChatPath;
    public string VllmApiKey => vllmApiKey;
    public string VllmModel => vllmModel;

    public void SetUseOpenAi(bool enabled)
    {
        useOpenAi = enabled;
    }
}
