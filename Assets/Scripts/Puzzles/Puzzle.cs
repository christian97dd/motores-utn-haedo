using UnityEngine;
using UnityEngine.Events;

// Base de todos los puzzles. La logica de cada tipo hereda de aca y la PuzzleWindow
// se encarga de mostrarlo, asi un puzzle nuevo solo tiene que resolver si se gano o no.
public abstract class Puzzle : MonoBehaviour
{
    [Header("Terminal")]
    [SerializeField] private string title = "ACCESO RESTRINGIDO";
    [SerializeField] private string subtitle = "PUERTA B-2";
    [SerializeField] private string terminalLabel = "> TERMINAL DE ACCESO 04 · CONEXIÓN ESTABLECIDA";
    [TextArea]
    [SerializeField] private string hint = "> Pista: el código está anotado en algún lugar del laboratorio";

    [Header("Al resolverse o fallar")]
    // se conectan desde el Inspector: abrir puerta, apagar camara, sonido, etc.
    public UnityEvent onSolved;
    public UnityEvent onFailed;

    public bool IsSolved { get; private set; }

    public string Title => title;
    public string Subtitle => subtitle;
    public string TerminalLabel => terminalLabel;
    public string Hint => hint;

    protected void Solve()
    {
        if (IsSolved) return;

        IsSolved = true;
        onSolved.Invoke();
    }

    protected void Fail()
    {
        onFailed.Invoke();
    }
}
