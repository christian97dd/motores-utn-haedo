using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// para la luz de alerta de la derrota
[RequireComponent(typeof(Graphic))]
public class BlinkGraphic : MonoBehaviour
{
    [SerializeField] private float interval = 0.6f;
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.15f; // 0 = se apaga del todo

    private Graphic graphic;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        var wait = new WaitForSecondsRealtime(interval);
        bool on = true;

        while (true)
        {
            graphic.canvasRenderer.SetAlpha(on ? 1f : minAlpha);
            on = !on;
            yield return wait;
        }
    }
}
