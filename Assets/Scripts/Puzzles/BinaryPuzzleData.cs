using UnityEngine;

/// <summary>
/// Datos de un puzzle binario de puerta.
/// Se crea un asset por puerta: clic derecho > Create > Protocol HIDE > Binary Puzzle Data.
/// </summary>
[CreateAssetMenu(fileName = "NewBinaryPuzzle", menuName = "Protocol HIDE/Binary Puzzle Data")]
public class BinaryPuzzleData : ScriptableObject
{
    [Header("Configuración del puzzle")]
    [Range(1, 8)]
    [SerializeField] private int bitCount = 8;

    [SerializeField] private int targetValue = 42;

    [Header("Ayudas en pantalla")]
    [Tooltip("Muestra debajo de cada casillero cuánto vale ese bit (128, 64, 32...)")]
    [SerializeField] private bool showBitWeights = true;
    [Tooltip("Muestra el valor en decimal de lo que se va escribiendo")]
    [SerializeField] private bool showDecimalValue = true;

    public int BitCount => bitCount;
    public int TargetValue => targetValue;
    public bool ShowBitWeights => showBitWeights;
    public bool ShowDecimalValue => showDecimalValue;

    // Mayor número que entra en bitCount bits (8 bits = 255)
    public int MaxValue => (1 << bitCount) - 1;

    // Mantiene el objetivo dentro del rango válido mientras se edita en el Inspector
    private void OnValidate()
    {
        targetValue = Mathf.Clamp(targetValue, 0, MaxValue);
    }
}