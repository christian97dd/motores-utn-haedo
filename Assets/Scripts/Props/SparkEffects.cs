using UnityEngine;

// chispas del nivel: al resolver un puzzle salta una descarga en su panel (y en la consola que enciende)
// y algunas luces que parpadean largan chispas. Se engancha solo a lo que hay en la escena
public class SparkEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparksPrefab; // se destruye solo al terminar

    [Header("Luces que parpadean")]
    [SerializeField] [Range(0f, 1f)] private float flickerSparkChance = 0.3f; // no todos los cortes tiran chispas
    [SerializeField] private float flickerMaxDistance = 18f; // lejos del jugador no se ven: ni se crean

    [Header("Puzzles")]
    // al reaparecer en un checkpoint los puzzles ya resueltos se vuelven a resolver al cargar
    [SerializeField] private float startupIgnoreTime = 0.5f;

    private Transform player;

    private void Start()
    {
        Player found = FindFirstObjectByType<Player>();
        if (found != null) player = found.transform;

        PuzzleInteractable[] interactables = FindObjectsByType<PuzzleInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Puzzle puzzle in FindObjectsByType<Puzzle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform point = GetPuzzlePoint(puzzle, interactables);
            puzzle.onSolved.AddListener(() => SparkOnSolve(point));
        }

        foreach (HintConsole console in FindObjectsByType<HintConsole>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (console.PowerPuzzle == null) continue;

            Transform point = console.transform;
            console.PowerPuzzle.onSolved.AddListener(() => SparkOnSolve(point));
        }

        foreach (HorrorFlicker flicker in FindObjectsByType<HorrorFlicker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform point = flicker.transform;
            flicker.FlickeredOff += () => SparkOnFlicker(point);
        }
    }

    // las chispas salen del panel que toca el jugador; si el puzzle no tiene uno, de donde esta el puzzle
    private static Transform GetPuzzlePoint(Puzzle puzzle, PuzzleInteractable[] interactables)
    {
        foreach (PuzzleInteractable interactable in interactables)
        {
            if (interactable.Puzzle == puzzle) return interactable.transform;
        }

        return puzzle.transform;
    }

    private void SparkOnSolve(Transform point)
    {
        if (Time.timeSinceLevelLoad < startupIgnoreTime) return;

        Spawn(point.position, Quaternion.Euler(-90f, 0f, 0f)); // hacia arriba, despues caen
    }

    private void SparkOnFlicker(Transform point)
    {
        if (Random.value > flickerSparkChance) return;
        if (player != null && Vector3.Distance(player.position, point.position) > flickerMaxDistance) return;

        Spawn(point.position + Vector3.down * 0.1f, Quaternion.Euler(90f, 0f, 0f)); // desde la luz hacia el piso
    }

    private void Spawn(Vector3 position, Quaternion rotation)
    {
        if (sparksPrefab != null) Instantiate(sparksPrefab, position, rotation);
    }
}
