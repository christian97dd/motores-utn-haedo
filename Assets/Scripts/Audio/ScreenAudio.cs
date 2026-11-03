using System;
using UnityEngine;
using UnityEngine.Audio;

// audio de las pantallas fuera del nivel (menu, victoria, derrota): la musica de fondo y
// sonidos sueltos que entran a destiempo, por ejemplo la puerta y despues la respiracion
public class ScreenAudio : MonoBehaviour
{
    [Serializable]
    private class Cue
    {
        public AudioSource source;
        public float delay; // segundos desde que carga la pantalla
    }

    // el mixer sigue como lo dejo el nivel: si se perdio en una persecucion o se salio desde
    // la pausa, la musica de esta pantalla sonaria filtrada
    [SerializeField] private AudioMixerSnapshot startSnapshot;
    [SerializeField] private AudioSource music;
    [SerializeField] private Cue[] cues;

    private void Start()
    {
        if (startSnapshot != null) startSnapshot.TransitionTo(0f);
        if (music != null) music.Play();

        // agendados en el reloj de audio: si la carga traba un cuadro, igual respetan la distancia entre si
        double now = AudioSettings.dspTime;
        foreach (Cue cue in cues)
        {
            if (cue.source != null) cue.source.PlayScheduled(now + cue.delay);
        }
    }
}
