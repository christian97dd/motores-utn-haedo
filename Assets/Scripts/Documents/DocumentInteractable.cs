using UnityEngine;

// va en lo que se lee: compu, papel, pantalla (necesita collider)
public class DocumentInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DocumentData document;

    [Header("Leyenda")]
    [SerializeField] private string displayName = "TERMINAL";
    [SerializeField] private string actionText = "Leer ficha";

    [Header("Voz")]
    [SerializeField] private GameObject objectToActivate;

    public InteractionPrompt Prompt => new InteractionPrompt(displayName, actionText);

    public void Interact()
    {
        if (document == null || DocumentWindow.Current == null)
        {
            Debug.LogError("DocumentInteractable: falta el documento o no hay DocumentWindow en la escena", this);
            return;
        }

        DocumentWindow.Current.Open(document);

        if (objectToActivate != null && DocumentWindow.IsOpen)
        {
            objectToActivate.SetActive(true);
            DocumentWindow.Current.Closed += StopVoice;
        }
    }

    // la voz dura lo que la ficha abierta, asi al volver a abrirla arranca de nuevo
    private void StopVoice()
    {
        DocumentWindow.Current.Closed -= StopVoice;
        objectToActivate.SetActive(false);
    }
}
