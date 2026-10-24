using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Un casillero del puzzle binario. BinaryPuzzleView clona uno por cada bit.
public class BinaryBitCell : MonoBehaviour
{
    [SerializeField] private Image frame;
    [SerializeField] private TMP_Text digitLabel;
    [SerializeField] private GameObject cursor;     // la rayita que titila en el casillero que sigue
    [SerializeField] private TMP_Text weightLabel;

    public void Setup(int weight, bool showWeight)
    {
        weightLabel.text = weight.ToString();
        weightLabel.gameObject.SetActive(showWeight);
    }

    public void Show(char digit, bool isNext, Color frameColor, Color digitColor)
    {
        bool filled = digit != '\0';
        digitLabel.text = filled ? digit.ToString() : "";
        digitLabel.color = digitColor;
        cursor.SetActive(isNext);
        frame.color = frameColor;
    }
}
