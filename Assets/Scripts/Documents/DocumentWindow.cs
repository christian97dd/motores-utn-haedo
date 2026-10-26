using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ventana de las fichas, se cierra con E o Esc
public class DocumentWindow : MonoBehaviour
{
    public static DocumentWindow Current { get; private set; }

    // lo usa la pausa para no abrirse con el mismo Esc
    public static bool IsOpen { get; private set; }

    [Header("Partes de la ventana")]
    [SerializeField] private GameObject windowRoot;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text subtitleLabel;
    [SerializeField] private TMP_Text terminalLabel;
    [SerializeField] private TMP_Text headlineLabel;
    [SerializeField] private GameObject stampRoot;
    [SerializeField] private TMP_Text stampLabel;
    [SerializeField] private GameObject photoRoot;
    [SerializeField] private Image photoImage;
    [SerializeField] private TMP_Text photoCaption;
    [SerializeField] private RectTransform fieldsArea; // se corre a la izquierda si no hay foto
    [SerializeField] private GameObject fieldTemplate; // se clona uno por dato
    [SerializeField] private TMP_Text noteAuthor;
    [SerializeField] private TMP_Text noteBody;

    [Header("Aviso de pista")]
    [SerializeField] private GameObject clueToast;
    [SerializeField] private TMP_Text clueLabel;
    [SerializeField] private float clueDuration = 4f;

    [Header("Colores de los datos")]
    [SerializeField] private Color normalColor = new Color(0.82f, 0.92f, 0.96f);
    [SerializeField] private Color warningColor = new Color(1f, 0.7f, 0.24f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.35f, 0.39f);

    // los avisos de pista salen una sola vez por partida
    private static readonly HashSet<DocumentData> alreadyRead = new HashSet<DocumentData>();

    private readonly List<GameObject> fieldInstances = new List<GameObject>();
    private InputActionMap playerMap;
    private InputAction closeAction;
    private InputAction cancelAction;
    private Vector2 fieldsWithPhoto;
    private int closedFrame = -1;

    private void Awake()
    {
        Current = this;
        playerMap = InputSystem.actions.FindActionMap("Player");
        closeAction = InputSystem.actions.FindAction("Document/Close");
        cancelAction = InputSystem.actions.FindAction("System/Pause");

        fieldsWithPhoto = fieldsArea.anchoredPosition;
        fieldTemplate.SetActive(false);
        windowRoot.SetActive(false);
        clueToast.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
        if (IsOpen) IsOpen = false;
    }

    public void Open(DocumentData document)
    {
        // el mismo E que cierra la ficha no la tiene que volver a abrir
        if (IsOpen || PuzzleWindow.IsAnyOpen || Time.frameCount == closedFrame) return;

        Fill(document);
        IsOpen = true;
        windowRoot.SetActive(true);

        playerMap.Disable();
        closeAction.Enable();
        closeAction.performed += OnClose;
        cancelAction.performed += OnClose;

        if (!string.IsNullOrEmpty(document.ClueText) && alreadyRead.Add(document))
        {
            StopAllCoroutines();
            StartCoroutine(ClueRoutine(document.ClueText));
        }
    }

    public void Close()
    {
        if (!IsOpen) return;

        closeAction.performed -= OnClose;
        cancelAction.performed -= OnClose;
        closeAction.Disable();
        playerMap.Enable();

        windowRoot.SetActive(false);
        IsOpen = false;
        closedFrame = Time.frameCount;
    }

    private void OnClose(InputAction.CallbackContext context)
    {
        Close();
    }

    private void Fill(DocumentData document)
    {
        titleLabel.text = document.Title;
        subtitleLabel.text = document.Subtitle;
        terminalLabel.text = document.TerminalLabel;
        headlineLabel.text = document.Headline;

        stampRoot.SetActive(!string.IsNullOrEmpty(document.Stamp));
        stampLabel.text = document.Stamp;

        bool hasPhoto = document.Photo != null;
        photoRoot.SetActive(hasPhoto);
        photoImage.sprite = document.Photo;
        photoCaption.text = document.PhotoCaption;
        // sin foto, los datos ocupan el lugar de la foto
        fieldsArea.anchoredPosition = hasPhoto ? fieldsWithPhoto : new Vector2(50f, fieldsWithPhoto.y);

        foreach (GameObject field in fieldInstances) Destroy(field);
        fieldInstances.Clear();

        foreach (DocumentField field in document.Fields)
        {
            GameObject instance = Instantiate(fieldTemplate, fieldTemplate.transform.parent);
            instance.SetActive(true);
            instance.transform.Find("Label").GetComponent<TMP_Text>().text = document.Format(field.label);

            TMP_Text value = instance.transform.Find("Value").GetComponent<TMP_Text>();
            value.text = document.Format(field.value);
            value.color = field.style == DocumentFieldStyle.Danger ? dangerColor
                : field.style == DocumentFieldStyle.Warning ? warningColor
                : normalColor;

            fieldInstances.Add(instance);
        }

        noteAuthor.text = document.NoteAuthor;
        noteAuthor.gameObject.SetActive(!string.IsNullOrEmpty(document.NoteAuthor));
        noteBody.text = document.NoteBody;
    }

    private IEnumerator ClueRoutine(string text)
    {
        clueLabel.text = text;
        clueToast.SetActive(true);
        yield return new WaitForSecondsRealtime(clueDuration);
        clueToast.SetActive(false);
    }
}
