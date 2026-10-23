using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lógica del puzzle binario de una puerta.
/// Recibe el código que escribió el jugador (por ejemplo "00101010"), lo convierte a número
/// y lo compara con el objetivo. Si acierta dispara onSolved y si falla dispara onFailed.
/// </summary>
public class BinaryPanel : MonoBehaviour
{
    [SerializeField] private BinaryPuzzleData puzzleData;

    // UnityEvents: se conectan desde el Inspector (abrir puerta, sonido, alertar al enemigo, etc.)
    public UnityEvent onSolved;
    public UnityEvent onFailed;

    private bool isSolved;

    public bool IsSolved => isSolved;
    public BinaryPuzzleData Data => puzzleData;

    private void Awake()
    {
        if (puzzleData == null)
        {
            Debug.LogError("BinaryPanel: no BinaryPuzzleData assigned in " + gameObject.name, this);
        }
    }

    // Recibe lo que escribió el jugador, por ejemplo "00101010" o "101", y lo envía.
    // Si tiene algo que no sea 0 o 1, está vacío o es más largo que la cantidad de bits, cuenta como intento fallido.
    public void SubmitBinaryString(string binary)
    {
        if (puzzleData == null || isSolved) return;

        if (!TryParseBinary(binary, puzzleData.BitCount, out int value))
        {
            Debug.Log("BinaryPanel: invalid binary input '" + binary + "'");
            onFailed.Invoke();
            return;
        }

        CheckValue(value);
    }

    // Compara el valor contra el objetivo y avisa el resultado
    private void CheckValue(int value)
    {
        if (value == puzzleData.TargetValue)
        {
            isSolved = true;
            Debug.Log("BinaryPanel: solved with " + value);
            onSolved.Invoke();
        }
        else
        {
            Debug.Log("BinaryPanel: wrong value " + value + " (target is " + puzzleData.TargetValue + ")"); // se puede quitar en la versión final para no dar pistas
            onFailed.Invoke();
        }
    }

    // Lee los dígitos de izquierda a derecha: en cada paso duplica lo acumulado y suma el dígito nuevo.
    // Ejemplo "101": 0 -> 0*2+1 = 1 -> 1*2+0 = 2 -> 2*2+1 = 5
    private static bool TryParseBinary(string binary, int maxBits, out int value)
    {
        value = 0;

        if (string.IsNullOrEmpty(binary) || binary.Length > maxBits) return false;

        foreach (char digit in binary)
        {
            if (digit != '0' && digit != '1') return false;
            value = (value << 1) | (digit - '0');
        }

        return true;
    }
}