using UnityEngine;
using UnityEngine.InputSystem;

// Va en las escenas de victoria y derrota: un click vuelve al juego
public class RestartOnClick : MonoBehaviour
{
    // el juego deja el cursor bloqueado, acá se libera para poder clickear
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            GameFlow.PlayAgain();
        }
    }
}
