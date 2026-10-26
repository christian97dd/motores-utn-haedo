using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PuzzleState
{
    Normal,
    Error,
    Solved
}

// La ventana de terminal que comparten todos los puzzles. Se encarga de abrir y cerrar,
// de los estados (error, resuelto) y de cortar los controles del jugador mientras esta abierta.
// Lo propio de cada puzzle lo dibuja su PuzzleView, que va como hija de esta ventana.
public class PuzzleWindow : MonoBehaviour
{
    public static PuzzleWindow Current { get; private set; }

    // el menu de pausa lo consulta para no abrirse con el mismo Esc que cierra el puzzle
    public static bool IsAnyOpen { get; private set; }

    [Header("Partes de la ventana")]
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Image frame;
    [SerializeField] private Image statusLight;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text subtitleLabel;
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private TMP_Text terminalLabel;
    [SerializeField] private TMP_Text hintLabel;
    [SerializeField] private TMP_Text footerLabel;

    [Header("Colores")]
    [SerializeField] private Color normalColor = new Color(0.47f, 0.78f, 0.88f);
    [SerializeField] private Color errorColor = new Color(1f, 0.35f, 0.39f);
    [SerializeField] private Color solvedColor = new Color(0.35f, 0.9f, 0.59f);
    [SerializeField] private Color messageColor = new Color(0.82f, 0.92f, 0.96f);

    [Header("Tiempos")]
    [SerializeField] private float errorDuration = 0.7f;      // cuanto queda en rojo antes de volver a normal
    [SerializeField] private float shakeStrength = 14f;
    [SerializeField] private float solvedCloseDelay = 1.2f;   // se ve el verde un momento y se cierra sola

    public PuzzleState State { get; private set; }

    private readonly List<PuzzleView> views = new List<PuzzleView>();
    private Puzzle currentPuzzle;
    private PuzzleView currentView;
    private Vector2 panelPosition;
    private InputActionMap playerMap;
    private InputActionMap puzzleMap;
    private InputAction cancelAction;

    private void Awake()
    {
        Current = this;
        GetComponentsInChildren(true, views);
        foreach (PuzzleView view in views) view.Hide();

        playerMap = InputSystem.actions.FindActionMap("Player");
        puzzleMap = InputSystem.actions.FindActionMap("Puzzle");
        cancelAction = InputSystem.actions.FindAction("System/Pause");

        panelPosition = panel.anchoredPosition;
        windowRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
        if (currentPuzzle != null) IsAnyOpen = false;
    }

    public void Open(Puzzle puzzle)
    {
        if (IsAnyOpen || puzzle.IsSolved) return;

        currentView = views.Find(view => view.CanShow(puzzle));
        if (currentView == null)
        {
            Debug.LogError("PuzzleWindow: no hay una vista para " + puzzle.GetType().Name, this);
            return;
        }

        currentPuzzle = puzzle;
        IsAnyOpen = true;

        titleLabel.text = puzzle.Title;
        subtitleLabel.text = puzzle.Subtitle;
        terminalLabel.text = puzzle.TerminalLabel;
        hintLabel.text = puzzle.Hint;
        footerLabel.text = currentView.Controls;
        panel.sizeDelta = currentView.PanelSize;

        puzzle.onSolved.AddListener(HandleSolved);
        puzzle.onFailed.AddListener(HandleFailed);
        cancelAction.performed += OnCancel;

        playerMap.Disable();
        puzzleMap.Enable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        windowRoot.SetActive(true);
        currentView.Show(puzzle);
        SetState(PuzzleState.Normal);
    }

    public void Close()
    {
        if (currentPuzzle == null) return;

        StopAllCoroutines();
        panel.anchoredPosition = panelPosition;

        currentPuzzle.onSolved.RemoveListener(HandleSolved);
        currentPuzzle.onFailed.RemoveListener(HandleFailed);
        cancelAction.performed -= OnCancel;

        currentView.Hide();
        windowRoot.SetActive(false);

        puzzleMap.Disable();
        playerMap.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentPuzzle = null;
        currentView = null;
        IsAnyOpen = false;
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        Close();
    }

    private void HandleFailed()
    {
        StopAllCoroutines();
        StartCoroutine(ErrorRoutine());
    }

    private void HandleSolved()
    {
        StopAllCoroutines();
        StartCoroutine(SolvedRoutine());
    }

    private IEnumerator ErrorRoutine()
    {
        SetState(PuzzleState.Error);

        float elapsed = 0f;
        while (elapsed < errorDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            // el temblor arranca fuerte y se va apagando
            float strength = shakeStrength * (1f - elapsed / errorDuration);
            panel.anchoredPosition = panelPosition + new Vector2(Random.Range(-strength, strength), Random.Range(-strength, strength) * 0.4f);
            yield return null;
        }

        panel.anchoredPosition = panelPosition;
        SetState(PuzzleState.Normal);
    }

    private IEnumerator SolvedRoutine()
    {
        SetState(PuzzleState.Solved);
        yield return new WaitForSecondsRealtime(solvedCloseDelay);
        Close();
    }

    public void RefreshMessage()
    {
        if (currentView != null && State == PuzzleState.Normal) messageLabel.text = currentView.Instructions;
    }

    private void SetState(PuzzleState state)
    {
        State = state;

        Color accent = state == PuzzleState.Error ? errorColor : state == PuzzleState.Solved ? solvedColor : normalColor;
        frame.color = accent;

        bool open = state == PuzzleState.Solved;
        statusLight.color = open ? solvedColor : errorColor;
        statusLabel.color = open ? solvedColor : errorColor;
        statusLabel.text = open ? "OPEN" : "LOCK";

        messageLabel.color = state == PuzzleState.Normal ? messageColor : accent;
        messageLabel.text = state == PuzzleState.Error ? currentView.ErrorMessage
            : state == PuzzleState.Solved ? currentView.SolvedMessage
            : currentView.Instructions;

        currentView.OnStateChanged(state);
    }
}
