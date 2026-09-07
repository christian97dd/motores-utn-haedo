using UnityEngine;

/// <summary>
/// Representa una tarjeta de acceso recolectable en el mundo. Al interactuar,
/// se registra en el KeycardManager y desaparece de la escena.
/// </summary>
public class KeycardPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string keycardId = "keycard_lab1";
    [SerializeField] private string pickupPrompt = "Mantené E para recoger la tarjeta";

    public string GetInteractionPrompt() => pickupPrompt;

    public bool CanInteract() => true;

    public void Interact()
    {
        KeycardManager.Instance.CollectKeycard(keycardId);

        // A propósito NO llamamos HintTextController acá para mostrar
        // "Tarjeta obtenida": como el objeto se desactiva en la misma
        // llamada, PlayerInteractor detecta el cambio en el frame siguiente
        // y pisa cualquier hint custom con HideHint(). Un mensaje de
        // confirmación más prolijo queda pendiente para cuando se defina
        // el feedback visual/sonoro con el resto del equipo.
        gameObject.SetActive(false);
    }
}