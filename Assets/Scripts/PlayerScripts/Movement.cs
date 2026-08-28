using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
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
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate( new Vector3(0, 0, 1) * speedWalk * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate( new Vector3(0, 0, -1) * speedWalk * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate( new Vector3(-1, 0, 0) * speedWalk * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate( new Vector3(1, 0, 0) * speedWalk * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speedWalk = speedRun;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speedWalk = 5f;
        }

        velocidadActual = (transform.position - ultimaPosicion).magnitude / Time.deltaTime;

        ultimaPosicion = transform.position;

        Debug.Log("Velocidad por Transform: " + velocidadActual + " m/s");
    }
  
}
