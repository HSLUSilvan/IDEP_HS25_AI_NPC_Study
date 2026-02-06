using UnityEngine;
using UnityEngine.InputSystem;

public class InputProbe : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        Debug.Log("[InputProbe] Awake on " + gameObject.name);
    }

    private void Start()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("[InputProbe] PlayerInput/actions missing.");
            return;
        }

        var a = playerInput.actions.FindAction("Interact", true);
        if (a == null)
        {
            Debug.LogError("[InputProbe] Could not find action 'Interact' in PlayerInput actions.");
            return;
        }

        Debug.Log("[InputProbe] Found action 'Interact'. Enabled=" + a.enabled);

        a.performed += ctx => Debug.Log("[InputProbe] Interact performed (direct hook)");
        a.Enable();
    }
}
