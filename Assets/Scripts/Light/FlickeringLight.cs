using System.Collections;
using UnityEngine;

public class HorrorFlicker : MonoBehaviour
{
    [SerializeField] private Light targetLight;

    [SerializeField] private float normalIntensity = 4f;
    [SerializeField] private float brokenIntensity = 0.2f;

    [SerializeField] private float minStableTime = 2f;
    [SerializeField] private float maxStableTime = 6f;

    [SerializeField] private float minFlickerTime = 0.04f;
    [SerializeField] private float maxFlickerTime = 0.15f;

    private void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Luz estable
            targetLight.intensity = normalIntensity;

            yield return new WaitForSeconds(
                Random.Range(minStableTime, maxStableTime)
            );

            // Número aleatorio de fallos
            int flickers = Random.Range(2, 6);

            for (int i = 0; i < flickers; i++)
            {
                targetLight.intensity = brokenIntensity;

                yield return new WaitForSeconds(
                    Random.Range(minFlickerTime, maxFlickerTime)
                );

                targetLight.intensity = normalIntensity;

                yield return new WaitForSeconds(
                    Random.Range(minFlickerTime, maxFlickerTime)
                );
            }
        }
    }
}