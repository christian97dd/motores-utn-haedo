using System.Collections;
using UnityEngine;

// los efectos de glitch leen Intensity de aca
public class GlitchController : MonoBehaviour
{
    [Header("Golpe inicial")]
    [SerializeField] private float introDelay = 0.5f; // que arranque cuando ya se ve la UI
    [SerializeField] private float introDuration = 0.35f;

    [Header("Glitches al azar")]
    [SerializeField] private Vector2 pauseRange = new Vector2(1.5f, 4f); // segundos entre glitch y glitch
    [SerializeField] private Vector2 burstDurationRange = new Vector2(0.1f, 0.35f);
    [SerializeField] private Vector2 burstIntensityRange = new Vector2(0.4f, 1f);
    [Range(0f, 1f)]
    [SerializeField] private float idleIntensity = 0.05f; // para que tiemble un poco siempre

    public float Intensity { get; private set; }

    private void OnEnable()
    {
        StartCoroutine(GlitchRoutine());
    }

    private IEnumerator GlitchRoutine()
    {
        Intensity = idleIntensity;
        yield return new WaitForSecondsRealtime(introDelay);
        yield return BurstRoutine(introDuration, 1f);

        while (true)
        {
            Intensity = idleIntensity;
            yield return new WaitForSecondsRealtime(Random.Range(pauseRange.x, pauseRange.y));

            float duration = Random.Range(burstDurationRange.x, burstDurationRange.y);
            float intensity = Random.Range(burstIntensityRange.x, burstIntensityRange.y);
            yield return BurstRoutine(duration, intensity);
        }
    }

    private IEnumerator BurstRoutine(float duration, float intensity)
    {
        Intensity = intensity;
        yield return new WaitForSecondsRealtime(duration);
    }
}
