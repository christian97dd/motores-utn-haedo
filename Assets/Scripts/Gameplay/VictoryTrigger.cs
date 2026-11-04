using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private GameObject objetoAActivar;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objetoAActivar != null) objetoAActivar.SetActive(true);
            GameFlow.Victory();
        }
    }
}