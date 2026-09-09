using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Alcance")]
    [SerializeField] private float interactionRange = 3f; // Distancia máxima para alcanzar una puerta
    [SerializeField] private float eyeHeight = 1f; // Altura desde la que sale el rayo, el pivote está en los pies

    private InputAction interactAction;

    private void Awake()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void OnEnable()
    {
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        interactAction?.Disable();
    }

    private void Update()
    {
        if (interactAction == null || !interactAction.WasPressedThisFrame()) return;

        // Se ignoran los triggers para que el área de la cámara de seguridad no tape a la puerta
        if (Physics.Raycast(GetRayOrigin(), transform.forward, out RaycastHit hit, interactionRange, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(out Door door))
            {
                door.Toggle();
            }
        }
    }

    private Vector3 GetRayOrigin()
    {
        return transform.position + Vector3.up * eyeHeight;
    }

    // Muestra el alcance de la interacción en la Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(GetRayOrigin(), GetRayOrigin() + transform.forward * interactionRange);
    }
}
