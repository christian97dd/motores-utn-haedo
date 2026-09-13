using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Apertura")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f); // Desplazamiento desde la posición cerrada
    [SerializeField] private float openSpeed = 3f; // Unidades por segundo

    [Header("Testeo de estados")]
    [SerializeField] private bool isOpen = false;

    private Vector3 closedPosition;

    private void Awake()
    {
        closedPosition = transform.position;
    }

    // Lo llama el jugador al interactuar
    public void Toggle()
    {
        isOpen = !isOpen;
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
