
using UnityEngine;

public class ChatSystemActivator : MonoBehaviour
{
    [Header("Chat UI Root (Panel/Canvas)")]
    [SerializeField] private GameObject chatUiRoot;
    [SerializeField] private bool startHidden = true;

    private void Awake()
    {
        if (chatUiRoot == null)
            chatUiRoot = gameObject;

        if (startHidden)
            chatUiRoot.SetActive(false);

        Debug.Log($"[ChatSystemActivator] Awake on {gameObject.name} chatUiRoot={chatUiRoot.name} active={chatUiRoot.activeSelf}");
    }

    public void ShowChat()
    {
        if (chatUiRoot == null)
        {
            Debug.LogError("[ChatSystemActivator] chatUiRoot is NULL");
            return;
        }

        chatUiRoot.SetActive(true);
        Debug.Log($"[ChatSystemActivator] ShowChat -> {chatUiRoot.name} active={chatUiRoot.activeSelf}");
    }

    public void HideChat()
    {
        if (chatUiRoot == null)
        {
            Debug.LogError("[ChatSystemActivator] chatUiRoot is NULL");
            return;
        }

        chatUiRoot.SetActive(false);
        Debug.Log($"[ChatSystemActivator] HideChat -> {chatUiRoot.name} active={chatUiRoot.activeSelf}");
    }

    public bool IsVisible => chatUiRoot != null && chatUiRoot.activeSelf;
}
