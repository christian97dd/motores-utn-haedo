using UnityEngine;

// va en el panel de la pared (necesita collider)
public class PuzzleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Puzzle puzzle;

    [Header("Leyenda")]
    [SerializeField] private string displayName = "PANEL DE ACCESO";
    [SerializeField] private string actionText = "Usar panel";

    public InteractionPrompt Prompt => puzzle != null && puzzle.IsSolved
        ? new InteractionPrompt(displayName, "Ya resuelto", InteractionPromptStyle.Done)
        : new InteractionPrompt(displayName, actionText);

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
