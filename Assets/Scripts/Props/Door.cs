using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Apertura")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f); // Desplazamiento desde la posición cerrada
    [SerializeField] private float openDuration = 1f; // Segundos que tarda en abrir o cerrar

    [Header("Testeo de estados")]
    [SerializeField] private bool isOpen = false;

    private Vector3 closedPosition;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        closedPosition = transform.position;
    }

    // Lo llama el jugador al interactuar
    public void Toggle()
    {
        isOpen = !isOpen;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToRoutine(isOpen ? closedPosition + openOffset : closedPosition));
    }

    private IEnumerator MoveToRoutine(Vector3 target)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
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
