using Unity.Cinemachine;
using UnityEngine;

// Va en un objeto de la escena. Al cargar, si hay checkpoint, mueve al jugador ahi.
public class CheckpointRespawn : MonoBehaviour
{
    [SerializeField] private Player player;

    [Tooltip("Opcional: se gira detras del jugador, como en la intro")]
    [SerializeField] private CinemachineOrbitalFollow playerOrbit;

    private void Start()
    {
        if (!Checkpoint.HasCheckpoint)
        {
            Debug.Log("CheckpointRespawn: no hay checkpoint, partida normal");
            return;
        }

        if (player == null)
        {
            Debug.LogError("CheckpointRespawn: falta asignar el Player", this);
            return;
        }

        // el CharacterController pisa la posicion si no se lo apaga antes de moverlo
        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.SetPositionAndRotation(Checkpoint.Position, Checkpoint.Rotation);
        controller.enabled = true;

        if (playerOrbit != null) playerOrbit.HorizontalAxis.Value = Checkpoint.Rotation.eulerAngles.y;

        Debug.Log("CheckpointRespawn: jugador movido a " + Checkpoint.Position);
    }
}