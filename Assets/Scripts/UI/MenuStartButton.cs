using UnityEngine;

public sealed class MenuStartButton : MonoBehaviour
{
    [SerializeField] private bool selectOpenAI;

    [Header("References")]
    [SerializeField] private AiBackendConfig backendConfig;
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject chatSystem;
    [SerializeField] private GameObject roomRoot;

    public void OnClick()
    {
        Debug.Log("[MenuStartButton] Clicked");

        if (backendConfig == null)
        {
            Debug.LogError("[MenuStartButton] backendConfig not assigned.");
            return;
        }

        backendConfig.SetUseOpenAi(selectOpenAI);

        if (mainMenuRoot != null) mainMenuRoot.SetActive(false);
        if (chatPanel != null) chatPanel.SetActive(true);
        if (roomRoot != null) roomRoot.SetActive(true);
        if (chatSystem != null) chatSystem.SetActive(true);

        Debug.Log($"[MenuStartButton] Mode={(backendConfig.UseOpenAi ? "OpenAI" : "vLLM")} roomRootActive={(roomRoot != null && roomRoot.activeSelf)} chatSystemActive={(chatSystem != null && chatSystem.activeSelf)}");
    }
}
