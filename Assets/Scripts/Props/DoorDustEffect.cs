using UnityEngine;

/// <summary>
/// Reproduce polvo mientras la puerta se abre.
/// Se pone en el mismo GameObject que tiene el script Door.
/// Los sistemas de partículas van APARTE (sueltos en la escena, no como hijos de la puerta),
/// porque si fueran hijos se moverían junto con ella.
/// </summary>
public class DoorDustEffect : MonoBehaviour
{
    [Header("Efectos")]
    [Tooltip("Por ejemplo: polvo en el piso y polvo que cae desde arriba")]
    [SerializeField] private ParticleSystem[] dustEffects;

    [Header("Opciones")]
    [Tooltip("Si es true, también sale polvo cuando la puerta se cierra")]
    [SerializeField] private bool alsoOnClose = false;

    [Tooltip("Cuánto tiene que moverse la puerta por frame para considerarla en movimiento")]
    [SerializeField] private float movementThreshold = 0.0005f;

    private Vector3 closedPosition;
    private float lastDistance;
    private bool isPlaying;

    private void Awake()
    {
        if (dustEffects == null || dustEffects.Length == 0)
        {
            Debug.LogError("DoorDustEffect: no ParticleSystem assigned in " + gameObject.name, this);
        }

        closedPosition = transform.position;
        lastDistance = 0f;
    }

    private void Update()
    {
        if (dustEffects == null || dustEffects.Length == 0) return;

        // Distancia a la posición cerrada: si crece, la puerta se abre; si baja, se cierra
        float distance = Vector3.Distance(transform.position, closedPosition);
        float delta = distance - lastDistance;
        lastDistance = distance;

        bool isOpening = delta > movementThreshold;
        bool isClosing = delta < -movementThreshold;
        bool shouldPlay = isOpening || (alsoOnClose && isClosing);

        if (shouldPlay && !isPlaying)
        {
            SetPlaying(true);
        }
        else if (!shouldPlay && isPlaying)
        {
            SetPlaying(false);
        }
    }

    private void SetPlaying(bool play)
    {
        isPlaying = play;

        foreach (ParticleSystem dust in dustEffects)
        {
            if (dust == null) continue;

            if (play)
            {
                dust.Play();
            }
            else
            {
                // Deja de emitir, pero las partículas que ya existen se desvanecen solas
                dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}