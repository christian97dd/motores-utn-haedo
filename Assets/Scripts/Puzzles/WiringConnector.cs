using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Un conector del cableado. Los de la izquierda se arrastran, los de la derecha reciben el cable.
public class WiringConnector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image frame;
    [SerializeField] private Image dot;
    [SerializeField] private Image stub; // el pedacito de cable que asoma del conector
    [SerializeField] private TMP_Text label; // solo lo usan los de la izquierda

    public Action<WiringConnector, PointerEventData> DragStarted;
    public Action<WiringConnector, PointerEventData> Dragged;
    public Action<WiringConnector, PointerEventData> DragEnded;

    public int Index { get; private set; }

    // donde arranca o termina el cable, en el espacio del padre
    public Vector2 CablePoint => ((RectTransform)transform).anchoredPosition + stub.rectTransform.anchoredPosition;

    public void Setup(int index, string name, Color wireColor)
    {
        Index = index;
        dot.color = wireColor;
        stub.color = wireColor;
        if (label != null) label.text = name;
    }

    public void SetFrameColor(Color color)
    {
        frame.color = color;
    }

    public void OnBeginDrag(PointerEventData eventData) => DragStarted?.Invoke(this, eventData);
    public void OnDrag(PointerEventData eventData) => Dragged?.Invoke(this, eventData);
    public void OnEndDrag(PointerEventData eventData) => DragEnded?.Invoke(this, eventData);
}
