using System;
using UnityEngine;

// todo lo que hace ruido avisa por aca (pisadas, botellas) y los enemigos escuchan un solo evento
public static class Noise
{
    public static event Action<Vector3, float> Emitted;

    public static void Emit(Vector3 position, float radius)
    {
        Emitted?.Invoke(position, radius);
    }
}
