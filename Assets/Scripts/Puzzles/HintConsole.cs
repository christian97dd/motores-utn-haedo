using UnityEngine;

// consola de pista, sin energia solo avisa "sin energia"; cuando se resuelve el puzzle de cables, al interactuar abre la ficha  con el codigo de una puerta.

public class HintConsole : MonoBehaviour, IInteractable
{
    [Header("Energia")]
    [SerializeField] private Puzzle powerPuzzle;      // el puzzle de cables que la enciende

    [Header("Pista")]
    [SerializeField] private DocumentData document;   // la ficha que se lee con energia
    [Tooltip("Opcional: luz o pantalla que se enciende con la energia")]
    [SerializeField] private GameObject screenRoot;

    [Header("Leyenda")]
    [SerializeField] private string displayName = "CONSOLA";
    [SerializeField] private string unpoweredText = "Sin energía";
    [SerializeField] private string poweredText = "Leer archivo";

    private bool IsPowered => powerPuzzle != null && powerPuzzle.IsSolved;

    public InteractionPrompt Prompt => IsPowered
        ? new InteractionPrompt(displayName, poweredText)
        : new InteractionPrompt(displayName, unpoweredText, InteractionPromptStyle.Locked);

    private void Awake()
    {
        if (powerPuzzle == null)
            Debug.LogError("HintConsole: falta el puzzle de energia en " + gameObject.name, this);

        UpdateScreen();
    }

    private void OnEnable()
    {
        if (powerPuzzle != null) powerPuzzle.onSolved.AddListener(UpdateScreen);
    }

    private void OnDisable()
    {
        if (powerPuzzle != null) powerPuzzle.onSolved.RemoveListener(UpdateScreen);
    }

    public void Interact()
    {
        if (!IsPowered) return;

        if (document == null || DocumentWindow.Current == null)
        {
            Debug.LogError("HintConsole: falta el documento o no hay DocumentWindow en la escena", this);
            return;
        }

        DocumentWindow.Current.Open(document);
    }

    private void UpdateScreen()
    {
        if (screenRoot != null) screenRoot.SetActive(IsPowered);
    }
}