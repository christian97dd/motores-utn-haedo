using UnityEngine;
using UnityEngine.AI;

// pisadas del androide: suenan desde el enemigo (3D) en el momento en que apoya cada pie en la animacion,
// asi caminando y corriendo el ritmo coincide con lo que se ve aunque el agente vaya siempre a la misma velocidad.
// Corriendo ademas cambia el tratamiento del sonido
public class EnemyFootstepSound : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    // misma grabacion con distinto tratamiento: corriendo pisa mas fuerte y mas agudo, asi se oye la persecucion
    [SerializeField] private SoundVariation walkSteps;
    [SerializeField] private SoundVariation runSteps;

    [Header("Apoyo de los pies (fraccion del ciclo de la animacion)")]
    [SerializeField] private float[] walkContacts = { 0.38f, 0.9f };
    [SerializeField] private float[] runContacts = { 0.33f, 0.9f };
    [SerializeField] private string runParameter = "run"; // el mismo bool que usa Enemy para correr

    [SerializeField] private float minSpeed = 0.2f; // por debajo de esto esta quieto

    private int runHash;
    private float lastPhase = -1f;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        runHash = Animator.StringToHash(runParameter);
    }

    private void Update()
    {
        if (agent.velocity.magnitude < minSpeed)
        {
            lastPhase = -1f;
            return;
        }

        // durante el cruce entre caminar y correr manda la animacion que esta entrando
        AnimatorStateInfo state = animator.IsInTransition(0)
            ? animator.GetNextAnimatorStateInfo(0)
            : animator.GetCurrentAnimatorStateInfo(0);
        float phase = Mathf.Repeat(state.normalizedTime, 1f);

        bool running = animator.GetBool(runHash);
        float[] contacts = running ? runContacts : walkContacts;
        if (lastPhase >= 0f && CrossedContact(lastPhase, phase, contacts))
        {
            (running ? runSteps : walkSteps).Play(audioSource);
        }

        lastPhase = phase;
    }

    // el ciclo da la vuelta de 1 a 0: un apoyo cuenta si quedo entre la fase anterior y la actual
    private static bool CrossedContact(float from, float to, float[] contacts)
    {
        foreach (float contact in contacts)
        {
            bool crossed = from <= to
                ? from < contact && contact <= to
                : from < contact || contact <= to;

            if (crossed) return true;
        }

        return false;
    }
}
