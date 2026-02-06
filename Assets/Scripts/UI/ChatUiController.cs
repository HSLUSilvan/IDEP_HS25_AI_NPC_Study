using UnityEngine;

public class ChatUiController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ChatUiView view;
    [SerializeField] private RiddleGameController game;

    private void Awake()
    {
        if (view == null)
        {
            Debug.LogError("[AI] ChatUiController: view reference missing.");
            return;
        }

        if (game == null)
        {
            Debug.LogError("[AI] ChatUiController: game reference missing.");
            return;
        }

        AiDebugLog.Info("ChatUiController initialized (routes input to RiddleGameController).");
    }

    public void OnSendClicked()
    {
        if (view == null || game == null) return;

        if (game.IsBusy)
        {
            view.SetStatus("Wait...");
            return;
        }

        var text = view.GetInputTextTrimmed();
        if (string.IsNullOrWhiteSpace(text))
        {
            view.SetStatus("Type something first.");
            return;
        }

        AiDebugLog.Info($"UI send: \"{text}\"");
        view.ClearInputAndFocus();
        game.SubmitPlayerMessage(text);
    }
}
