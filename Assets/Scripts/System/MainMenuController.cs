using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot;

    private void Awake()
    {
        menuRoot.SetActive(true);
    }

    private void OnEnable()
    {
        AppEvents.ChatStartRequested += OnChatStartRequested;
    }

    private void OnDisable()
    {
        AppEvents.ChatStartRequested -= OnChatStartRequested;
    }

    private void OnChatStartRequested(ChatProvider provider)
    {
        menuRoot.SetActive(false);
    }
}
