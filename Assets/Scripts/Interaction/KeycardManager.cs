using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Singleton que guarda qué tarjetas de acceso recolectó el jugador durante
/// la partida. Las puertas consultan HasKeycard() para decidir si desbloquean
/// solas al interactuar.
/// </summary>
public class KeycardManager : MonoBehaviour
{
    public static KeycardManager Instance { get; private set; }


    // HashSet en vez de List: no permite duplicados y Contains() es más
    // rápido, ideal para un chequeo tipo "¿tiene esta tarjeta o no?".
    private readonly HashSet<string> collectedKeycards = new HashSet<string>();

    private void Awake()
    {
        // Singleton simple: si ya existe una instancia (por ejemplo al
        // recargar la escena), destruye la nueva para no tener dos managers
        // compitiendo entre sí.

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CollectKeycard(string keycardId)
    {
        collectedKeycards.Add(keycardId);
        Debug.Log($"Tarjeta recogida: {keycardId}");
    }

    public bool HasKeycard(string keycardId)
    {
        return collectedKeycards.Contains(keycardId);
    }
}