using UnityEngine;

// Va en la consola de un puzzle
// Cuando el puzzle falla atrae al androide hacia este punto
public class PuzzleFailAlert : MonoBehaviour
{
    [Tooltip("El puzzle que vigila. Cada vez que falla, se alerta a los androides")]
    [SerializeField] private Puzzle puzzle;

    [Tooltip("Punto al que va el androide. Si esta vacio, usa la posicion de este objeto")]
    [SerializeField] private Transform alertPoint;

    private void OnEnable()
    {
        if (puzzle != null) puzzle.onFailed.AddListener(Alert);
    }

    private void OnDisable()
    {
        if (puzzle != null) puzzle.onFailed.RemoveListener(Alert);
    }

    public void Alert()
    {
        Vector3 position = alertPoint != null ? alertPoint.position : transform.position;

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy.Chase(position);
        }
    }
}