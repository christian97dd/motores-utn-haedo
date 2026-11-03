using UnityEngine;
using UnityEngine.AI;
 
/// <summary>
/// Cámara de seguridad con detección por Raycast (no atraviesa paredes).
/// Estados: Patrolling (barrido) -> Tracking (sigue al player) -> Returning (vuelve al barrido).
/// Si ve al player más de "timeToAlarm" segundos: la alarma suena en loop, se activa el collider
/// de ruido y se llama a los enemigos hacia la cámara. Todo se corta cuando deja de ver al player.
/// </summary>
public class SecurityCameraDetection : MonoBehaviour
{
    private enum State { Patrolling, Tracking, Returning }
 
    // Las lee AudioStateController
    public bool isPlayerInZone => state == State.Tracking;
    public bool isAlarmActive => alarmActive;
 
    [Header("Referencias")]
    [Tooltip("Objeto que gira. Rota alrededor de su posición, así que ponele el que tenga el eje de giro (ej: Base).")]
    [SerializeField] private Transform pivot;
    [Tooltip("De dónde sale el raycast (la lente). Si está vacío usa el pivot.")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private Transform player;
 
    [Header("Dirección de la vista")]
    [Tooltip("Rotación (en grados) que se suma a la dirección del Ray Origin para saber hacia dónde mira.")]
    [SerializeField] private Vector3 viewRotationOffset = Vector3.zero;
 
    [Header("Rango de detección")]
    [SerializeField] private float detectionRange = 12f;
    [Tooltip("Ángulo total del cono de visión (grados).")]
    [Range(5f, 180f)] [SerializeField] private float viewAngle = 60f;
    [Tooltip("Altura sobre el pivot del player a la que apunta el raycast (pecho/cabeza).")]
    [SerializeField] private float playerAimHeight = 1f;
    [Tooltip("Capas que bloquean o detectan: paredes Y la capa del player.")]
    [SerializeField] private LayerMask detectionMask = ~0;
    [Tooltip("Tiempo que tolera perder al player antes de volver al barrido.")]
    [SerializeField] private float lostGraceTime = 0.5f;
 
    [Header("Debug")]
    [Tooltip("Dibuja el raycast real en la Scene (verde = ve al player, rojo = bloqueado) y avisa en la Console qué lo bloquea o si la lente está dentro de un collider.")]
    [SerializeField] private bool debugRay = false;
 
    [Header("Movimiento")]
    [Tooltip("Ángulo total del barrido izquierda-derecha (grados).")]
    [SerializeField] private float sweepAngle = 90f;
    [SerializeField] private float sweepSpeed = 0.8f;
    [Tooltip("Velocidad de giro (grados/seg) al seguir al player.")]
    [SerializeField] private float trackSpeed = 120f;
    [Tooltip("Velocidad de giro (grados/seg) al volver al barrido.")]
    [SerializeField] private float returnSpeed = 60f;
 
    [Header("Audio")]
    [Tooltip("Sonido de detección (loop mientras ve al player, antes de la alarma).")]
    [SerializeField] private AudioSource detectionSource;
    [Tooltip("Sonido de alarma (loop mientras la alarma está activa).")]
    [SerializeField] private AudioSource alarmSource;
 
    [Header("Alarma")]
    [Tooltip("Segundos de detección continua antes de que suene la alarma.")]
    [SerializeField] private float timeToAlarm = 3f;
 
    [Header("Ruido (collider opcional)")]
    [Tooltip("Hijo con SphereCollider (Is Trigger) y Rigidbody kinematic. Queda activo mientras dure la alarma. Opcional.")]
    [SerializeField] private SphereCollider noiseCollider;
 
    [Header("Llamar al enemigo")]
    [Tooltip("Si está activo, la cámara llama directamente a los Enemy cercanos (Enemy.Chase) hacia su zona.")]
    [SerializeField] private bool callEnemies = true;
    [Tooltip("Radio en el que los enemigos 'escuchan' la alarma (también el radio del collider de ruido).")]
    [SerializeField] private float noiseRadius = 30f;
    [Tooltip("Punto al que va el enemigo. Ponelo en el piso debajo de la cámara (sobre el NavMesh). Si está vacío usa la posición de la cámara.")]
    [SerializeField] private Transform enemyDestination;
    [Tooltip("Cada cuántos segundos se vuelve a llamar al enemigo mientras suena la alarma.")]
    [SerializeField] private float callInterval = 1f;
    [Tooltip("Distancia máxima para buscar un punto del NavMesh cerca del destino.")]
    [SerializeField] private float navSampleDistance = 8f;
 
    private State state = State.Patrolling;
    private Quaternion baseRotation;
    private float currentYaw;
    private float sweepTime;
    private float detectionTimer;
    private float lostTimer;
    private float callTimer;
    private bool alarmActive;
    private Enemy[] enemies;
 
    private Vector3 ViewForward =>
        rayOrigin.rotation * Quaternion.Euler(viewRotationOffset) * Vector3.forward;
 
    private void Awake()
    {
        if (pivot == null) pivot = transform;
        if (rayOrigin == null) rayOrigin = pivot;
 
        baseRotation = pivot.rotation;
 
        if (detectionSource != null) { detectionSource.loop = true; detectionSource.playOnAwake = false; }
        if (alarmSource != null) { alarmSource.loop = true; alarmSource.playOnAwake = false; }
        if (noiseCollider != null) { noiseCollider.isTrigger = true; noiseCollider.gameObject.SetActive(false); }
    }
 
    private void Start()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
    }
 
