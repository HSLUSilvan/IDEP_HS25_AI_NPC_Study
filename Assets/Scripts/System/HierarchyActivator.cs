using UnityEngine;

public class HierarchyActivator : MonoBehaviour
{
    [Tooltip("If true, enables all children (including inactive) when this object is enabled.")]
    [SerializeField] private bool enableChildrenOnEnable = true;

    private void OnEnable()
    {
        if (!enableChildrenOnEnable) return;

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
        }
    }
}
