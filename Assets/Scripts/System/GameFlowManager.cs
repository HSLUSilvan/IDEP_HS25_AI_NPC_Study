using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private AiBackendConfig backendConfig;

    [Header("Roots")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject roomRoot;
    [SerializeField] private GameObject chatSystemRoot;
    [SerializeField] private GameObject chatPanelRoot; 

    private void Awake()
    {
        if (backendConfig == null)
            backendConfig = FindFirstObjectByType<AiBackendConfig>();
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);
        if (roomRoot != null) roomRoot.SetActive(false);
        if (chatSystemRoot != null) chatSystemRoot.SetActive(false);
        if (chatPanelRoot != null) chatPanelRoot.SetActive(false);
    }

    public void StartGameOpenAI()
    {
        if (backendConfig == null)
        {
            Debug.LogError("[GameFlowManager] backendConfig missing.");
            return;
        }

        backendConfig.SetUseOpenAi(true);
        StartGame();
    }

    public void StartGameVllm()
    {
        if (backendConfig == null)
        {
            Debug.LogError("[GameFlowManager] backendConfig missing.");
            return;
        }

        backendConfig.SetUseOpenAi(false);
        StartGame();
    }

    public void StartGame()
    {
        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        if (roomRoot != null) roomRoot.SetActive(true);
        if (chatSystemRoot != null) chatSystemRoot.SetActive(true);
        if (chatPanelRoot != null) chatPanelRoot.SetActive(false);

        Debug.Log($"[GameFlowManager] StartGame mode={(backendConfig != null && backendConfig.UseOpenAi ? "OpenAI" : "vLLM")}");
    }

    public void ReturnToMenu()
    {
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);
        if (roomRoot != null) roomRoot.SetActive(false);
        if (chatSystemRoot != null) chatSystemRoot.SetActive(false);
        if (chatPanelRoot != null) chatPanelRoot.SetActive(false);

        Debug.Log("[GameFlowManager] ReturnToMenu");
    }
}