    private void Update()
    {
        if (player == null) return;
 
        bool seen = CanSeePlayer();
 
        if (seen)
        {
            lostTimer = 0f;
            if (state != State.Tracking) StartDetection();
        }
        else if (state == State.Tracking)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= lostGraceTime) StopDetection();
        }
 
        switch (state)
        {
            case State.Patrolling:
                sweepTime += Time.deltaTime * sweepSpeed;
                currentYaw = GetPatrolYaw();
                break;
 
            case State.Tracking:
                if (seen)
                {
                    TrackPlayer();
                    detectionTimer += Time.deltaTime;
 
                    if (!alarmActive && detectionTimer >= timeToAlarm)
                        StartAlarm();
                }
                break;
 
            case State.Returning:
                ReturnToPatrol();
                break;
        }
 
        if (alarmActive && callEnemies)
        {
            callTimer -= Time.deltaTime;
            if (callTimer <= 0f)
            {
                CallEnemies();
                callTimer = callInterval;
            }
        }
 
        // El giro se aplica sobre la rotación original: el modelo nunca se "da vuelta"
        pivot.rotation = Quaternion.AngleAxis(currentYaw, Vector3.up) * baseRotation;
    }
 
    // ---------------------------------------------------------------- Detección
 
    private bool CanSeePlayer()
    {
        Vector3 origin = rayOrigin.position;
        Vector3 target = player.position + Vector3.up * playerAimHeight;
        Vector3 toTarget = target - origin;
        float distance = toTarget.magnitude;
 
        if (distance > detectionRange || distance < 0.001f) return false;
 
        Vector3 dir = toTarget / distance;
        if (Vector3.Angle(ViewForward, dir) > viewAngle * 0.5f) return false;
 
        bool hitSomething = Physics.Raycast(origin, dir, out RaycastHit hit, detectionRange, detectionMask, QueryTriggerInteraction.Ignore);
        bool visible = hitSomething && (hit.transform == player || hit.transform.IsChildOf(player));
 
        if (debugRay) DebugRay(origin, dir, hitSomething, hit, visible);
 
        return visible;
    }
 
    private Collider lastBlocker;
    private Collider lastInside;
 
    private void DebugRay(Vector3 origin, Vector3 dir, bool hitSomething, RaycastHit hit, bool visible)
    {
        Vector3 end = hitSomething ? hit.point : origin + dir * detectionRange;
        Debug.DrawLine(origin, end, visible ? Color.green : Color.red);
 
        // qué collider está tapando la vista
        if (hitSomething && !visible && hit.collider != lastBlocker)
        {
            lastBlocker = hit.collider;
            Debug.Log($"[{name}] Raycast bloqueado por '{hit.collider.name}' (capa {LayerMask.LayerToName(hit.collider.gameObject.layer)})", hit.collider);
        }
 
        // la lente dentro de un collider ajeno: el raycast no detecta ese collider
        foreach (Collider col in Physics.OverlapSphere(origin, 0.05f, detectionMask, QueryTriggerInteraction.Ignore))
        {
            if (col.transform.IsChildOf(transform) || col == lastInside) continue;
            lastInside = col;
            Debug.LogWarning($"[{name}] El origen del raycast está DENTRO del collider '{col.name}'. Ese collider no va a bloquear la vista. Alejá el Ray Origin de la pared.", col);
        }
    }
 
    private void StartDetection()
    {
        state = State.Tracking;
        detectionTimer = 0f;
 
        if (detectionSource != null && !detectionSource.isPlaying) detectionSource.Play();
    }
 
    private void StopDetection()
    {
        state = State.Returning;
        detectionTimer = 0f;
 
        if (detectionSource != null) detectionSource.Stop();
        StopAlarm();
    }
 
    // ---------------------------------------------------------------- Movimiento
 
    private float GetPatrolYaw() => Mathf.Sin(sweepTime) * sweepAngle * 0.5f;
 
    private void TrackPlayer()
    {
        Vector3 toPlayer = (player.position + Vector3.up * playerAimHeight) - rayOrigin.position;
        Vector3 flatTarget = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
        Vector3 flatView = Vector3.ProjectOnPlane(ViewForward, Vector3.up);
        if (flatTarget.sqrMagnitude < 0.0001f || flatView.sqrMagnitude < 0.0001f) return;
 
        float delta = Vector3.SignedAngle(flatView, flatTarget, Vector3.up);
        float maxStep = trackSpeed * Time.deltaTime;
        currentYaw += Mathf.Clamp(delta, -maxStep, maxStep);
    }
 
    private void ReturnToPatrol()
    {
        float target = GetPatrolYaw();
        float delta = Mathf.DeltaAngle(currentYaw, target);
        float maxStep = returnSpeed * Time.deltaTime;
        currentYaw += Mathf.Clamp(delta, -maxStep, maxStep);
 
        if (Mathf.Abs(Mathf.DeltaAngle(currentYaw, target)) < 1f)
            state = State.Patrolling;
    }
 
    // ---------------------------------------------------------------- Alarma
 
    private void StartAlarm()
    {
        alarmActive = true;
 
        if (detectionSource != null) detectionSource.Stop();
        if (alarmSource != null && alarmSource.clip != null && !alarmSource.isPlaying) alarmSource.Play();
 
        if (noiseCollider != null)
        {
            noiseCollider.radius = noiseRadius;
            noiseCollider.gameObject.SetActive(true);
        }
 
        callTimer = 0f; // llama al enemigo ya mismo en el próximo chequeo
    }
 
    private void StopAlarm()
    {
        if (!alarmActive) return;
        alarmActive = false;
 
        if (alarmSource != null) alarmSource.Stop();
        if (noiseCollider != null) noiseCollider.gameObject.SetActive(false);
    }
 
    private void CallEnemies()
    {
        if (enemies == null) return;
 
        Vector3 dest = enemyDestination != null ? enemyDestination.position : transform.position;
        if (!NavMesh.SamplePosition(dest, out NavMeshHit navHit, navSampleDistance, NavMesh.AllAreas))
            return; // no hay NavMesh cerca del destino
 
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.isActiveAndEnabled) continue;
            if (Vector3.Distance(enemy.transform.position, transform.position) > noiseRadius) continue;
 
            enemy.Chase(navHit.position);
        }
    }
 
    // ---------------------------------------------------------------- Gizmos
 
    private void OnDrawGizmosSelected()
    {
        Transform o = rayOrigin != null ? rayOrigin : (pivot != null ? pivot : transform);
        Vector3 pos = o.position;
        Vector3 fwd = o.rotation * Quaternion.Euler(viewRotationOffset) * Vector3.forward;
 
        float half = viewAngle * 0.5f;
        Vector3 left = Quaternion.AngleAxis(-half, Vector3.up) * fwd;
        Vector3 right = Quaternion.AngleAxis(half, Vector3.up) * fwd;
 
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(pos, left * detectionRange);
        Gizmos.DrawRay(pos, right * detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(pos, fwd * detectionRange);
 
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
 
        if (enemyDestination != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(enemyDestination.position, 0.5f);
        }
    }
}
 