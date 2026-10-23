using System.Collections;
using UnityEngine;

// va en el canvas del menu, la victoria y la derrota
[RequireComponent(typeof(CanvasGroup))]
public class EndScreen : MonoBehaviour
{
    [Header("Aparición")]
    [SerializeField] private float fadeDelay = 0.3f;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // invisible y sin botones hasta que termina el fade
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        // en el juego el cursor queda bloqueado, aca lo libero
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        // realtime por si despues se pausa con timeScale
        yield return new WaitForSecondsRealtime(fadeDelay);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // los usan los botones en el OnClick
    public void PlayAgain()
    {
        GameFlow.PlayAgain();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
