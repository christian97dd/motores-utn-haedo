using UnityEngine;

/// <summary>
/// Puerta interactuable con estado locked/unlocked. Si tiene un
/// requiredKeycardId asignado, se desbloquea sola al interactuar si el
/// jugador ya recolectó esa tarjeta (ver KeycardManager). También se puede
/// desbloquear manualmente por código llamando a Unlock() — por ejemplo,
/// desde un interruptor o panel de código en vez de una tarjeta.
/// </summary>

public class InteractableDoor : MonoBehaviour, IInteractable
{
    [Header("Door State")]
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredKeycardId = "keycard_lab1";

    [Header("Feedback")]
    [SerializeField] private Light statusLight;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    [Header("Prompts")]
    [SerializeField] private string lockedPrompt = "Puerta bloqueada - necesitás la tarjeta";
    [SerializeField] private string unlockedPrompt = "Mantené E para abrir";

    private void Start()
    {
        UpdateStatusLight();
    }

    // OnValidate lo llama Unity automáticamente cada vez que se cambia un
    // campo desde el Inspector (incluso en Play Mode). Sin esto, destildar
    // "Is Locked" a mano para testear no actualiza la luz, porque
    // UpdateStatusLight() normalmente solo se llama desde Start() y Unlock().

    private void OnValidate()
    {
        UpdateStatusLight();
    }

    public string GetInteractionPrompt()
    {
        return isLocked ? lockedPrompt : unlockedPrompt;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (isLocked)
        {
            bool hasRequiredKeycard = !string.IsNullOrEmpty(requiredKeycardId)
                && KeycardManager.Instance.HasKeycard(requiredKeycardId);

            if (hasRequiredKeycard)
            {
                Unlock();
                Debug.Log("Acceso concedido con tarjeta");
            }
            else
            {
                Debug.Log("Acceso denegado");
            }
            return;
        }

        Debug.Log("Acceso concedido");
        // TODO: acá después va lo que pase al abrirse de verdad — animación, cambio de escena, lo que definan
    }

    // Público a propósito: cualquier otro sistema (tarjeta, interruptor,
    // panel de código) puede llamar door.Unlock() sin necesitar saber nada
    // más de cómo funciona la puerta por dentro.
    public void Unlock()
    {
        isLocked = false;
        UpdateStatusLight();
    }

    private void UpdateStatusLight()
    {
        if (statusLight != null)
        {
            statusLight.color = isLocked ? lockedColor : unlockedColor;
        }
    }
}