using Unity.Cinemachine;
using UnityEngine;

// La camara lee el mouse por su cuenta (no usa el mapa Player), asi que con un puzzle
// o una ficha abierta hay que cortarle el input aparte para que no gire.
[RequireComponent(typeof(CinemachineInputAxisController))]
public class CameraInputLock : MonoBehaviour
{
    private CinemachineInputAxisController axisController;

    private void Awake()
    {
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void Update()
    {
        axisController.enabled = !PuzzleWindow.IsAnyOpen && !DocumentWindow.IsOpen;
    }
}
