using UnityEngine;

// El contenido de la ventana para un tipo de puzzle (los bits del binario, las ondas de la frecuencia...).
// Cada tipo nuevo agrega su vista como hija del contenido de la PuzzleWindow.
public abstract class PuzzleView : MonoBehaviour
{
    [SerializeField] private Vector2 panelSize = new Vector2(1000f, 740f); // cada puzzle puede pedir una ventana mas grande

    public Vector2 PanelSize => panelSize;

    public abstract bool CanShow(Puzzle puzzle);

    public virtual void Show(Puzzle puzzle)
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    // la ventana avisa cuando pasa a error, resuelto o vuelve a normal
    public virtual void OnStateChanged(PuzzleState state) { }

    // textos que muestra la ventana arriba del contenido segun el estado
    public virtual string Instructions => "";
    public virtual string ErrorMessage => "INCORRECTO";
    public virtual string SolvedMessage => "ACCESO CONCEDIDO";
    public virtual string Controls => "ESC para salir";

    // para los puzzles que cambian el mensaje mientras se juegan (piezas que faltan, cables conectados)
    protected void RefreshMessage()
    {
        if (PuzzleWindow.Current != null) PuzzleWindow.Current.RefreshMessage();
    }
}
