using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputFocusBlocker : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PlayerInput playerInput;

    [Header("Action Map Names")]
    [Tooltip("Gameplay action map (Move/Interact/etc.)")]
    [SerializeField] private string gameplayMap = "Player";

    [Tooltip("UI-only action map (can be empty if you don't use one)")]
    [SerializeField] private string uiMap = ""; // optional

    private bool _wasFocused;

    private void Update()
    {
        if (inputField == null || playerInput == null) return;

        bool focused = inputField.isFocused;

        // On focus gained: disable gameplay actions
        if (focused && !_wasFocused)
        {
            playerInput.actions.FindActionMap(gameplayMap, true).Disable();
        }
        // On focus lost: re-enable gameplay actions
        else if (!focused && _wasFocused)
        {
            playerInput.actions.FindActionMap(gameplayMap, true).Enable();
        }

        _wasFocused = focused;
    }
}
