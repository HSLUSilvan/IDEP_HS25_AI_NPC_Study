using UnityEngine;

public sealed class ChatBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject chatRoot;
    [SerializeField] private MonoBehaviour chatStarterComponent;

    private IChatStarter _starter;

    private void Awake()
    {
        chatRoot.SetActive(false);
        _starter = chatStarterComponent as IChatStarter;
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
        chatRoot.SetActive(true);
        _starter?.StartChatSession(provider);
    }
}
