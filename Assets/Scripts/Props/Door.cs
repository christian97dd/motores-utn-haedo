using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Leyenda")]
    [SerializeField] private string displayName = "PUERTA"; // nombre en la leyenda

    [Header("Apertura")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f); // Desplazamiento desde la posición cerrada
    [SerializeField] private float openSpeed = 3f; // Unidades por segundo

    [Header("Bloqueo")]
    [SerializeField] private bool isLocked = false; // Si está bloqueada, Toggle() no hace nada hasta que un puzzle la desbloquee

    [Header("Testeo de estados")]
    [SerializeField] private bool isOpen = false;

    private Vector3 closedPosition;

    private void Awake()
    {
        closedPosition = transform.position;
    }

    public InteractionPrompt Prompt => isLocked
        ? new InteractionPrompt(displayName, "Bloqueada", InteractionPromptStyle.Locked)
        : new InteractionPrompt(displayName, isOpen ? "Cerrar" : "Abrir");

    // Lo llama el jugador al interactuar
    public void Interact()
    {
        Toggle();
    }

    public void Toggle()
    {
        if (isLocked) return; // TODO: feedback (sonido / texto de pista)

        isOpen = !isOpen;
    }

    // Lo llama el puzzle al resolverse
    public void Unlock()
    {
        isLocked = false;
    }

    // Abre la puerta sin alternar (no la cierra si ya estaba abierta)
    public void Open()
    {
        isOpen = true;
    }

    private void Update()
    {
        Vector3 target = isOpen ? closedPosition + openOffset : closedPosition;

        if (transform.position == target) return;

        transform.position = Vector3.MoveTowards(transform.position, target, openSpeed * Time.deltaTime);
    }

    // Marca en la escena dónde va a quedar la puerta abierta
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? closedPosition : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + openOffset);
        Gizmos.DrawWireCube(origin + openOffset, transform.lossyScale);
    }
}