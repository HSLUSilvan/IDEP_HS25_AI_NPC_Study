using UnityEngine;
using TMPro;

public class SendOnEnter : MonoBehaviour
{
    public TMP_InputField input;
    public ChatUiController controller;

    private void Update()
    {
        if (input != null && input.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            controller.OnSendClicked();
        }
    }
}
