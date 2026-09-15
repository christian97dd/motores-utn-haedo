using UnityEngine;

public class EnemyGuard : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private Player player;
    [Min(0f)]
    [SerializeField] private float detectionRadius = 5f;

    [Header("Testeo de estados")]
    [SerializeField] private bool isPlayerDetected = false;

    private void Update()
    {
        if (isPlayerDetected) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= detectionRadius && !player.IsCrouching)
        {
            isPlayerDetected = true;
            GameFlow.Defeat();
        }
    }

    // marca el radio de detección en la Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}