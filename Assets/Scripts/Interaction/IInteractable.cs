
/// <summary>
/// Contrato para cualquier objeto con el que el jugador pueda interactuar
/// (puertas, tarjetas, interruptores, paneles, etc). El PlayerInteractor
/// detecta objetos que implementen esta interfaz y los maneja a todos igual,
/// sin necesitar saber de qué tipo específico se trata.
/// </summary>

public interface IInteractable
{
    /// Texto que se muestra en el hint cuando el jugador está en rango.
    /// Puede cambiar según el estado interno del objeto (ej: puerta
    /// bloqueada vs desbloqueada).
    string GetInteractionPrompt();

    /// Si es false, el jugador puede ver el prompt pero no ejecutar Interact().
    bool CanInteract();

    /// Lógica que corre al apretar la tecla de interacción.
    void Interact();
}