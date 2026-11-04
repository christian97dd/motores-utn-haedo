using UnityEngine;

// botella tirada en el nivel: con E se guarda, siempre que no se lleve otra (necesita collider)
public class DistractionPickup : MonoBehaviour, IInteractable
{
    [Header("Leyenda")]
    [SerializeField] private string displayName = "BOTELLA";
    [SerializeField] private string actionText = "Levantar";
    [SerializeField] private string fullText = "Ya llevás una botella";

    public InteractionPrompt Prompt => IsFull
        ? new InteractionPrompt(displayName, fullText, InteractionPromptStyle.Locked)
        : new InteractionPrompt(displayName, actionText);

    private static bool IsFull => ThrowDistraction.Current != null && ThrowDistraction.Current.HasDistraction;

    public void Interact()
    {
        if (ThrowDistraction.Current == null || !ThrowDistraction.Current.TryPickUp()) return;

        Destroy(gameObject);
    }
}
