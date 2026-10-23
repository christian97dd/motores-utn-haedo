using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// las copias se crean al arrancar, asi en la escena queda un solo texto
[RequireComponent(typeof(TMP_Text))]
public class GlitchText : MonoBehaviour
{
    [SerializeField] private GlitchController controller;

    [Header("Separación de color")]
    [SerializeField] private Color leftChannel = new Color(0f, 0.9f, 1f, 0.75f);
    [SerializeField] private Color rightChannel = new Color(1f, 0.1f, 0.3f, 0.75f);
    [SerializeField] private float minChannelOffset = 3f; // en reposo
    [SerializeField] private float maxChannelOffset = 12f;

    [Header("Franjas desplazadas")]
    [Min(1)]
    [SerializeField] private int sliceCount = 5;
    [SerializeField] private float maxSliceOffset = 40f;
    [Range(0f, 1f)]
    [SerializeField] private float sliceThreshold = 0.2f;

    [SerializeField] private float refreshInterval = 0.05f;

    private TMP_Text text;
    private RectTransform leftGhost;
    private RectTransform rightGhost;
    private RectTransform[] sliceMasks;
    private Vector2[] sliceBasePositions;
    private Vector2 basePosition;
    private float nextRefresh;
    private readonly List<TMP_Text> copies = new List<TMP_Text>();

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        basePosition = text.rectTransform.anchoredPosition;

        // los colores van atras del texto
        int index = transform.GetSiblingIndex();
        leftGhost = CreateCopy("GlitchLeft", leftChannel, transform.parent);
        rightGhost = CreateCopy("GlitchRight", rightChannel, transform.parent);
        leftGhost.SetSiblingIndex(index);
        rightGhost.SetSiblingIndex(index);

        CreateSlices();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + refreshInterval;

        // otro script puede cambiar el texto despues del Awake (DefeatCauseLabel lo hace en el Start)
        if (copies[0].text != text.text)
        {
            foreach (TMP_Text copy in copies) copy.text = text.text;
        }

        float intensity = controller != null ? controller.Intensity : 0f;
        float offset = Mathf.Lerp(minChannelOffset, maxChannelOffset, intensity);

        leftGhost.anchoredPosition = basePosition + new Vector2(-offset * Random.Range(0.6f, 1f), Random.Range(-offset, offset) * 0.2f);
        rightGhost.anchoredPosition = basePosition + new Vector2(offset * Random.Range(0.6f, 1f), Random.Range(-offset, offset) * 0.2f);

        bool glitching = intensity >= sliceThreshold;

        for (int i = 0; i < sliceMasks.Length; i++)
        {
            bool active = glitching && Random.value < intensity * 0.4f;
            sliceMasks[i].gameObject.SetActive(active);

            if (active)
            {
                float shift = Random.Range(-maxSliceOffset, maxSliceOffset) * intensity;
                sliceMasks[i].anchoredPosition = sliceBasePositions[i] + new Vector2(shift, 0f);
            }
        }
    }

    // cada franja es una mascara con una copia del texto adentro
    private void CreateSlices()
    {
        RectTransform source = text.rectTransform;
        float height = source.rect.height;
        float bandHeight = height / sliceCount;

        sliceMasks = new RectTransform[sliceCount];
        sliceBasePositions = new Vector2[sliceCount];

        for (int i = 0; i < sliceCount; i++)
        {
            float bandCenter = height * 0.5f - (i + 0.5f) * bandHeight;

            var mask = new GameObject("GlitchSlice" + i, typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
            mask.gameObject.layer = gameObject.layer;
            mask.SetParent(transform.parent, false);
            mask.anchorMin = source.anchorMin;
            mask.anchorMax = source.anchorMax;
            mask.pivot = new Vector2(0.5f, 0.5f);
            mask.sizeDelta = new Vector2(source.rect.width + maxSliceOffset * 2f, bandHeight);
            sliceBasePositions[i] = basePosition + new Vector2(0f, bandCenter);
            mask.anchoredPosition = sliceBasePositions[i];

            RectTransform copy = CreateCopy("Text", text.color, mask);
            copy.anchorMin = copy.anchorMax = new Vector2(0.5f, 0.5f);
            copy.anchoredPosition = new Vector2(0f, -bandCenter);

            mask.gameObject.SetActive(false);
            sliceMasks[i] = mask;
        }
    }

    private RectTransform CreateCopy(string copyName, Color color, Transform parent)
    {
        var go = new GameObject(copyName, typeof(RectTransform));
        go.layer = gameObject.layer;
        go.transform.SetParent(parent, false);

        var copy = go.AddComponent<TextMeshProUGUI>();
        copy.text = text.text;
        copy.font = text.font;
        copy.fontSize = text.fontSize;
        copy.fontStyle = text.fontStyle;
        copy.alignment = text.alignment;
        copy.characterSpacing = text.characterSpacing;
        copy.lineSpacing = text.lineSpacing;
        copy.textWrappingMode = text.textWrappingMode;
        copy.color = color;
        copy.raycastTarget = false;
        copies.Add(copy);
        // sin el glow, si no los colores se ensucian
        copy.fontSharedMaterial = text.font.material;

        RectTransform rect = copy.rectTransform;
        RectTransform source = text.rectTransform;
        rect.anchorMin = source.anchorMin;
        rect.anchorMax = source.anchorMax;
        rect.pivot = source.pivot;
        rect.sizeDelta = source.sizeDelta;
        rect.anchoredPosition = basePosition;
        return rect;
    }
}
