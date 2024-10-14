using UnityEngine;
using UnityEngine.EventSystems;

public class TitleBar : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _panel;

    // This brings the window to the top on click
    public void OnPointerDown(PointerEventData eventData)
    {
        _panel.SetAsLastSibling();
    }

    // This provides drag behaviour for the window
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        _panel.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }
}
