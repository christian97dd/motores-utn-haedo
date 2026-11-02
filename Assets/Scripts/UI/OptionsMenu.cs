using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Vistas")]
    [SerializeField] private GameObject menuView;
    [SerializeField] private GameObject optionsView;
    [SerializeField] private Selectable optionsButton; // vuelve a quedar seleccionado al cerrar
    [SerializeField] private Selectable firstOption;

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;
    // nombres de los parametros expuestos en MainMixer
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam = "SFXVolume";
    [SerializeField] private string ambienceParam = "AmbienceVolume";
    [SerializeField] private string voiceParam = "VoiceVolume";
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambienceSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private TMP_Text masterValue;
    [SerializeField] private TMP_Text musicValue;
    [SerializeField] private TMP_Text sfxValue;
    [SerializeField] private TMP_Text ambienceValue;
    [SerializeField] private TMP_Text voiceValue;

    [Header("Video")]
    [SerializeField] private Toggle fullscreenToggle;

    public bool IsOpen => optionsView.activeSelf;

    private void Awake()
    {
        GameSettings.Load();

        masterSlider.onValueChanged.AddListener(value => { GameSettings.Master = value; ApplyAudio(); });
        musicSlider.onValueChanged.AddListener(value => { GameSettings.Music = value; ApplyAudio(); });
        sfxSlider.onValueChanged.AddListener(value => { GameSettings.Sfx = value; ApplyAudio(); });
        ambienceSlider.onValueChanged.AddListener(value => { GameSettings.Ambience = value; ApplyAudio(); });
        voiceSlider.onValueChanged.AddListener(value => { GameSettings.Voice = value; ApplyAudio(); });
        fullscreenToggle.onValueChanged.AddListener(value => { GameSettings.Fullscreen = value; Screen.fullScreen = value; });

        optionsView.SetActive(false);
    }

    // SetFloat del mixer no funciona en el Awake, por eso se aplica aca
    private void Start()
    {
        ApplyAudio();
    }

    // los usan los botones Opciones y Volver en el OnClick
    public void Open()
    {
        masterSlider.SetValueWithoutNotify(GameSettings.Master);
        musicSlider.SetValueWithoutNotify(GameSettings.Music);
        sfxSlider.SetValueWithoutNotify(GameSettings.Sfx);
        ambienceSlider.SetValueWithoutNotify(GameSettings.Ambience);
        voiceSlider.SetValueWithoutNotify(GameSettings.Voice);
        fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        UpdateLabels();

        menuView.SetActive(false);
        optionsView.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstOption.gameObject);
    }

    public void Close()
    {
        GameSettings.Save();

        optionsView.SetActive(false);
        menuView.SetActive(true);
        EventSystem.current.SetSelectedGameObject(optionsButton.gameObject);
    }

    private void ApplyAudio()
    {
        // el general va por el listener porque Master no esta expuesto en el mixer
        AudioListener.volume = GameSettings.Master;

        if (mixer != null)
        {
            mixer.SetFloat(musicParam, ToDecibels(GameSettings.Music));
            mixer.SetFloat(sfxParam, ToDecibels(GameSettings.Sfx));
            mixer.SetFloat(ambienceParam, ToDecibels(GameSettings.Ambience));
            mixer.SetFloat(voiceParam, ToDecibels(GameSettings.Voice));
        }

        UpdateLabels();
    }

    private void UpdateLabels()
    {
        masterValue.text = ToPercent(GameSettings.Master);
        musicValue.text = ToPercent(GameSettings.Music);
        sfxValue.text = ToPercent(GameSettings.Sfx);
        ambienceValue.text = ToPercent(GameSettings.Ambience);
        voiceValue.text = ToPercent(GameSettings.Voice);
    }

    // el slider va de 0 a 1 pero el mixer trabaja en decibeles
    private static float ToDecibels(float value)
    {
        return value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
    }

    private static string ToPercent(float value)
    {
        return Mathf.RoundToInt(value * 100f) + "%";
    }
}
