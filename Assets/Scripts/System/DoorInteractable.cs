using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private ChatSystemActivator chatActivator;
    [SerializeField] private RiddleGameController gameController;

    public void Interact()
    {
        if (chatActivator == null)
        {
            Debug.LogWarning("[DoorInteractable] chatActivator not assigned.");
            return;
        }

        chatActivator.ShowChat();

        if (gameController != null)
            gameController.BeginConversation();
        else
            Debug.LogWarning("[DoorInteractable] gameController not assigned (riddle will not start).");
    }
}
