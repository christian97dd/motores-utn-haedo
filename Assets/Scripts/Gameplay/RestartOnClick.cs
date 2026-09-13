using UnityEngine;
using UnityEngine.InputSystem;

// Va en las escenas de victoria y derrota: un click vuelve al juego
public class RestartOnClick : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            GameFlow.PlayAgain();
        }
    }
}
