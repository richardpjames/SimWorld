using UnityEngine;
using UnityEngine.EventSystems;

public class TitleBar : MonoBehaviour, IDragHandler
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _panel;
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        _panel.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }
}
