using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class GlitchNoise : MonoBehaviour
{
    [SerializeField] private GlitchController controller;

    [Header("Barras")]
    [Min(1)]
    [SerializeField] private int barCount = 28;
    [SerializeField] private Color[] palette =
    {
        new Color(0f, 0.9f, 1f),
        new Color(1f, 0.15f, 0.25f),
        new Color(1f, 1f, 1f)
    };
    [SerializeField] private Vector2 barHeightRange = new Vector2(2f, 14f);
    [SerializeField] private Vector2 alphaRange = new Vector2(0.1f, 0.45f);
    [Range(0f, 1f)]
    [SerializeField] private float verticalLineChance = 0.06f;

    [SerializeField] private float refreshInterval = 0.05f;

    private RectTransform area;
    private Image[] bars;
    private float nextRefresh;

    private void Awake()
    {
        area = GetComponent<RectTransform>();
        bars = new Image[barCount];

        for (int i = 0; i < barCount; i++)
        {
            var go = new GameObject("NoiseBar" + i, typeof(RectTransform), typeof(Image));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);

            var bar = go.GetComponent<Image>();
            bar.raycastTarget = false;
            bar.rectTransform.anchorMin = bar.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            go.SetActive(false);
            bars[i] = bar;
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + refreshInterval;

        float intensity = controller != null ? controller.Intensity : 0f;
        int visible = Mathf.RoundToInt(barCount * intensity);
        Vector2 size = area.rect.size;

        for (int i = 0; i < bars.Length; i++)
        {
            bool active = i < visible;
            bars[i].gameObject.SetActive(active);
            if (!active) continue;

            RectTransform rect = bars[i].rectTransform;

            if (Random.value < verticalLineChance)
            {
                rect.sizeDelta = new Vector2(Random.Range(2f, 6f), size.y * Random.Range(0.3f, 1f));
            }
            else
            {
                rect.sizeDelta = new Vector2(size.x * Random.Range(0.05f, 0.5f), Random.Range(barHeightRange.x, barHeightRange.y));
            }

            rect.anchoredPosition = new Vector2(Random.Range(-size.x, size.x) * 0.5f, Random.Range(-size.y, size.y) * 0.5f);

            Color color = palette[Random.Range(0, palette.Length)];
            color.a = Random.Range(alphaRange.x, alphaRange.y);
            bars[i].color = color;
        }
    }
}
