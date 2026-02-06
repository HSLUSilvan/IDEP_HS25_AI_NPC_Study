using TMPro;
using UnityEngine;

public class ChatUiView : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text chatOutput;
    [SerializeField] private TMP_Text statusText;

    public void AppendToChat(string text)
    {
        if (chatOutput == null) return;
        chatOutput.text += text;
    }

    public void AppendLine(string line)
    {
        if (chatOutput == null) return;
        chatOutput.text += "\n" + line;
    }

    public void SetChatText(string fullText)
    {
        if (chatOutput == null) return;
        chatOutput.text = fullText ?? "";
    }

    public void SetStatus(string text)
    {
        if (statusText != null) statusText.text = text;
    }

    public string GetInputTextTrimmed()
    {
        if (inputField == null) return null;
        return inputField.text != null ? inputField.text.Trim() : null;
    }

    public void ClearInputAndFocus()
    {
        if (inputField == null) return;
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public bool HasInputFieldSelected()
    {
        if (inputField == null) return false;
        return inputField.isFocused;
    }
}
