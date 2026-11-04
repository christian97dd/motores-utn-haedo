using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Relato del principio: en negro suena la voz con subtitulos y en los momentos marcados aparecen
// imagenes. El mundo queda congelado y el resto del audio en pausa hasta que termina o se saltea.
// Los tiempos se toman de la posicion de la voz, asi los subtitulos no se corren aunque el juego trabe.
public class IntroNarration : MonoBehaviour
{
    [Serializable]
    private class Subtitle
    {
        public float start;
        public float end;
        [TextArea] public string text;
    }

    // se ve desde su tiempo hasta el de la siguiente; sin imagen queda en negro
    [Serializable]
    private class Panel
    {
        public float time;
        public Sprite image;
    }

    // la intro del jugador, la musica y la pausa lo consultan para esperar a que termine
    public static bool IsPlaying { get; private set; }

    [Header("Voz")]
    [SerializeField] private AudioSource voice; // 2D, sale por el grupo Voice

    [Header("Pantalla")]
    [SerializeField] private CanvasGroup screen;
    [SerializeField] private Image panelImage;
    [SerializeField] private TMP_Text subtitleLabel;
    [SerializeField] private float panelFadeTime = 0.8f;
    [SerializeField] private float endFadeTime = 1.5f; // el negro se abre mientras el personaje se despierta

    [Header("Guion (segundos de la voz)")]
    [SerializeField] private Subtitle[] subtitles;
    [SerializeField] private Panel[] panels;

    [SerializeField] private InputAction skipAction = new InputAction("Saltear", InputActionType.Button, "<Keyboard>/space");

    private int currentPanel = -1;

    private void Awake()
    {
        // al reaparecer desde un checkpoint no se repite el relato
        IsPlaying = !Checkpoint.HasCheckpoint && voice != null && voice.clip != null;
        screen.gameObject.SetActive(IsPlaying);
    }

    private void Start()
    {
        if (IsPlaying) StartCoroutine(PlayRoutine());
    }

    private void OnDestroy()
    {
        if (!IsPlaying) return;

        IsPlaying = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        skipAction.Disable();
    }

    private IEnumerator PlayRoutine()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        voice.ignoreListenerPause = true;
        voice.Play();
        skipAction.Enable();

        screen.alpha = 1f;
        SetPanelAlpha(0f);
        subtitleLabel.text = string.Empty;

        // un cuadro de espera: el mismo frame de Play el AudioSource todavia no figura sonando
        yield return null;

        while (voice.isPlaying && !skipAction.WasPressedThisFrame())
        {
            float time = voice.time;
            subtitleLabel.text = GetSubtitle(time);
            UpdatePanel(time);
            yield return null;
        }

        voice.Stop();
        skipAction.Disable();
        subtitleLabel.text = string.Empty;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsPlaying = false;

        for (float elapsed = 0f; elapsed < endFadeTime; elapsed += Time.unscaledDeltaTime)
        {
            screen.alpha = 1f - elapsed / endFadeTime;
            yield return null;
        }

        screen.gameObject.SetActive(false);
    }

    private string GetSubtitle(float time)
    {
        foreach (Subtitle subtitle in subtitles)
        {
            if (time >= subtitle.start && time < subtitle.end) return subtitle.text;
        }

        return string.Empty;
    }

    private void UpdatePanel(float time)
    {
        int index = -1;
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].time <= time) index = i;
        }

        if (index != currentPanel)
        {
            currentPanel = index;
            Sprite image = index >= 0 ? panels[index].image : null;
            panelImage.sprite = image;
            panelImage.enabled = image != null;
            SetPanelAlpha(0f);
        }

        if (panelImage.enabled && panelImage.color.a < 1f)
        {
            SetPanelAlpha(panelImage.color.a + Time.unscaledDeltaTime / panelFadeTime);
        }
    }

    private void SetPanelAlpha(float alpha)
    {
        Color color = panelImage.color;
        color.a = Mathf.Clamp01(alpha);
        panelImage.color = color;
    }
}