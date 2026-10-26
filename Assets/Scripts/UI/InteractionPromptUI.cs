using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// leyenda de abajo al centro, tipo [E] Leer ficha
[RequireComponent(typeof(CanvasGroup))]
public class InteractionPromptUI : MonoBehaviour
{
    [Header("Partes")]
    [SerializeField] private Image frame;
    [SerializeField] private Image keyFrame;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private GameObject nameTag;
    [SerializeField] private TMP_Text nameLabel;

    [Header("Colores")]
    [SerializeField] private Color normalColor = new Color(0.47f, 0.86f, 0.96f);
    [SerializeField] private Color lockedColor = new Color(1f, 0.35f, 0.39f);
    [SerializeField] private Color doneColor = new Color(0.35f, 0.9f, 0.59f);

    [SerializeField] private float fadeSpeed = 10f;

    private CanvasGroup canvasGroup;
    private IInteractable target;
    private InteractionPrompt shown;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // la tecla sale del binding de Interact
        InputAction interact = InputSystem.actions.FindAction("Player/Interact");
        if (interact != null)
        {
            keyLabel.text = interact.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"),
                InputBinding.DisplayStringOptions.DontIncludeInteractions);
        }
    }

    private void OnEnable()
    {
        PlayerInteraction.TargetChanged += OnTargetChanged;
    }

    private void OnDisable()
    {
        PlayerInteraction.TargetChanged -= OnTargetChanged;
    }

    private void OnTargetChanged(IInteractable newTarget)
    {
        target = newTarget;
        if (target != null) Show(target.Prompt);
    }

    private void Update()
    {
        float goal = target != null ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, goal, fadeSpeed * Time.unscaledDeltaTime);

        // por si cambia mientras se mira (puerta que se abre, puzzle resuelto)
        if (target != null && !SamePrompt(target.Prompt, shown)) Show(target.Prompt);
    }

    private void Show(InteractionPrompt prompt)
    {
        shown = prompt;
        actionLabel.text = prompt.action;
        nameLabel.text = prompt.objectName;
        nameTag.SetActive(!string.IsNullOrEmpty(prompt.objectName));

        Color color = prompt.style == InteractionPromptStyle.Locked ? lockedColor
            : prompt.style == InteractionPromptStyle.Done ? doneColor
            : normalColor;
        frame.color = color;
        keyFrame.color = color;
        nameLabel.color = color;
    }

    private static bool SamePrompt(InteractionPrompt a, InteractionPrompt b)
    {
        return a.action == b.action && a.objectName == b.objectName && a.style == b.style;
    }
}
