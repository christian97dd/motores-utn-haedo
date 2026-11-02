using System;
using UnityEngine;

[Serializable]
public class WireColor
{
    public string name;
    public Color color = Color.white;
}

// Reparar cableado: cada cable de la izquierda va al conector de su mismo color,
// que a la derecha aparecen mezclados. Conectar uno mal cuenta como error.
public class WiringPuzzle : Puzzle
{
    [Header("Cables")]
    [SerializeField] private WireColor[] wires =
    {
        new WireColor { name = "ROJO", color = new Color(1f, 0.35f, 0.39f) },
        new WireColor { name = "AMARILLO", color = new Color(1f, 0.71f, 0.24f) },
        new WireColor { name = "VERDE", color = new Color(0.35f, 0.9f, 0.59f) },
        new WireColor { name = "CELESTE", color = new Color(0.47f, 0.86f, 0.96f) },
        new WireColor { name = "VIOLETA", color = new Color(0.72f, 0.5f, 0.98f) },
    };

    public WireColor[] Wires => wires;

    // socketOrder[i] = que cable va en el conector i de la derecha
    public int[] SocketOrder { get; private set; }
    public int ConnectedCount { get; private set; }

    private bool[] connected;

    public void Shuffle()
    {
        connected = new bool[wires.Length];
        ConnectedCount = 0;
        SocketOrder = new int[wires.Length];
        for (int i = 0; i < wires.Length; i++) SocketOrder[i] = i;

        // ningun cable queda enfrente de su conector, si no es demasiado facil
        do
        {
            for (int i = SocketOrder.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (SocketOrder[i], SocketOrder[j]) = (SocketOrder[j], SocketOrder[i]);
            }
        }
        while (wires.Length > 1 && HasWireInFront());
    }

    public bool IsConnected(int wire) => connected[wire];

    public bool TryConnect(int wire, int socket)
    {
        if (IsSolved || connected[wire]) return false;

        if (SocketOrder[socket] != wire)
        {
            Fail();
            return false;
        }

        connected[wire] = true;
        ConnectedCount++;
        if (ConnectedCount == wires.Length) Solve();
        return true;
    }

    private bool HasWireInFront()
    {
        for (int i = 0; i < SocketOrder.Length; i++)
            if (SocketOrder[i] == i) return true;
        return false;
    }
}
