using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// click de todos los botones de este canvas. Suena desde un objeto aparte que sobrevive al cambio
// de escena, asi "Jugar" o "Reintentar" no lo cortan, e ignora la pausa del audio del menu de pausa
public class UIClickSound : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioMixerGroup output; // grupo SFX
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    private void Awake()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            button.onClick.AddListener(Play);
        }
    }

    private void Play()
    {
        if (clip == null) return;

        GameObject sound = new GameObject("Click");
        DontDestroyOnLoad(sound);

        AudioSource source = sound.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = output;
        source.ignoreListenerPause = true;
        source.spatialBlend = 0f;
        source.PlayOneShot(clip, volume);

        Destroy(sound, clip.length + 0.1f);
    }
}
