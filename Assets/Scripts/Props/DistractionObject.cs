using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DistractionObject : MonoBehaviour
{
    [SerializeField] private GameObject noisePrefab; // zona de ruido que queda donde impacto
    [SerializeField] private float noiseDuration = 0f; // poner mayor a 0 cuando se hagan los scripts de seguimiento por sonido

    // se rompe contra lo primero que toca y deja el ruido en ese lugar
    private void OnCollisionEnter(Collision collision)
    {
        if (noisePrefab != null)
        {
            GameObject noise = Instantiate(noisePrefab, transform.position, Quaternion.identity);
            Destroy(noise, noiseDuration);
        }

        Destroy(gameObject);
    }
}
