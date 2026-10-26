using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Contenido de la ventana para el desvio de energia: la grilla de piezas, la fuente y la puerta.
// Se gira con click o con espacio, y con las flechas se elige la pieza.
public class PowerPuzzleView : PuzzleView
{
    [Header("Grilla")]
    [SerializeField] private PowerTile tileTemplate; // queda desactivado, se clona uno por casillero
    [SerializeField] private RectTransform grid;
    [SerializeField] private float tileSize = 110f;
    [SerializeField] private float spacing = 8f;

    [Header("Fuente y puerta")]
    [SerializeField] private RectTransform source;
    [SerializeField] private RectTransform sourceWire;
    [SerializeField] private RectTransform door;
    [SerializeField] private RectTransform doorWire;
    [SerializeField] private Image doorFrame;
    [SerializeField] private Image doorIcon;
    [SerializeField] private Image doorWireImage;

    [Header("Colores")]
    [SerializeField] private Color pieceColor = new Color(0.17f, 0.33f, 0.42f);
    [SerializeField] private Color poweredColor = new Color(1f, 0.71f, 0.24f);
    [SerializeField] private Color frameColor = new Color(0.12f, 0.23f, 0.3f);
    [SerializeField] private Color selectedColor = new Color(0.47f, 0.86f, 0.96f);
    [SerializeField] private Color lockedColor = new Color(1f, 0.35f, 0.39f);
    [SerializeField] private Color solvedColor = new Color(0.35f, 0.9f, 0.59f);

    public override string Instructions
    {
        get
        {
            int wrong = puzzle != null ? puzzle.WrongPieces : 0;
            return wrong == 1 ? "Circuito incompleto: 1 pieza mal orientada"
                : "Circuito incompleto: " + wrong + " piezas mal orientadas";
        }
    }

    public override string SolvedMessage => "ENERGÍA RESTABLECIDA";
    public override string Controls => "click o espacio: girar · flechas: moverse · ESC para salir";

    private readonly List<PowerTile> tiles = new List<PowerTile>();
    private PowerPuzzle puzzle;
    private PuzzleState state;
    private Vector2Int selected;

    private InputAction moveAction;
    private InputAction rotateAction;

    private void Awake()
    {
        tileTemplate.gameObject.SetActive(false);
        moveAction = InputSystem.actions.FindAction("Puzzle/Move");
        rotateAction = InputSystem.actions.FindAction("Puzzle/Rotate");
    }

    public override bool CanShow(Puzzle puzzle)
    {
        return puzzle is PowerPuzzle;
    }

    public override void Show(Puzzle puzzle)
    {
        base.Show(puzzle);
        this.puzzle = (PowerPuzzle)puzzle;
        this.puzzle.Generate();
        selected = new Vector2Int(0, this.puzzle.SourceRow);
        BuildGrid();

        moveAction.performed += OnMove;
        rotateAction.performed += OnRotate;
    }

    public override void Hide()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            rotateAction.performed -= OnRotate;
        }

        base.Hide();
    }

    public override void OnStateChanged(PuzzleState newState)
    {
        state = newState;
        Refresh();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (state != PuzzleState.Normal || input == Vector2.zero) return;

        // en la grilla la fila 0 es la de arriba
        Vector2Int step = Mathf.Abs(input.x) > Mathf.Abs(input.y)
            ? new Vector2Int(input.x > 0 ? 1 : -1, 0)
            : new Vector2Int(0, input.y > 0 ? -1 : 1);

        selected.x = Mathf.Clamp(selected.x + step.x, 0, puzzle.Columns - 1);
        selected.y = Mathf.Clamp(selected.y + step.y, 0, puzzle.Rows - 1);
        Refresh();
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        RotateSelected();
    }

    private void OnTileClicked(PowerTile tile)
    {
        selected = tile.Cell;
        RotateSelected();
    }

    private void RotateSelected()
    {
        if (state != PuzzleState.Normal) return;

        puzzle.RotatePiece(selected.x, selected.y);
        TileAt(selected).TurnTo(puzzle.GetRotation(selected.x, selected.y));
        Refresh();
        RefreshMessage();
    }

    private void BuildGrid()
    {
        foreach (PowerTile tile in tiles) Destroy(tile.gameObject);
        tiles.Clear();

        float step = tileSize + spacing;
        grid.sizeDelta = new Vector2(puzzle.Columns * step - spacing, puzzle.Rows * step - spacing);

        for (int y = 0; y < puzzle.Rows; y++)
        {
            for (int x = 0; x < puzzle.Columns; x++)
            {
                PowerTile tile = Instantiate(tileTemplate, grid);
                tile.gameObject.SetActive(true);
                ((RectTransform)tile.transform).anchoredPosition = CellPosition(x, y);
                tile.Setup(new Vector2Int(x, y), puzzle.PieceType(x, y), puzzle.GetRotation(x, y));
                tile.Clicked = OnTileClicked;
                tiles.Add(tile);
            }
        }

        // la fuente y la puerta quedan pegadas a la grilla, a la altura de su fila
        float edge = grid.sizeDelta.x / 2f;
        source.anchoredPosition = new Vector2(-edge - 120f, CellPosition(0, puzzle.SourceRow).y);
        door.anchoredPosition = new Vector2(edge + 120f, CellPosition(0, puzzle.DoorRow).y);
        sourceWire.anchoredPosition = new Vector2(-edge - 60f, source.anchoredPosition.y);
        doorWire.anchoredPosition = new Vector2(edge + 60f, door.anchoredPosition.y);
    }

    private Vector2 CellPosition(int x, int y)
    {
        float step = tileSize + spacing;
        return new Vector2((x - (puzzle.Columns - 1) / 2f) * step, ((puzzle.Rows - 1) / 2f - y) * step);
    }

    private PowerTile TileAt(Vector2Int cell) => tiles[cell.y * puzzle.Columns + cell.x];

    private void Refresh()
    {
        if (puzzle == null) return;

        foreach (PowerTile tile in tiles)
        {
            bool isPowered = puzzle.IsPowered(tile.Cell.x, tile.Cell.y);
            bool isSelected = state == PuzzleState.Normal && tile.Cell == selected;
            Color color = isPowered ? (state == PuzzleState.Solved ? solvedColor : poweredColor) : pieceColor;
            tile.Show(color, isSelected ? selectedColor : frameColor);
        }

        bool open = puzzle.IsDoorPowered();
        doorFrame.color = doorIcon.color = open ? solvedColor : lockedColor;
        doorWireImage.color = open ? solvedColor : pieceColor;
    }
}
