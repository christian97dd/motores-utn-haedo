using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Se coloca en el GameObject del jugador. Detecta el IInteractable más
/// cercano dentro de un rango, actualiza el hint text en pantalla, y
/// ejecuta Interact() cuando el jugador aprieta la tecla asignada.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform detectionOrigin;

    private InputAction interactAction;

    private IInteractable currentInteractable;

    // Buffer reusado en vez de crear un array nuevo cada frame:
    // OverlapSphereNonAlloc no genera basura para el Garbage Collector,
    // a diferencia de Physics.OverlapSphere normal.
    private readonly Collider[] detectionBuffer = new Collider[10];

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
        DetectInteractable();

        if (currentInteractable != null && interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (currentInteractable.CanInteract())
            {
                currentInteractable.Interact();

                // Importante: Interact() puede cambiar el estado interno del
                // objeto (ej: una puerta que se desbloquea). Sin este refresh,
                // el prompt en pantalla se queda con el texto viejo hasta que
                // el jugador se aleja y vuelve a acercarse, porque
                // DetectInteractable() de abajo solo actualiza el hint cuando
                // el objeto detectado CAMBIA, no cuando cambia su estado interno.

                if (currentInteractable != null)
                {
                    ShowHint(currentInteractable.GetInteractionPrompt());
                }
            }
        }
    }

    private void DetectInteractable()
    {
        Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
        int hitCount = Physics.OverlapSphereNonAlloc(origin, interactionRange, detectionBuffer, interactableLayer);

        // Si hay varios interactuables en rango, nos quedamos con el más
        // cercano al jugador, para no mostrar el prompt de un objeto lejano
        // mientras hay otro más pegado.

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            IInteractable interactable = detectionBuffer[i].GetComponent<IInteractable>();
            if (interactable == null)
                continue;

            float distance = Vector3.Distance(origin, detectionBuffer[i].transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        // Solo tocamos el hint cuando el objeto detectado cambia, para no
        // llamar ShowHint en cada frame sin necesidad.

        if (closest != currentInteractable)
        {
            currentInteractable = closest;

            if (currentInteractable != null)
                ShowHint(currentInteractable.GetInteractionPrompt());
            else
                HideHint();
        }
    }

    // El hint es opcional: si la escena todavía no tiene el Canvas armado,
    // la interacción sigue funcionando en vez de tirar NullReference
    private void ShowHint(string message)
    {
        if (HintTextController.Instance != null)
            HintTextController.Instance.ShowHint(message);
    }

    private void HideHint()
    {
        if (HintTextController.Instance != null)
            HintTextController.Instance.HideHint();
    }

    // Dibuja el rango de detección como una esfera amarilla en la Scene view
    // cuando el objeto está seleccionado — sirve para ajustar interactionRange a ojo.
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, interactionRange);
    }
}