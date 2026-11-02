using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Una pieza del desvio de energia. PowerPuzzleView clona una por casillero.
public class PowerTile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image frame;
    [SerializeField] private RectTransform arms; // se gira entero, las patas quedan fijas adentro
    [SerializeField] private Image upArm;
    [SerializeField] private Image rightArm;
    [SerializeField] private Image downArm;
    [SerializeField] private Image dot;
    [SerializeField] private float turnSpeed = 900f; // grados por segundo

    public Action<PowerTile> Clicked;
    public Vector2Int Cell { get; private set; }

    private float targetAngle;

    public void Setup(Vector2Int cell, PowerPieceType type, int rotation)
    {
        Cell = cell;
        rightArm.gameObject.SetActive(type != PowerPieceType.Straight);
        downArm.gameObject.SetActive(type != PowerPieceType.Corner);

        targetAngle = -90f * rotation;
        arms.localEulerAngles = new Vector3(0f, 0f, targetAngle);
    }

    public void TurnTo(int rotation)
    {
        targetAngle = -90f * rotation;
    }

    public void Show(Color pieceColor, Color frameColor)
    {
        upArm.color = rightArm.color = downArm.color = dot.color = pieceColor;
        frame.color = frameColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }

    private void Update()
    {
        float current = arms.localEulerAngles.z;
        if (Mathf.Approximately(Mathf.DeltaAngle(current, targetAngle), 0f)) return;

        // va por el lado corto, que siempre es el horario porque gira de a 90
        arms.localEulerAngles = new Vector3(0f, 0f, Mathf.MoveTowardsAngle(current, targetAngle, turnSpeed * Time.unscaledDeltaTime));
    }
}
