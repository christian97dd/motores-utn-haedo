using System.Collections.Generic;
using UnityEngine;

// guarda el ultimo checkpoint, es estatico para que sobreviva a la recarga de la escena cuando el jugador pierde.

public static class Checkpoint
{
    public static bool HasCheckpoint { get; private set; }
    public static Vector3 Position { get; private set; }
    public static Quaternion Rotation { get; private set; }

    private static readonly HashSet<string> solvedIds = new HashSet<string>();

    public static void Save(string puzzleId, Vector3 position, Quaternion rotation)
    {
        solvedIds.Add(puzzleId);
        Position = position;
        Rotation = rotation;
        HasCheckpoint = true;
    }

    public static bool IsSolved(string puzzleId)
    {
        return solvedIds.Contains(puzzleId);
    }

    public static void Clear()
    {
        HasCheckpoint = false;
        solvedIds.Clear();
    }

    // al darle play en el editor siempre se empieza sin checkpoint
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        Clear();
    }
}