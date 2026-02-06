using UnityEngine;

public sealed class ChatPlayController : MonoBehaviour, IChatStarter
{
    public void StartChatSession(ChatProvider provider)
    {

        Debug.Log($"Chat started with {provider}");
    }
}
