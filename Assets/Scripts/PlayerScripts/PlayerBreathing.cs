using UnityEngine;

// respiracion del personaje: aparece de a poco mientras va agachado, que es cuando mas
// importa no hacer ruido, y se apaga igual de suave al levantarse
public class PlayerBreathing : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private AudioSource breathing; // loop 2D por el grupo SFX
    [SerializeField] [Range(0f, 1f)] private float crouchVolume = 0.7f;
    [SerializeField] private float fadeSpeed = 1.2f; // cuanto volumen gana o pierde por segundo

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();

        breathing.loop = true;
        breathing.volume = 0f;
        breathing.Play();
    }

    private void Update()
    {
        // con el jugador sin control (intro, susto) no respira en primer plano
        float target = player.enabled && player.IsCrouching ? crouchVolume : 0f;
        breathing.volume = Mathf.MoveTowards(breathing.volume, target, fadeSpeed * Time.deltaTime);
    }
}
