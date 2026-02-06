using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor2D : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactCooldown = 0.2f;

    private IInteractable _current;
    private float _nextAllowedTime;

    public void OnJump()
    {

        Debug.Log("[PlayerInteractor2D] OnInteract PERFORMED");

        if (Time.time < _nextAllowedTime) return;
        _nextAllowedTime = Time.time + interactCooldown;

        if (_current != null)
            _current.Interact();
        else
            Debug.Log("[PlayerInteractor2D] Interact pressed but no interactable in range.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var i = other.GetComponentInParent<IInteractable>();
        if (i != null)
        {
            _current = i;
            Debug.Log("[PlayerInteractor2D] Found interactable: " + other.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var i = other.GetComponentInParent<IInteractable>();
        if (i != null && ReferenceEquals(i, _current))
        {
            _current = null;
            Debug.Log("[PlayerInteractor2D] Left interactable range: " + other.name);
        }
    }
}
