using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

// Esc pausa y despausa. Con el juego en pausa se cortan los controles del jugador,
// el tiempo y el audio, y un volumen de post-processing apaga el nivel de fondo
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseView;
    [SerializeField] private Selectable firstButton;
    [SerializeField] private OptionsMenu options;

    [Header("Fondo")]
    [SerializeField] private Volume pauseVolume;
    [SerializeField] private float volumeFadeSpeed = 6f;

    public bool IsPaused { get; private set; }

    private InputAction pauseAction;
    private InputActionMap playerMap;
    private bool cameraHadPostProcessing;

    private void Awake()
    {
        pauseAction = InputSystem.actions.FindAction("System/Pause");
        playerMap = InputSystem.actions.FindActionMap("Player");
        pauseView.SetActive(false);
        if (pauseVolume != null) pauseVolume.weight = 0f;
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPausePressed;
    }

    private void Update()
    {
        if (pauseVolume == null) return;

        float target = IsPaused ? 1f : 0f;
        pauseVolume.weight = Mathf.MoveTowards(pauseVolume.weight, target, volumeFadeSpeed * Time.unscaledDeltaTime);

        // la camara del nivel tiene el post-processing apagado, se prende solo mientras dura la pausa
        if (!IsPaused && pauseVolume.weight <= 0f) SetCameraPostProcessing(cameraHadPostProcessing);
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        // con un puzzle abierto, Esc lo cierra a el y no abre la pausa
        if (PuzzleWindow.IsAnyOpen) return;

        if (!IsPaused)
        {
            Pause();
        }
        else if (options != null && options.IsOpen)
        {
            options.Close(); // con opciones abiertas, Esc vuelve a la pausa y no al juego
        }
        else
        {
            Resume();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        playerMap.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraHadPostProcessing = GetCameraPostProcessing();
        SetCameraPostProcessing(true);

        pauseView.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    // los usan los botones en el OnClick
    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        playerMap.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseView.SetActive(false);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        playerMap.Enable();
        GameFlow.GoToMenu();
    }

    private bool GetCameraPostProcessing()
    {
        Camera cam = Camera.main;
        return cam != null && cam.GetUniversalAdditionalCameraData().renderPostProcessing;
    }

    private void SetCameraPostProcessing(bool value)
    {
        Camera cam = Camera.main;
        if (cam != null) cam.GetUniversalAdditionalCameraData().renderPostProcessing = value;
    }
}
