using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float hearingMultiplier = 1f;

    [Header("References")]
    [SerializeField] private Transform patrolRoute; // cada hijo es un punto de patrulla
    private Transform[] _patrolPoints;

    [Header("Vision")]
    [SerializeField] private EnemyFOV fov;
    [SerializeField] private float chaseUpdateInterval = 0.2f;
    private float _nextChaseTime;

    [Header("Settings")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;

    [Header("Catch")]
    [SerializeField] private float catchDistance = 1.2f;
    private bool _caughtPlayer;

    [Header("SafeZoneCheck")]
    [SerializeField] private Collider[] safeZones;
    [SerializeField] private LayerMask safeZoneLayer;
    [SerializeField] private float safeZoneRadius = 0.3f;
    [SerializeField] private Transform safeZoneCheckPivot;

    private static readonly int WalkHash = Animator.StringToHash("walk");
    private static readonly int RunHash = Animator.StringToHash("run");

    private NavMeshAgent _agent;
    private Animator _animator;
    private Coroutine _waitRoutine;
    private int _currentPatrolIndex = -1;
    private bool _isWaiting;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        if (fov == null) fov = GetComponent<EnemyFOV>();

        _patrolPoints = new Transform[patrolRoute != null ? patrolRoute.childCount : 0];
        for (int i = 0; i < _patrolPoints.Length; i++) _patrolPoints[i] = patrolRoute.GetChild(i);
    }

    private void Start()
    {
        GoToNextPatrolPoint();
    }

    private void Update()
    {
        CheckCatch();
        CheckVision();
        Patrol();
        UpdateAnimations();

        if (IsInSafeZone())
        {
            _agent.isStopped = true;
            _animator.SetBool(RunHash, false);
            if (_waitRoutine == null)
            {
                _waitRoutine = StartCoroutine(WaitAtPatrolPoint());
            }
        }
    }

    private void Patrol()
    {
        if (_isWaiting) return;
        if (fov != null && fov.canSeePlayer) return;
        if (_patrolPoints.Length == 0) return;

        if (!_agent.pathPending && _agent.hasPath &&
            _agent.remainingDistance <= _agent.stoppingDistance + stopAtDistance)
        {
            _waitRoutine = StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        _isWaiting = true;
        _agent.isStopped = true;
        _animator.SetBool(RunHash, false);

        yield return new WaitForSeconds(patrolWaitTime);

        _agent.isStopped = false;
        _isWaiting = false;
        _waitRoutine = null;
        GoToNextPatrolPoint();
    }

    private void GoToNextPatrolPoint()
    {
        if (_patrolPoints.Length == 0) return;

        // al azar para que no se aprenda el recorrido, pero sin repetir el punto donde ya esta
        int next = Random.Range(0, _patrolPoints.Length);
        if (_patrolPoints.Length > 1 && next == _currentPatrolIndex) next = (next + 1) % _patrolPoints.Length;

        _currentPatrolIndex = next;
        _agent.SetDestination(_patrolPoints[_currentPatrolIndex].position);
    }

    private void UpdateAnimations()
    {
        bool isMoving = _agent.velocity.sqrMagnitude > 0.0001f;
        _animator.SetBool(WalkHash, isMoving);
    }

    private void OnEnable() => PlayerFootstepSound.OnNoiseEmitted += HearNoise;

    private void OnDisable()
    {
        PlayerFootstepSound.OnNoiseEmitted -= HearNoise;
        _isWaiting = false;
        _waitRoutine = null;
    }

    private void HearNoise(Vector3 noisePosition, float noiseRadius)
    {
        float distance = Vector3.Distance(transform.position, noisePosition);

        if (distance > noiseRadius * hearingMultiplier) return;

        Chase(noisePosition);
    }

    public void Chase(Vector3 noisePosition)
    {
        if (NavMesh.SamplePosition(noisePosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            if (_waitRoutine != null)
            {
                StopCoroutine(_waitRoutine);
                _waitRoutine = null;
                _isWaiting = false;
            }

            _agent.isStopped = false;
            _agent.SetDestination(hit.position);
            _animator.SetBool(RunHash, true);
        }
    }

    private bool IsInSafeZone()
    {
        return Physics.CheckSphere(GetSafeZoneCheckOrigin(), safeZoneRadius, safeZoneLayer, QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetSafeZoneCheckOrigin()
    {
        return safeZoneCheckPivot != null ? safeZoneCheckPivot.position : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsInSafeZone() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(GetSafeZoneCheckOrigin(), safeZoneRadius);
    }
    private void CheckVision()
    {
        if (fov == null || !fov.canSeePlayer || fov.playerRef == null) return;
        if (IsInSafeZone()) return;

        if (Time.time < _nextChaseTime) return;
        _nextChaseTime = Time.time + chaseUpdateInterval;

        Chase(fov.playerRef.transform.position);
    }

    private void CheckCatch()
    {
        if (_caughtPlayer || fov == null || fov.playerRef == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, fov.playerRef.transform.position);
        if (distanceToPlayer > catchDistance) return;

        // la escena de derrota se carga recien al final del frame, sin el flag se llamaria varias veces
        _caughtPlayer = true;
        GameFlow.Defeat(DefeatCause.Android);
    }
}