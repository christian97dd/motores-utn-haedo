using UnityEngine;
using TMPro;
using System.Collections;
/// <summary>
/// Singleton que controla el único texto de pista en pantalla (abajo,
/// mínimo y diegético según el diseño del juego). Cualquier interactuable
/// llama ShowHint()/HideHint() sin necesitar su propio texto o lógica de fade.
/// </summary>
public class HintTextController : MonoBehaviour
{
    public static HintTextController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Arranca invisible y sin texto — cada interactuable decide cuándo
        // mostrar algo, este script no asume ningún estado inicial propio.
        canvasGroup.alpha = 0f;
        hintText.text = string.Empty;
    }

    public void ShowHint(string message)
    {
        hintText.text = message;
        StartFade(1f);
    }

    public void HideHint()
    {
        StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        // Si ya hay un fade corriendo (por ejemplo, el jugador entra y sale
        // de rango rápido), lo cortamos antes de arrancar uno nuevo. Sin
        // esto, dos coroutines de fade compitiendo entre sí dejan el alpha
        // en un valor inconsistente o parpadeando.

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvasGroup(targetAlpha));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        // Parte del alpha actual, no de 0 o 1 fijo, para que un fade
        // interrumpido a mitad de camino continúe suave desde donde iba,
        // en vez de saltar bruscamente.

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}