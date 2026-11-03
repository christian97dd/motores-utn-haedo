using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Lleva un objeto de distraccion (uno solo) y lo tira: con el click apretado se apunta
// y se ve por donde va a ir, al soltarlo sale hacia donde mira la camara y el click derecho cancela
public class ThrowDistraction : MonoBehaviour
{
    // los objetos del nivel lo buscan para guardarse al levantarlos
    public static ThrowDistraction Current { get; private set; }

    public event Action Changed;

    public bool HasDistraction { get; private set; }
    public bool IsAiming { get; private set; }

    [SerializeField] private GameObject distractionPrefab;
    [SerializeField] private Transform throwOrigin; // de donde sale el objeto, si no se usa el pecho del player
    [SerializeField] private float throwSpeed = 11f;
    [SerializeField] private float throwLift = 0.45f; // cuanto sube el tiro por encima de donde mira la camara
    [SerializeField] private float shoulderOffset = 0.45f;

    [Header("Trayectoria")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private Transform impactMarker;
    [SerializeField] private float simulationStep = 0.03f;
    [SerializeField] private float maxFlightTime = 3f;
    [SerializeField] private float projectileRadius = 0.08f;
    [SerializeField] private LayerMask collisionMask = ~0;

    [Header("Camara al apuntar")]
    [SerializeField] private CinemachineRotationComposer aimComposer;
    // el personaje se corre a la izquierda de la pantalla y la camara mira a su derecha, asi no tapa
    // donde cae. Se mueve el encuadre y no la camara, que en los pasillos chocaria con las paredes
    [SerializeField] private float aimScreenX = -0.2f;
    [SerializeField] private float aimBlendSpeed = 1.5f;

    private InputAction throwAction;
    private InputAction cancelAction;
    private Collider playerCollider;
    private Transform cameraTransform;
    private readonly List<Vector3> path = new List<Vector3>();

    private void Awake()
    {
        Current = this;
        throwAction = InputSystem.actions.FindAction("Attack");
        cancelAction = InputSystem.actions.FindAction("CancelThrow");
        playerCollider = GetComponent<Collider>();
        ShowTrajectory(false);
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    private void OnEnable()
    {
        throwAction?.Enable();
    }

    private void OnDisable()
    {
        throwAction?.Disable();
        StopAiming();
    }

    private void Update()
    {
        BlendAimCamera();

        // con la pausa, un puzzle o una ficha abierta el mapa del jugador esta apagado
        if (!HasDistraction || throwAction == null || !throwAction.enabled || PlayerWakeUp.IsPlaying)
        {
            StopAiming();
            return;
        }

        if (throwAction.WasPressedThisFrame()) SetAiming(true);
        if (!IsAiming) return;

        // cancelado, al soltar el click no sale nada
        if (cancelAction != null && cancelAction.WasPressedThisFrame())
        {
            StopAiming();
            return;
        }

        Vector3 origin = GetOrigin();
        Vector3 velocity = GetThrowVelocity();
        DrawTrajectory(origin, velocity);

        if (throwAction.WasReleasedThisFrame())
        {
            Throw(origin, velocity);
            StopAiming();
        }
    }

    public bool TryPickUp()
    {
        if (HasDistraction) return false;

        HasDistraction = true;
        Changed?.Invoke();
        return true;
    }

    private void Throw(Vector3 origin, Vector3 velocity)
    {
        if (distractionPrefab == null) return;

        GameObject thrown = Instantiate(distractionPrefab, origin, Quaternion.LookRotation(velocity));
        IgnorePlayerCollision(thrown);

        // misma velocidad que la simulada, asi cae donde marcaba la linea
        if (thrown.TryGetComponent(out Rigidbody body))
        {
            body.linearVelocity = velocity;
            body.angularVelocity = transform.right * 8f;
        }

        HasDistraction = false;
        Changed?.Invoke();
    }

    private Vector3 GetOrigin()
    {
        if (throwOrigin != null) return throwOrigin.position;

        // desde el hombro derecho y adelante de la capsula: con la camara atras, si sale del centro
        // el arco queda tapado por el propio personaje, y si nace dentro de la capsula se rompe apenas sale
        Vector3 forward = Vector3.ProjectOnPlane(GetAimForward(), Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 chest = transform.position + Vector3.up * 1.45f;
        Vector3 shoulder = chest + right * shoulderOffset + forward * 0.5f;

        // pegado a una pared el hombro queda del otro lado, ahi sale del centro
        if (Physics.Linecast(chest, shoulder, collisionMask, QueryTriggerInteraction.Ignore)) return chest + forward * 0.5f;
        return shoulder;
    }

    private Vector3 GetThrowVelocity()
    {
        Vector3 direction = (GetAimForward() + Vector3.up * throwLift).normalized;
        return direction * throwSpeed;
    }

    private Vector3 GetAimForward()
    {
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        return cameraTransform != null ? cameraTransform.forward : transform.forward;
    }

    // misma cuenta que hace la fisica con el rigidbody sin rozamiento, paso a paso hasta chocar
    private void DrawTrajectory(Vector3 origin, Vector3 velocity)
    {
        path.Clear();
        path.Add(origin);

        Vector3 position = origin;
        bool hitSomething = false;
        RaycastHit hit = default;

        for (float time = 0f; time < maxFlightTime; time += simulationStep)
        {
            Vector3 next = position + velocity * simulationStep;
            velocity += Physics.gravity * simulationStep;

            Vector3 step = next - position;
            if (Physics.SphereCast(position, projectileRadius, step.normalized, out hit, step.magnitude, collisionMask, QueryTriggerInteraction.Ignore)
                && hit.collider != playerCollider)
            {
                path.Add(hit.point);
                hitSomething = true;
                break;
            }

            path.Add(next);
            position = next;
        }

        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++) trajectoryLine.SetPosition(i, path[i]);
        }

        if (impactMarker != null)
        {
            impactMarker.gameObject.SetActive(hitSomething);
            if (hitSomething) impactMarker.SetPositionAndRotation(hit.point + hit.normal * 0.02f, Quaternion.FromToRotation(Vector3.up, hit.normal));
        }
    }

    private void SetAiming(bool aiming)
    {
        if (IsAiming == aiming) return;

        IsAiming = aiming;
        ShowTrajectory(aiming);
        Changed?.Invoke();
    }

    private void StopAiming() => SetAiming(false);

    private void BlendAimCamera()
    {
        if (aimComposer == null) return;

        ScreenComposerSettings composition = aimComposer.Composition;
        float target = IsAiming ? aimScreenX : 0f;
        composition.ScreenPosition.x = Mathf.MoveTowards(composition.ScreenPosition.x, target, aimBlendSpeed * Time.deltaTime);
        aimComposer.Composition = composition;
    }

    private void ShowTrajectory(bool visible)
    {
        if (trajectoryLine != null) trajectoryLine.enabled = visible;
        if (impactMarker != null) impactMarker.gameObject.SetActive(visible);
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
