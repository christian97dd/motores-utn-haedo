using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowDistraction : MonoBehaviour
{
    [SerializeField] private GameObject distractionPrefab;
    [SerializeField] private Transform throwOrigin; // de donde sale el objeto, si no se usa el pecho del player
    [SerializeField] private float throwForce = 8f;

    private InputAction throwAction;
    private Collider playerCollider;
    private GameObject activeDistraction;

    private void Awake()
    {
        throwAction = InputSystem.actions.FindAction("Attack");
        playerCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        throwAction?.Enable();
    }

    private void OnDisable()
    {
        throwAction?.Disable();
    }

    private void Update()
    {
        if (throwAction == null || !throwAction.WasPressedThisFrame()) return;

        // uno por vez: la referencia queda en null sola cuando el objeto se destruye
        if (activeDistraction != null) return;

        Throw();
    }

    private void Throw()
    {
        if (distractionPrefab == null) return;

        // un metro adelante, si nace dentro de la capsula se rompe apenas sale
        Vector3 origin = throwOrigin != null ? throwOrigin.position : transform.position + Vector3.up + transform.forward;

        GameObject thrown = Instantiate(distractionPrefab, origin, transform.rotation);
        activeDistraction = thrown;

        IgnorePlayerCollision(thrown);

        // el impulso lo maneja la fisica del objeto, no el script del player
        if (thrown.TryGetComponent(out Rigidbody body))
        {
            body.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }
    }

    // sin esto el objeto choca contra el propio personaje y se rompe en el acto
    private void IgnorePlayerCollision(GameObject thrown)
    {
        if (playerCollider == null) return;

        foreach (Collider thrownCollider in thrown.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(thrownCollider, playerCollider);
        }
    }
}
