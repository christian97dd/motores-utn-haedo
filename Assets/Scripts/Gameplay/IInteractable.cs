// lo implementa todo lo que se usa con E (puertas, fichas, puzzles)
public interface IInteractable
{
    void Interact();

    // el texto de la leyenda de abajo
    InteractionPrompt Prompt { get; }
}

public enum InteractionPromptStyle
{
    Normal,
    Locked, // puerta bloqueada
    Done // puzzle ya resuelto
}

public struct InteractionPrompt
{
    public string objectName;
    public string action;
    public InteractionPromptStyle style;

    public InteractionPrompt(string objectName, string action, InteractionPromptStyle style = InteractionPromptStyle.Normal)
    {
        this.objectName = objectName;
        this.action = action;
        this.style = style;
    }
}
