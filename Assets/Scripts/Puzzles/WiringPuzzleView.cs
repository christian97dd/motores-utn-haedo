using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Contenido de la ventana para el cableado: los cables de la izquierda se arrastran
// con el mouse hasta el conector de su color.
public class WiringPuzzleView : PuzzleView
{
    [Header("Partes")]
    [SerializeField] private WiringConnector plugTemplate; // los de la izquierda, quedan desactivados
    [SerializeField] private WiringConnector socketTemplate;
    [SerializeField] private GameObject cableTemplate;
    [SerializeField] private RectTransform cables; // los cables se dibujan aca, debajo de los conectores
    [SerializeField] private RectTransform cableTip; // la puntita que sigue al mouse
    [SerializeField] private float rowSpacing = 110f;

    [Header("Colores")]
    [SerializeField] private Color idleFrameColor = new Color(0.2f, 0.37f, 0.45f);

    public override string Instructions => puzzle == null ? "" : puzzle.ConnectedCount + " / " + puzzle.Wires.Length + " cables conectados";
    public override string ErrorMessage => "CONEXIÓN INCORRECTA";
    public override string SolvedMessage => "CIRCUITO REPARADO";
    public override string Controls => "arrastrá con el mouse · ESC para salir";

    private readonly List<WiringConnector> plugs = new List<WiringConnector>();
    private readonly List<WiringConnector> sockets = new List<WiringConnector>();
    private readonly List<GameObject> cableInstances = new List<GameObject>();
    private WiringPuzzle puzzle;
    private PuzzleState state;
    private GameObject draggedCable;

    private void Awake()
    {
        plugTemplate.gameObject.SetActive(false);
        socketTemplate.gameObject.SetActive(false);
        cableTemplate.SetActive(false);
        cableTip.gameObject.SetActive(false);
    }

    public override bool CanShow(Puzzle puzzle)
    {
        return puzzle is WiringPuzzle;
    }

    public override void Show(Puzzle puzzle)
    {
        base.Show(puzzle);
        this.puzzle = (WiringPuzzle)puzzle;
        this.puzzle.Shuffle();
        Build();
    }

    public override void Hide()
    {
        DropCable();
        base.Hide();
    }

    public override void OnStateChanged(PuzzleState newState)
    {
        state = newState;
        if (state != PuzzleState.Normal) DropCable();
    }

    private void Build()
    {
        foreach (WiringConnector connector in plugs) Destroy(connector.gameObject);
        foreach (WiringConnector connector in sockets) Destroy(connector.gameObject);
        foreach (GameObject cable in cableInstances) Destroy(cable);
        plugs.Clear();
        sockets.Clear();
        cableInstances.Clear();

        WireColor[] wires = puzzle.Wires;
        for (int i = 0; i < wires.Length; i++)
        {
            float y = ((wires.Length - 1) / 2f - i) * rowSpacing;

            WiringConnector plug = Spawn(plugTemplate, y);
            plug.Setup(i, wires[i].name, wires[i].color);
            plug.SetFrameColor(wires[i].color);
            plug.DragStarted = OnDragStarted;
            plug.Dragged = OnDragged;
            plug.DragEnded = OnDragEnded;
            plugs.Add(plug);

            WireColor socketWire = wires[puzzle.SocketOrder[i]];
            WiringConnector socket = Spawn(socketTemplate, y);
            socket.Setup(i, socketWire.name, socketWire.color);
            socket.SetFrameColor(idleFrameColor);
            sockets.Add(socket);
        }
    }

    private WiringConnector Spawn(WiringConnector template, float y)
    {
        WiringConnector connector = Instantiate(template, template.transform.parent);
        connector.gameObject.SetActive(true);
        RectTransform rect = (RectTransform)connector.transform;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        return connector;
    }

    private void OnDragStarted(WiringConnector plug, PointerEventData eventData)
    {
        if (state != PuzzleState.Normal || puzzle.IsConnected(plug.Index)) return;

        draggedCable = NewCable(puzzle.Wires[plug.Index].color);
        cableTip.GetComponent<Image>().color = puzzle.Wires[plug.Index].color;
        cableTip.gameObject.SetActive(true);
        OnDragged(plug, eventData);
    }

    private void OnDragged(WiringConnector plug, PointerEventData eventData)
    {
        if (draggedCable == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(cables, eventData.position, eventData.pressEventCamera, out Vector2 pointer);
        SetCable(draggedCable, plug.CablePoint, pointer);
        cableTip.anchoredPosition = pointer;
    }

    private void OnDragEnded(WiringConnector plug, PointerEventData eventData)
    {
        if (draggedCable == null) return;

        GameObject target = eventData.pointerCurrentRaycast.gameObject;
        WiringConnector socket = target != null ? target.GetComponentInParent<WiringConnector>() : null;

        // se suelta antes de conectar: con el ultimo cable el puzzle se resuelve y el cambio de estado lo borraria
        GameObject cable = draggedCable;
        draggedCable = null;
        cableTip.gameObject.SetActive(false);

        if (socket != null && sockets.Contains(socket) && puzzle.TryConnect(plug.Index, socket.Index))
        {
            SetCable(cable, plug.CablePoint, socket.CablePoint);
            socket.SetFrameColor(puzzle.Wires[plug.Index].color);
            cableInstances.Add(cable);
            RefreshMessage();
            return;
        }

        Destroy(cable);
    }

    private void DropCable()
    {
        if (draggedCable != null) Destroy(draggedCable);
        draggedCable = null;
        cableTip.gameObject.SetActive(false);
    }

    private GameObject NewCable(Color color)
    {
        GameObject cable = Instantiate(cableTemplate, cables);
        cable.SetActive(true);
        foreach (UICable part in cable.GetComponentsInChildren<UICable>())
        {
            // el brillo de atras usa el mismo color pero transparente
            part.color = new Color(color.r, color.g, color.b, part.color.a);
        }
        return cable;
    }

    private static void SetCable(GameObject cable, Vector2 from, Vector2 to)
    {
        foreach (UICable part in cable.GetComponentsInChildren<UICable>()) part.SetPoints(from, to);
    }
}
