using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// con el mouse encima el boton queda seleccionado, asi mouse y teclado marcan lo mismo
[RequireComponent(typeof(Selectable))]
public class SelectOnHover : MonoBehaviour, IPointerEnterHandler
{
    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable.IsInteractable() && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
