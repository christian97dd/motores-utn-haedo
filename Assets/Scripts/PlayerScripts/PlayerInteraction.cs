using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Alcance")]
    [SerializeField] private float interactionRange = 3f; // Distancia máxima para alcanzar una puerta
    [SerializeField] private float eyeHeight = 1f; // Altura desde la que sale el rayo, el pivote está en los pies

    // avisa a la leyenda que objeto se esta mirando (null si ninguno)
    public static event Action<IInteractable> TargetChanged;

    public IInteractable CurrentTarget { get; private set; }

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
        SetTarget(null);
    }

    private void Update()
    {
        if (interactAction == null) return;

        // con una ficha, puzzle o pausa abierta el mapa esta apagado
        SetTarget(interactAction.enabled ? FindTarget() : null);

        if (CurrentTarget != null && interactAction.WasPressedThisFrame())
        {
            CurrentTarget.Interact();
        }
    }

    private IInteractable FindTarget()
    {
        // Se ignoran los triggers para que el área de la cámara de seguridad no tape a la puerta
        if (Physics.Raycast(GetRayOrigin(), transform.forward, out RaycastHit hit, interactionRange, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.TryGetComponent(out IInteractable interactable))
        {
            return interactable;
        }

        return null;
    }

    private void SetTarget(IInteractable target)
    {
        if (target == CurrentTarget) return;

        CurrentTarget = target;
        TargetChanged?.Invoke(target);
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
