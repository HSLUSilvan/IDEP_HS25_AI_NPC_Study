using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatScrollController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private ScrollRect scrollRect;

    [Range(0f, 0.5f)]
    [SerializeField] private float autoScrollThreshold = 0.08f;

    [SerializeField] private bool _userDragging;
    [SerializeField] private bool _shouldAutoScroll = true;
    [SerializeField] private bool _forceFollow;
    [SerializeField] private Coroutine _scrollCoroutine;

    private void Awake()
    {
        if (scrollRect == null)
        {
            Debug.LogError("[ChatScrollController] ScrollRect not assigned.");
            enabled = false;
            return;
        }

        scrollRect.onValueChanged.AddListener(_ => OnScrollValueChanged());
    }

    private void OnDestroy()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(_ => OnScrollValueChanged());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _userDragging = true;

        if (!_forceFollow)
            _shouldAutoScroll = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _userDragging = false;
        if (!_forceFollow && IsNearBottom())
            _shouldAutoScroll = true;
    }

    private void OnScrollValueChanged()
    {
        if (_userDragging) return;
        if (_forceFollow) return;
        _shouldAutoScroll = IsNearBottom();
    }

    public void BeginAutoFollow()
    {
        _forceFollow = true;
        _shouldAutoScroll = true;
        NotifyContentChanged(forceToBottom: true);
    }

    public void EndAutoFollow()
    {
        _forceFollow = false;

        _shouldAutoScroll = IsNearBottom();
    }

    public void NotifyContentChanged(bool forceToBottom = false)
    {
        if (!CanRunCoroutine())
            return;

        if (_forceFollow || forceToBottom)
        {
            StartScrollCoroutine();
            return;
        }
        if (_userDragging || !_shouldAutoScroll)
            return;

        StartScrollCoroutine();
    }

    private void StartScrollCoroutine()
    {
        if (_scrollCoroutine != null)
            StopCoroutine(_scrollCoroutine);

        _scrollCoroutine = StartCoroutine(ScrollToBottomAfterLayout());
    }

    private IEnumerator ScrollToBottomAfterLayout()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        yield return null;

        scrollRect.verticalNormalizedPosition = 0f;
        _scrollCoroutine = null;
    }

    private bool IsNearBottom()
    {
        return scrollRect.verticalNormalizedPosition <= autoScrollThreshold;
    }

    private bool CanRunCoroutine()
    {
        return isActiveAndEnabled &&
               gameObject.activeInHierarchy &&
               scrollRect != null &&
               scrollRect.gameObject.activeInHierarchy &&
               scrollRect.content != null;
    }
}
