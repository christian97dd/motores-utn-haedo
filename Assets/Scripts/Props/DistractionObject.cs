using UnityEngine;

// lo que se tira: se rompe contra lo primero que toca y el ruido atrae a los enemigos a ese punto
[RequireComponent(typeof(Rigidbody))]
public class DistractionObject : MonoBehaviour
{
    [SerializeField] private float noiseRadius = 15f;
    public float NoiseRadius => noiseRadius;

    [Header("Al romperse")]
    [SerializeField] private AudioSource breakEffect; // prefab con el sonido y las particulas, sale por el grupo SFX
    [SerializeField] private AudioClip[] breakClips;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private bool broken;

    private void OnCollisionEnter(Collision collision)
    {
        if (broken) return;
        broken = true;

        Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        PlayBreak(point);
        Noise.Emit(point, noiseRadius);

        Destroy(gameObject);
    }

    private void PlayBreak(Vector3 point)
    {
        if (breakEffect == null) return;

        AudioSource effect = Instantiate(breakEffect, point, Quaternion.identity);

        // clip y tono al azar para que dos botellas no suenen igual
        if (breakClips.Length > 0)
        {
            effect.clip = breakClips[Random.Range(0, breakClips.Length)];
            effect.pitch = Random.Range(pitchRange.x, pitchRange.y);
            effect.Play();
        }

        float clipLength = effect.clip != null ? effect.clip.length / effect.pitch : 0f;
        Destroy(effect.gameObject, Mathf.Max(clipLength, 2f));
    }
}
