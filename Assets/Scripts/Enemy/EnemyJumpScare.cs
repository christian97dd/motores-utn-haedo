using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Cuando el androide atrapa al jugador: corte a una camara frente a su cara, grito,
// sacudida y fundido a negro. Recien al terminar se carga la escena de derrota.
public class EnemyJumpScare : MonoBehaviour
{
    // el menu de pausa lo consulta para no abrirse en medio del susto
    public static bool IsPlaying { get; private set; }

    [Header("Camara")]
    [SerializeField] private CinemachineCamera scareCamera; // hija del enemigo, mirando a la cabeza
    [SerializeField] private CinemachineImpulseSource impulseSource; // la sacudida, la escucha el listener de scareCamera

    [Header("Efectos")]
    [SerializeField] private Light faceLight; // apagada hasta el susto, con la luz del nivel la cara casi no se ve
    [SerializeField] private Image screenOverlay; // flash al empezar y fundido a negro al final
    [SerializeField] private AudioSource screamSource;
    [SerializeField] private AudioClip screamClip;
    [SerializeField] private string scareTrigger = "scare";

    [Header("Tiempos")]
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float scareDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Colores")]
    [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color tintColor = new Color(0.6f, 0f, 0f, 0.35f);

    private bool started;

    private void Awake()
    {
        scareCamera.gameObject.SetActive(false);
        if (faceLight != null) faceLight.enabled = false;
        if (screenOverlay != null) screenOverlay.color = Color.clear;
    }

    private void OnDestroy()
    {
        if (started) IsPlaying = false;
    }

    public void Play(GameObject player)
    {
        if (IsPlaying) return;
        IsPlaying = true;
        started = true;

        InputSystem.actions.FindActionMap("Player").Disable();

        // el jugador queda entre la camara y la cara, se oculta para que no tape
        foreach (Renderer r in player.GetComponentsInChildren<Renderer>()) r.enabled = false;

        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(toPlayer);

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null && HasTrigger(animator, scareTrigger)) animator.SetTrigger(scareTrigger);

        // corte seco, con la mezcla por defecto de la camara se veria un paneo
        CinemachineBrain brain = CinemachineCore.FindPotentialTargetBrain(scareCamera);
        if (brain != null) brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
        scareCamera.gameObject.SetActive(true);
        scareCamera.Prioritize();

        if (faceLight != null) faceLight.enabled = true;
        if (screamSource != null && screamClip != null) screamSource.PlayOneShot(screamClip);
        if (impulseSource != null) impulseSource.GenerateImpulse();

        StartCoroutine(ScareRoutine());
    }

    private IEnumerator ScareRoutine()
    {
        float elapsed = 0f;

        while (elapsed < scareDuration)
        {
            if (screenOverlay != null)
            {
                screenOverlay.color = elapsed < flashDuration
                    ? Color.Lerp(flashColor, tintColor, elapsed / flashDuration)
                    : Color.Lerp(tintColor, Color.clear, (elapsed - flashDuration) / (scareDuration - flashDuration));
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (screenOverlay != null) screenOverlay.color = new Color(0f, 0f, 0f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        GameFlow.Defeat(DefeatCause.Android);
    }

    private static bool HasTrigger(Animator animator, string trigger)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == trigger) return true;
        }
        return false;
    }
}
