using UnityEngine;

/// <summary>
/// Lógica del puzzle binario de una puerta.
/// Recibe el código que escribió el jugador (por ejemplo "00101010"), lo convierte a número
/// y lo compara con el objetivo. Si acierta dispara onSolved y si falla dispara onFailed.
/// </summary>
public class BinaryPanel : Puzzle
{
    [SerializeField] private BinaryPuzzleData puzzleData;

    public BinaryPuzzleData Data => puzzleData;

    private void Awake()
    {
        if (puzzleData == null)
        {
            Debug.LogError("BinaryPanel: no BinaryPuzzleData assigned in " + gameObject.name, this);
        }
    }

    // Recibe lo que escribió el jugador, por ejemplo "00101010".
    // Tiene que tener exactamente la cantidad de bits del puzzle: el primer dígito es el de más peso.
    public void SubmitBinaryString(string binary)
    {
        if (puzzleData == null || IsSolved) return;

        if (!TryParseBinary(binary, puzzleData.BitCount, out int value))
        {
            Debug.Log("BinaryPanel: invalid binary input '" + binary + "'");
            Fail();
            return;
        }

        if (value == puzzleData.TargetValue)
        {
            Solve();
        }
        else
        {
            Fail();
        }
    }

    // Lee los dígitos de izquierda a derecha: en cada paso duplica lo acumulado y suma el dígito nuevo.
    // Ejemplo "101": 0 -> 0*2+1 = 1 -> 1*2+0 = 2 -> 2*2+1 = 5
    private static bool TryParseBinary(string binary, int bitCount, out int value)
    {
        value = 0;

        // con menos digitos el valor no coincidiria con los pesos que muestra la pantalla
        if (string.IsNullOrEmpty(binary) || binary.Length != bitCount) return false;

        foreach (char digit in binary)
        {
            if (digit != '0' && digit != '1') return false;
            value = (value << 1) | (digit - '0');
        }

        return true;
    }
}
