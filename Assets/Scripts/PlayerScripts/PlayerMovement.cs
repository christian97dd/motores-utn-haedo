using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speedWalk = 5f;
    [SerializeField] private float speedRun = 15f;
    private Vector3 ultimaPosicion;
    private float velocidadActual;

    void Start()
    {
        ultimaPosicion = transform.position;
    }

    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            transform.Translate(new Vector3(0, 0, 1) * speedWalk * Time.deltaTime);
        }

        if (Keyboard.current.sKey.isPressed)
        {
            transform.Translate(new Vector3(0, 0, -1) * speedWalk * Time.deltaTime);
        }

        if (Keyboard.current.aKey.isPressed)
        {
            transform.Translate(new Vector3(-1, 0, 0) * speedWalk * Time.deltaTime);
        }

        if (Keyboard.current.dKey.isPressed)
        {
            transform.Translate(new Vector3(1, 0, 0) * speedWalk * Time.deltaTime);
        }

        if (Keyboard.current.leftShiftKey.isPressed)
        {
            speedWalk = speedRun;
        }
        else if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
        {
            speedWalk = 5f;
        }

        velocidadActual = (transform.position - ultimaPosicion).magnitude / Time.deltaTime;

        ultimaPosicion = transform.position;

        Debug.Log("Velocidad por Transform: " + velocidadActual + " m/s");
    }
}