using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Contenido de la ventana para el puzzle binario: los casilleros, el valor y el teclado.
// Se escribe con los botones o con 0, 1, retroceso y Enter.
public class BinaryPuzzleView : PuzzleView
{
    [Header("Casilleros")]
    [SerializeField] private BinaryBitCell cellTemplate;   // queda desactivado, se clona uno por bit
    [SerializeField] private Transform cellsContainer;
    [SerializeField] private TMP_Text valueLabel;
    [SerializeField] private Button confirmButton;

    [Header("Colores")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.37f, 0.45f);
    [SerializeField] private Color filledColor = new Color(0.47f, 0.78f, 0.88f);
    [SerializeField] private Color nextColor = new Color(0.47f, 0.86f, 0.96f);
    [SerializeField] private Color errorColor = new Color(1f, 0.35f, 0.39f);
    [SerializeField] private Color solvedColor = new Color(0.35f, 0.9f, 0.59f);
    [SerializeField] private Color oneColor = new Color(0.92f, 0.98f, 1f);
    [SerializeField] private Color zeroColor = new Color(0.47f, 0.63f, 0.69f);

    public override string Instructions => "Ingresá el código de acceso";
    public override string ErrorMessage => "CÓDIGO INCORRECTO";
    public override string SolvedMessage => "ACCESO CONCEDIDO";

    private readonly List<BinaryBitCell> cells = new List<BinaryBitCell>();
    private readonly StringBuilder code = new StringBuilder();
    private BinaryPanel panel;
    private PuzzleState state;

    private InputAction zeroAction;
    private InputAction oneAction;
    private InputAction eraseAction;
    private InputAction confirmAction;

    private void Awake()
    {
        cellTemplate.gameObject.SetActive(false);
        zeroAction = InputSystem.actions.FindAction("Puzzle/Zero");
        oneAction = InputSystem.actions.FindAction("Puzzle/One");
        eraseAction = InputSystem.actions.FindAction("Puzzle/Erase");
        confirmAction = InputSystem.actions.FindAction("Puzzle/Confirm");
    }

    public override bool CanShow(Puzzle puzzle)
    {
        return puzzle is BinaryPanel;
    }

    public override void Show(Puzzle puzzle)
    {
        base.Show(puzzle);
        panel = (BinaryPanel)puzzle;
        code.Clear();
        BuildCells();

        zeroAction.performed += OnZero;
        oneAction.performed += OnOne;
        eraseAction.performed += OnErase;
        confirmAction.performed += OnConfirm;
    }

    public override void Hide()
    {
        if (zeroAction != null)
        {
            zeroAction.performed -= OnZero;
            oneAction.performed -= OnOne;
            eraseAction.performed -= OnErase;
            confirmAction.performed -= OnConfirm;
        }

        base.Hide();
    }

    public override void OnStateChanged(PuzzleState newState)
    {
        // al volver de un error se arranca de cero
        if (state == PuzzleState.Error && newState == PuzzleState.Normal) code.Clear();

        state = newState;
        Refresh();
    }

    // los usan los botones en el OnClick
    public void PressZero() => AddDigit('0');
    public void PressOne() => AddDigit('1');

    public void PressErase()
    {
        if (state != PuzzleState.Normal || code.Length == 0) return;

        code.Length--;
        Refresh();
    }

    public void PressConfirm()
    {
        if (state != PuzzleState.Normal || code.Length < panel.Data.BitCount) return;

        panel.SubmitBinaryString(code.ToString());
    }

    private void OnZero(InputAction.CallbackContext context) => PressZero();
    private void OnOne(InputAction.CallbackContext context) => PressOne();
    private void OnErase(InputAction.CallbackContext context) => PressErase();
    private void OnConfirm(InputAction.CallbackContext context) => PressConfirm();

    private void AddDigit(char digit)
    {
        if (state != PuzzleState.Normal || code.Length >= panel.Data.BitCount) return;

        code.Append(digit);
        Refresh();
    }

    private void BuildCells()
    {
        foreach (BinaryBitCell cell in cells) Destroy(cell.gameObject);
        cells.Clear();

        int bitCount = panel.Data.BitCount;
        for (int i = 0; i < bitCount; i++)
        {
            BinaryBitCell cell = Instantiate(cellTemplate, cellsContainer);
            cell.gameObject.SetActive(true);
            cell.Setup(1 << (bitCount - 1 - i), panel.Data.ShowBitWeights);
            cells.Add(cell);
        }

        valueLabel.gameObject.SetActive(panel.Data.ShowDecimalValue);
    }

    private void Refresh()
    {
        if (panel == null) return;

        int value = 0;
        for (int i = 0; i < cells.Count; i++)
        {
            bool filled = i < code.Length;
            char digit = filled ? code[i] : '\0';
            if (digit == '1') value += 1 << (cells.Count - 1 - i);

            Color frameColor = state == PuzzleState.Error ? errorColor
                : state == PuzzleState.Solved ? solvedColor
                : filled ? filledColor
                : i == code.Length ? nextColor
                : emptyColor;

            bool isNext = state == PuzzleState.Normal && i == code.Length;
            cells[i].Show(digit, isNext, frameColor, digit == '1' ? oneColor : zeroColor);
        }

        valueLabel.text = "VALOR = " + value;
        confirmButton.interactable = state == PuzzleState.Normal && code.Length == cells.Count;

        // si un boton queda seleccionado despues del click, Enter lo apretaria ademas de confirmar
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
}
