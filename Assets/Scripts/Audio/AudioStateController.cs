using UnityEngine;
using UnityEngine.Audio;

public enum AudioState
{
    Exploration,
    Alert,
    Chase,
    Pause
}

// Elige el snapshot de MainMixer segun lo que pasa en el juego. Los snapshots solo mueven
// los grupos Musica Calma y Musica Tension y el low-pass de Music: el volumen de cada
// grupo principal lo maneja el menu de opciones con los parametros expuestos.
public class AudioStateController : MonoBehaviour
{
    [Header("Snapshots de MainMixer")]
    [SerializeField] private AudioMixerSnapshot exploration;
    [SerializeField] private AudioMixerSnapshot alert;
    [SerializeField] private AudioMixerSnapshot chase;
    [SerializeField] private AudioMixerSnapshot pause;

    [Header("Tiempos de transicion (segundos)")]
    [SerializeField] private float toChaseTime = 0.5f; // entrar en persecucion tiene que ser inmediato
    [SerializeField] private float toAlertTime = 1f;
    [SerializeField] private float toExplorationTime = 3f; // la calma vuelve despacio
    [SerializeField] private float pauseTime = 0.3f;

    [Header("Capas de musica")]
    [SerializeField] private AudioSource calmLayer;
    [SerializeField] private AudioSource tensionLayer;

    [Header("Referencias")]
    [SerializeField] private PauseMenu pauseMenu;

    public AudioState State { get; private set; }

    private Enemy[] enemies;
    private SecurityCameraDetection[] cameras;

    private void Start()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        cameras = FindObjectsByType<SecurityCameraDetection>(FindObjectsSortMode.None);
        if (pauseMenu == null) pauseMenu = FindFirstObjectByType<PauseMenu>();

        PlayLayersInSync();

        State = AudioState.Exploration;
        exploration.TransitionTo(0f);
    }

    private void Update()
    {
        AudioState next = ReadState();
        if (next == State) return;

        GetSnapshot(next).TransitionTo(GetTransitionTime(next));
        State = next;
    }

    // si hay varias cosas a la vez gana la mas importante: pausa, persecucion, alerta
    private AudioState ReadState()
    {
        if (pauseMenu != null && pauseMenu.IsPaused) return AudioState.Pause;

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.IsChasing) return AudioState.Chase;
        }

        foreach (SecurityCameraDetection cam in cameras)
        {
            if (cam != null && (cam.isPlayerInZone || cam.isAlarmActive)) return AudioState.Alert;
        }

        return AudioState.Exploration;
    }

    private AudioMixerSnapshot GetSnapshot(AudioState state)
    {
        switch (state)
        {
            case AudioState.Alert: return alert;
            case AudioState.Chase: return chase;
            case AudioState.Pause: return pause;
            default: return exploration;
        }
    }

    private float GetTransitionTime(AudioState state)
    {
        switch (state)
        {
            case AudioState.Alert: return toAlertTime;
            case AudioState.Chase: return toChaseTime;
            case AudioState.Pause: return pauseTime;
            default: return State == AudioState.Pause ? pauseTime : toExplorationTime;
        }
    }

    // las dos capas suenan siempre y los snapshots solo cambian cual se escucha,
    // por eso tienen que arrancar en el mismo instante para quedar a tiempo
    private void PlayLayersInSync()
    {
        // la pausa frena todo el audio con AudioListener.pause, la musica sigue sonando
        // apagada por el low-pass. Va por codigo porque Unity no guarda esta opcion en la escena
        if (calmLayer != null) calmLayer.ignoreListenerPause = true;
        if (tensionLayer != null) tensionLayer.ignoreListenerPause = true;

        double startTime = AudioSettings.dspTime + 0.1;
        if (calmLayer != null && calmLayer.clip != null) calmLayer.PlayScheduled(startTime);
        if (tensionLayer != null && tensionLayer.clip != null) tensionLayer.PlayScheduled(startTime);
    }
}
