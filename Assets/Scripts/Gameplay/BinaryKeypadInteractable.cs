using UnityEngine;

/// <summary>
/// Va en el panel de la pared que mira el jugador. PlayerInteraction llama a Interact(),
/// que abre el teclado en pantalla.
/// IMPORTANTE: este script debe estar en el mismo GameObject que tiene el Collider.
/// </summary>
public class BinaryKeypadInteractable : MonoBehaviour
{
    [SerializeField] private BinaryKeypadUI keypad;

    // Lo llama PlayerInteraction
    public void Interact()
    {
        if (keypad == null)
        {
            Debug.LogError("BinaryKeypadInteractable: no BinaryKeypadUI assigned in " + gameObject.name, this);
            return;
        }

        keypad.Open();
    }
}