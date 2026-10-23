using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>

/// Arma el código que escribe el jugador y lo envía al BinaryPanel.
/// IMPORTANTE: poner este script en un GameObject siempre activo (por ejemplo "KeypadController"),
/// NO en el Canvas, porque el Canvas está oculto hasta que se abre el teclado.
/// </summary>
public class BinaryKeypadUI : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private BinaryPanel panel;

    [Header("Interfaz")]
    [Tooltip("El objeto que se muestra y se oculta (normalmente el Canvas)")]
    [SerializeField] private GameObject keypadRoot;
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private Button zeroButton;
    [SerializeField] private Button oneButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button confirmButton;

    [Header("Mientras el teclado está abierto")]
    [Tooltip("Scripts de movimiento del jugador, de la cámara y PlayerInteraction que se apagan mientras se escribe")]
    [SerializeField] private Behaviour[] scriptsToDisable;

    // El menú de pausa lo consulta para no abrirse con el mismo Esc que cierra el teclado
    public static bool IsAnyOpen { get; private set; }

    private string currentCode = string.Empty;
    private bool isOpen;
    private bool isReady;
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (panel == null || panel.Data == null || keypadRoot == null || codeDisplay == null ||
            zeroButton == null || oneButton == null || deleteButton == null || confirmButton == null)
        {
            Debug.LogError("BinaryKeypadUI: missing references in " + gameObject.name, this);
            return;
        }

        isReady = true;
        keypadRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (!isReady) return;

        zeroButton.onClick.AddListener(PressZero);
        oneButton.onClick.AddListener(PressOne);
        deleteButton.onClick.AddListener(PressDelete);
        confirmButton.onClick.AddListener(PressConfirm);
    }

    private void OnDisable()
    {
        if (!isReady) return;

        zeroButton.onClick.RemoveListener(PressZero);
        oneButton.onClick.RemoveListener(PressOne);
        deleteButton.onClick.RemoveListener(PressDelete);
        confirmButton.onClick.RemoveListener(PressConfirm);
    }

    // Si te detectan con el teclado abierto se cambia de escena sin pasar por Close()
    private void OnDestroy()
    {
        if (isOpen) IsAnyOpen = false;
    }

    private void Update()
    {
        // Escape cierra el teclado sin enviar nada
        if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    // Lo llama BinaryKeypadInteractable cuando el jugador interactúa con el panel de la pared
    [ContextMenu("Debug/Open keypad")]
    public void Open()
    {
        if (!isReady || isOpen || panel.IsSolved) return;

        isOpen = true;
        IsAnyOpen = true;
        currentCode = string.Empty;
        RefreshDisplay();
        keypadRoot.SetActive(true);

        // El jugador necesita el mouse para hacer clic en los botones
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetPlayerControl(false);
    }

    public void Close()
    {
        if (!isReady || !isOpen) return;

        isOpen = false;
        IsAnyOpen = false;
        keypadRoot.SetActive(false);

        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;

        SetPlayerControl(true);
    }

    private void PressZero()
    {
        AddDigit('0');
    }

    private void PressOne()
    {
        AddDigit('1');
    }

    private void AddDigit(char digit)
    {
        // No se pueden escribir más dígitos que bits tiene el puzzle
        if (currentCode.Length >= panel.Data.BitCount) return;

        currentCode += digit;
        RefreshDisplay();
    }

    private void PressDelete()
    {
        if (currentCode.Length == 0) return;

        currentCode = currentCode.Substring(0, currentCode.Length - 1);
        RefreshDisplay();
    }

    private void PressConfirm()
    {
        if (currentCode.Length == 0) return;

        panel.SubmitBinaryString(currentCode);

        if (panel.IsSolved)
        {
            Close();
        }
        else
        {
            // Código incorrecto: empezar de nuevo
            currentCode = string.Empty;
            RefreshDisplay();
        }
    }

    // Muestra los dígitos escritos y un guion por cada casillero vacío, por ejemplo "0 1 0 - - - - -"
    private void RefreshDisplay()
    {
        int slots = panel.Data.BitCount;
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < slots; i++)
        {
            builder.Append(i < currentCode.Length ? currentCode[i] : '-');

            if (i < slots - 1)
            {
                builder.Append(' ');
            }
        }

        codeDisplay.text = builder.ToString();
    }

    private void SetPlayerControl(bool state)
    {
        foreach (Behaviour script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = state;
            }
        }
    }
}