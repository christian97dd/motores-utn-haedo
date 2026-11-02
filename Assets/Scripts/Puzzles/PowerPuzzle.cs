using System.Collections.Generic;
using UnityEngine;

public enum PowerPieceType
{
    Straight,
    Corner,
    Tee
}

// Desvio de energia: una grilla de piezas que se giran hasta llevar la energia
// de la fuente (izquierda) a la puerta (derecha). El camino se arma al azar cada vez que se abre.
public class PowerPuzzle : Puzzle
{
    // lados de cada pieza, en el orden en que gira (sentido horario)
    public const int Up = 0, Right = 1, Down = 2, Left = 3;

    [Header("Grilla")]
    [Range(3, 9)]
    [SerializeField] private int columns = 7;
    [Range(2, 6)]
    [SerializeField] private int rows = 4;
    [SerializeField] private int sourceRow = 1; // fila de la fuente, contando desde arriba
    [SerializeField] private int doorRow = 2;
    [Tooltip("Cuantas piezas del camino arrancan mal giradas como minimo")]
    [SerializeField] private int minScrambled = 4;

    public int Columns => columns;
    public int Rows => rows;
    public int SourceRow => sourceRow;
    public int DoorRow => doorRow;

    private PowerPieceType[,] types;
    private int[,] rotations;
    private int[,] solution; // lados que tiene que conectar cada pieza del camino (0 si no es del camino)
    private bool[,] powered;

    public void Generate()
    {
        types = new PowerPieceType[columns, rows];
        rotations = new int[columns, rows];
        solution = new int[columns, rows];
        powered = new bool[columns, rows];

        BuildPath();

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (solution[x, y] != 0)
                {
                    types[x, y] = IsStraight(solution[x, y]) ? PowerPieceType.Straight : PowerPieceType.Corner;
                }
                else
                {
                    // el resto son piezas de relleno para confundir
                    float roll = Random.value;
                    types[x, y] = roll < 0.4f ? PowerPieceType.Corner : roll < 0.8f ? PowerPieceType.Straight : PowerPieceType.Tee;
                }

                rotations[x, y] = Random.Range(0, 4);
            }
        }

        EnsureScrambled();
        UpdatePower();
    }

    public PowerPieceType PieceType(int x, int y) => types[x, y];
    public int GetRotation(int x, int y) => rotations[x, y];
    public bool IsPowered(int x, int y) => powered[x, y];
    public int Sides(int x, int y) => Rotate(BaseSides(types[x, y]), rotations[x, y]);

    public int WrongPieces
    {
        get
        {
            int count = 0;
            for (int x = 0; x < columns; x++)
                for (int y = 0; y < rows; y++)
                    if (solution[x, y] != 0 && Sides(x, y) != solution[x, y]) count++;
            return count;
        }
    }

    public void RotatePiece(int x, int y)
    {
        if (IsSolved) return;

        rotations[x, y] = (rotations[x, y] + 1) % 4;
        UpdatePower();

        if (IsDoorPowered()) Solve();
    }

    public bool IsDoorPowered()
    {
        int last = columns - 1;
        return powered[last, doorRow] && Has(Sides(last, doorRow), Right);
    }

    // recorre desde la fuente y marca todas las piezas a las que llega la energia
    private void UpdatePower()
    {
        System.Array.Clear(powered, 0, powered.Length);
        if (!Has(Sides(0, sourceRow), Left)) return;

        var pending = new Queue<Vector2Int>();
        pending.Enqueue(new Vector2Int(0, sourceRow));
        powered[0, sourceRow] = true;

        while (pending.Count > 0)
        {
            Vector2Int cell = pending.Dequeue();
            int sides = Sides(cell.x, cell.y);

            for (int side = 0; side < 4; side++)
            {
                if (!Has(sides, side)) continue;

                Vector2Int next = cell + Step(side);
                if (next.x < 0 || next.y < 0 || next.x >= columns || next.y >= rows || powered[next.x, next.y]) continue;
                if (!Has(Sides(next.x, next.y), Opposite(side))) continue;

                powered[next.x, next.y] = true;
                pending.Enqueue(next);
            }
        }
    }

    // en cada columna el camino sube o baja un poco y despues sigue a la derecha
    private void BuildPath()
    {
        int row = sourceRow;
        int entry = Left;

        for (int x = 0; x < columns; x++)
        {
            int exitRow = x == columns - 1 ? doorRow : Random.value < 0.6f ? Random.Range(0, rows) : row;
            int dir = exitRow > row ? Down : Up;

            while (row != exitRow)
            {
                solution[x, row] = Bit(entry) | Bit(dir);
                entry = Opposite(dir);
                row += dir == Down ? 1 : -1;
            }

            solution[x, row] = Bit(entry) | Bit(Right);
            entry = Left;
        }
    }

    private void EnsureScrambled()
    {
        var path = new List<Vector2Int>();
        for (int x = 0; x < columns; x++)
            for (int y = 0; y < rows; y++)
                if (solution[x, y] != 0) path.Add(new Vector2Int(x, y));

        int wanted = Mathf.Min(minScrambled, path.Count);
        while (WrongPieces < wanted)
        {
            Vector2Int cell = path[Random.Range(0, path.Count)];
            rotations[cell.x, cell.y] = (rotations[cell.x, cell.y] + Random.Range(1, 4)) % 4;
        }
    }

    private static int BaseSides(PowerPieceType type)
    {
        switch (type)
        {
            case PowerPieceType.Straight: return Bit(Up) | Bit(Down);
            case PowerPieceType.Corner: return Bit(Up) | Bit(Right);
            default: return Bit(Up) | Bit(Right) | Bit(Down);
        }
    }

    private static int Rotate(int sides, int turns)
    {
        for (int i = 0; i < turns; i++) sides = ((sides << 1) | (sides >> 3)) & 0xF;
        return sides;
    }

    private static bool IsStraight(int sides) => sides == (Bit(Up) | Bit(Down)) || sides == (Bit(Left) | Bit(Right));
    private static int Bit(int side) => 1 << side;
    private static bool Has(int sides, int side) => (sides & Bit(side)) != 0;
    private static int Opposite(int side) => (side + 2) % 4;

    private static Vector2Int Step(int side)
    {
        switch (side)
        {
            case Up: return new Vector2Int(0, -1);
            case Right: return new Vector2Int(1, 0);
            case Down: return new Vector2Int(0, 1);
            default: return new Vector2Int(-1, 0);
        }
    }

    private void OnValidate()
    {
        sourceRow = Mathf.Clamp(sourceRow, 0, rows - 1);
        doorRow = Mathf.Clamp(doorRow, 0, rows - 1);
    }
}
