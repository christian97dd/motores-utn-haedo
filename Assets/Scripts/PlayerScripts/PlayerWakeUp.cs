using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// Intro del nivel: el jugador arranca acostado en la camilla, se levanta y recien ahi toma el control.
// Las animaciones de Mixamo se mueven dentro del modelo sin desplazar al jugador, asi que la bajada
// de la camilla se hace moviendo al jugador entre los puntos mientras corre la animacion.
public class PlayerWakeUp : MonoBehaviour
{
    // la pausa y la camara lo consultan para no reaccionar mientras dura la intro
    public static bool IsPlaying { get; private set; }

    [Header("Jugador")]
    [SerializeField] private Player player;
    [SerializeField] private CinemachineOrbitalFollow playerOrbit; // se gira detras del jugador antes de pasarle la camara

    [Header("Camilla")]
    [SerializeField] private Transform lyingPoint; // acostado: la cabeza queda hacia adelante del punto
    [SerializeField] private Transform seatPoint;  // sentado en el borde, de donde arranca a levantarse
    [SerializeField] private Transform standPoint; // parado en el piso al pie de la camilla

    [Header("Camara")]
    [SerializeField] private CinemachineCamera wakeUpCamera;
    [SerializeField] private float cameraHoldTime = 1.2f; // segundos de la animacion de levantarse que se ven de cerca
    [SerializeField] private float cameraBlendTime = 2f;

    [Header("Animaciones")]
    [SerializeField] private string lyingState = "laying idle";
    [SerializeField] private string getUpState = "getting up";
    [SerializeField] private float lyingTime = 3f;
    [SerializeField] private float crossFadeTime = 0.5f;
    [SerializeField] private float getUpLength = 2.7f;

    // tramo de la animacion en que recoge las piernas: ahi baja de la camilla, antes de apoyar los pies
    [SerializeField] private float dropStart = 0.4f;
    [SerializeField] private float dropEnd = 1.1f;

    private Animator animator;
    private CharacterController characterController;

    private void Start()
    {
        animator = player.GetComponentInChildren<Animator>();
        characterController = player.GetComponent<CharacterController>();

        // al reaparecer desde un checkpoint no se repite la intro de la camilla
        if (Checkpoint.HasCheckpoint)
        {
            wakeUpCamera.gameObject.SetActive(false);
            return;
        }

        StartCoroutine(WakeUpRoutine());
    }

    private void OnDestroy()
    {
        IsPlaying = false;
    }

    private IEnumerator WakeUpRoutine()
    {
        IsPlaying = true;
        player.enabled = false;
        characterController.enabled = false;
        player.transform.SetPositionAndRotation(lyingPoint.position, lyingPoint.rotation);

        animator.Play(lyingState, 0, 0f);

        CinemachineBrain brain = CinemachineCore.FindPotentialTargetBrain(wakeUpCamera);
        CinemachineBlendDefinition previousBlend = brain != null ? brain.DefaultBlend : default;
        if (brain != null) brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
        wakeUpCamera.gameObject.SetActive(true);
        wakeUpCamera.Prioritize();

        yield return new WaitForSeconds(lyingTime);

        animator.CrossFadeInFixedTime(getUpState, crossFadeTime, 0, 0f);

        // las dos animaciones tienen el cuerpo a distinta altura: durante el cruce el jugador
        // pasa al punto sentado para que el cuerpo no salte
        float elapsed = 0f;
        while (elapsed < crossFadeTime)
        {
            float t = elapsed / crossFadeTime;
            player.transform.position = Vector3.Lerp(lyingPoint.position, seatPoint.position, t);
            player.transform.rotation = Quaternion.Slerp(lyingPoint.rotation, seatPoint.rotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = crossFadeTime;
        while (elapsed < getUpLength)
        {
            if (wakeUpCamera.gameObject.activeSelf && elapsed >= cameraHoldTime) ReleaseCamera(brain);

            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(dropStart, dropEnd, elapsed));
            player.transform.position = Vector3.Lerp(seatPoint.position, standPoint.position, t);
            player.transform.rotation = Quaternion.Slerp(seatPoint.rotation, standPoint.rotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (wakeUpCamera.gameObject.activeSelf) ReleaseCamera(brain);
        player.transform.SetPositionAndRotation(standPoint.position, standPoint.rotation);
        characterController.enabled = true;
        player.enabled = true;
        IsPlaying = false;

        // la mezcla original vuelve cuando termino el paso a la camara del jugador
        yield return new WaitForSeconds(cameraBlendTime);
        if (brain != null) brain.DefaultBlend = previousBlend;
    }

    private void ReleaseCamera(CinemachineBrain brain)
    {
        // la camara del jugador ya espera detras de donde va a quedar parado
        if (playerOrbit != null) playerOrbit.HorizontalAxis.Value = standPoint.eulerAngles.y;
        if (brain != null) brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, cameraBlendTime);
        wakeUpCamera.gameObject.SetActive(false);
    }
}