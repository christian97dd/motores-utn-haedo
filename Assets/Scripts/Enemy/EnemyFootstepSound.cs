using UnityEngine;
using UnityEngine.AI;

// pisadas del androide: suenan desde el enemigo (3D) y se apuran solas cuando persigue,
// porque cada paso sale de la distancia recorrida y no de un intervalo fijo
public class EnemyFootstepSound : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SoundVariation steps;

    [Header("Ritmo")]
    [SerializeField] private float strideLength = 1.2f; // metros entre un paso y el siguiente
    [SerializeField] private float minSpeed = 0.2f;     // por debajo de esto esta quieto

    private float distanceSinceStep;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        float speed = agent.velocity.magnitude;

        // quieto: el primer paso al arrancar llega a mitad de zancada, no en el acto
        if (speed < minSpeed)
        {
            distanceSinceStep = strideLength * 0.5f;
            return;
        }

        distanceSinceStep += speed * Time.deltaTime;
        if (distanceSinceStep < strideLength) return;

        distanceSinceStep -= strideLength;
        steps.Play(audioSource);
    }
}
