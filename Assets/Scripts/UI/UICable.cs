using UnityEngine;
using UnityEngine.UI;

// Un cable curvo para la UI: sale horizontal de un punto y llega horizontal al otro.
public class UICable : MaskableGraphic
{
    [SerializeField] private float thickness = 10f;
    [SerializeField] private int segments = 32;

    private Vector2 start;
    private Vector2 end;

    public void SetPoints(Vector2 from, Vector2 to)
    {
        start = from;
        end = to;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // curva de bezier con las dos manijas horizontales, como un cable que cuelga
        float handle = Mathf.Max(Mathf.Abs(end.x - start.x) * 0.5f, 40f);
        Vector2 c1 = start + Vector2.right * handle;
        Vector2 c2 = end - Vector2.right * handle;

        Vector2 previous = start;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 point = Bezier(start, c1, c2, end, t);
            Vector2 next = i < segments ? Bezier(start, c1, c2, end, (i + 1) / (float)segments) : point;
            Vector2 direction = (i < segments ? next - point : point - previous).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * thickness * 0.5f;

            vh.AddVert(point + normal, color, Vector2.zero);
            vh.AddVert(point - normal, color, Vector2.zero);

            if (i > 0)
            {
                int v = i * 2;
                vh.AddTriangle(v - 2, v - 1, v);
                vh.AddTriangle(v - 1, v + 1, v);
            }

            previous = point;
        }
    }

    private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        float u = 1f - t;
        return u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
    }
}
