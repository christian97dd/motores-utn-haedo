using UnityEngine;

// Va en el objeto de la pared que mira el jugador (tiene que tener el Collider).
// PlayerInteraction llama a Interact() y se abre el puzzle asignado, sea del tipo que sea.
public class PuzzleInteractable : MonoBehaviour
{
    [SerializeField] private Puzzle puzzle;

    public void Interact()
    {
        if (puzzle == null || PuzzleWindow.Current == null)
        {
            Debug.LogError("PuzzleInteractable: falta el puzzle o no hay PuzzleWindow en la escena", this);
            return;
        }

        PuzzleWindow.Current.Open(puzzle);
    }
}
