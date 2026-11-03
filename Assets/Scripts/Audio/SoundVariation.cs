using System;
using UnityEngine;
using Random = UnityEngine.Random;

// varias tomas de un mismo sonido: elige una al azar sin repetir la anterior y le varia tono y volumen
[Serializable]
public class SoundVariation
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 volumeRange = new Vector2(0.85f, 1f);

    private int lastIndex = -1;

    public bool HasClips => clips != null && clips.Length > 0;

    public void Play(AudioSource source, float volumeScale = 1f)
    {
        if (!HasClips || source == null) return;

        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(NextClip(), Random.Range(volumeRange.x, volumeRange.y) * volumeScale);
    }

    private AudioClip NextClip()
    {
        if (clips.Length == 1) return clips[0];

        // se sortea entre las otras tomas: la misma dos veces seguidas es lo que mas se nota
        if (lastIndex < 0)
        {
            lastIndex = Random.Range(0, clips.Length);
            return clips[lastIndex];
        }

        int index = Random.Range(0, clips.Length - 1);
        if (index >= lastIndex) index++;
        lastIndex = index;
        return clips[index];
    }
}
