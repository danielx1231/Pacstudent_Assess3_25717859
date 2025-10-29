using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverBorderThicken : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Mask to shrink/expand (makes the border look thicker)")]
    public RectTransform innerMask;   // Reference to InnerMask
    [Header("Normal / Highlight thickness (px)")]
    public float normal = 12f;
    public float highlighted = 24f;
    [Header("Lerp speed")]
    public float lerpSpeed = 16f;

    float target;
    RectTransform _rt;

    void Awake()
    {
        if (!innerMask) innerMask = transform.Find("InnerMask") as RectTransform;
        _rt = innerMask;
        target = normal;
        ApplyPadding(target, true);
    }

    void Update()
    {
        // Smooth transition
        float current = _rt.offsetMin.x; // Current left value
        float next = Mathf.Lerp(current, target, Time.unscaledDeltaTime * lerpSpeed);
        ApplyPadding(next, false);
    }

    void ApplyPadding(float px, bool force)
    {
        // Bottom-left = offsetMin; Top-right = offsetMax (negative)
        Vector2 min = new Vector2(px, px);
        Vector2 max = new Vector2(-px, -px);
        if (force || _rt.offsetMin != min || _rt.offsetMax != max)
        {
            _rt.offsetMin = min;
            _rt.offsetMax = max;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => target = highlighted;
    public void OnPointerExit(PointerEventData eventData) => target = normal;
}
