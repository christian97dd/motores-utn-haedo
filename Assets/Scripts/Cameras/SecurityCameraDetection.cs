using System.Collections;
using UnityEngine;

public class SecurityCameraDetection : MonoBehaviour
{
    [Header("Tiempos")]
    public float timeToTriggerAlarm = 3f;      // Segundos en colision del Player para activar la alarma
    public float alarmDuration = 5f;           // Duración de la alarma activada
    public float fadeDuration = 0.5f;          // Tiempo en segundos del Fade Out de audio

    [Header("Trigger detección")]
    public GameObject detectionTriggerZone;    // GameObject del Trigger de la cámara que detecta al Player

    [Min(0f)]
    [SerializeField] private float detectionRadius = 5f;   // Radio del área de detección, en unidades de mundo

    [Header("Audiosource de estados de alarma")]
    public AudioSource detectionAudioSource;   // Sonido mientras detecta al jugador 

    [Header("trigger de Sonido")]
    public GameObject monsterNoiseArea;        // GameObject del area de ruido

    [Header("Testeo de estados")]
    public bool isPlayerInZone = false;
    public float detectionTimer = 0f;
    public bool isAlarmActive = false;

    private Coroutine alarmCoroutine;
    private Coroutine fadeAudioCoroutine;
    private float originalDetectionVolume = 1f;
    private CapsuleCollider detectionCollider;

    private void Awake()
    {
        if (detectionAudioSource != null)
        {
            originalDetectionVolume = detectionAudioSource.volume;
        }

        ApplyDetectionRadius();
    }

    // Se llama al cambiar un valor en el Inspector, sin necesidad de entrar en Play
    private void OnValidate()
    {
        ApplyDetectionRadius();
    }

    private void ApplyDetectionRadius()
    {
        if (detectionCollider == null)
        {
            detectionCollider = GetComponent<CapsuleCollider>();
        }

        if (detectionCollider == null) return;

        // El collider hereda la escala de los padres, así que el radio se divide
        // por esa escala para que el valor del Inspector quede en unidades de mundo
        float escalaHeredada = transform.lossyScale.x;

        if (escalaHeredada <= 0f) return;

        detectionCollider.radius = detectionRadius / escalaHeredada;
    }

    private void Update()
    {
        if (isAlarmActive) return;

        if (isPlayerInZone)
        {
            detectionTimer += Time.deltaTime;

            // Iniciar audio de detección si no está sonando
            if (detectionAudioSource != null && !detectionAudioSource.isPlaying)
            {
                if (fadeAudioCoroutine != null) StopCoroutine(fadeAudioCoroutine);
                detectionAudioSource.volume = originalDetectionVolume;
                detectionAudioSource.Play();
            }

            // Si supera el tiempo límite se dispara la alarma
            if (detectionTimer >= timeToTriggerAlarm)
            {
                TriggerAlarm();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ResetDetection();
        }
    }

    private void ResetDetection()
    {
        detectionTimer = 0f;
        isPlayerInZone = false;

        // ffrenar sonido de deteccion si el jugador sale del collider
        StopDetectionAudioSmoothly();
    }

    private void TriggerAlarm()
    {
        isAlarmActive = true;

        // activar alarma y apagar deteccion 
        StopDetectionAudioSmoothly();

        // prender el area de ruido del monstruo
        if (monsterNoiseArea != null)
        {
            monsterNoiseArea.SetActive(true);
        }

        // apagar la alarma después del tiempo especificado
        if (alarmCoroutine != null) StopCoroutine(alarmCoroutine);
        alarmCoroutine = StartCoroutine(AlarmSequenceRoutine());
    }

    private IEnumerator AlarmSequenceRoutine()
    {
        yield return new WaitForSeconds(alarmDuration);

        // Apagar el collider de ruido
        if (monsterNoiseArea != null)
        {
            monsterNoiseArea.SetActive(false);
        }

        // Si el jugador sigue en la zona al terminar el tiempo, reactiva la alarma en bucle
        if (isPlayerInZone)
        {
            TriggerAlarm();
        }
        else
        {
            // Resetear la camara 
            ResetDetection();
            isAlarmActive = false;
        }
    }

    // Método auxiliar para bajar el volumen suavemente antes de frenar el audio
    private void StopDetectionAudioSmoothly()
    {
        if (detectionAudioSource != null && detectionAudioSource.isPlaying)
        {
            if (fadeAudioCoroutine != null) StopCoroutine(fadeAudioCoroutine);
            fadeAudioCoroutine = StartCoroutine(FadeOutAudioRoutine(detectionAudioSource, fadeDuration));
        }
    }

    private IEnumerator FadeOutAudioRoutine(AudioSource audioSrc, float duration)
    {
        float startVolume = audioSrc.volume;

        while (audioSrc.volume > 0)
        {
            audioSrc.volume -= startVolume * (Time.deltaTime / duration);
            yield return null;
        }

        audioSrc.Stop();
        audioSrc.volume = originalDetectionVolume;
    }
}