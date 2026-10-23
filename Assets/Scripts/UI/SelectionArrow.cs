using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// la flecha se para al lado del boton seleccionado, y se esconde si lo seleccionado no es de su lista
[RequireComponent(typeof(Graphic))]
public class SelectionArrow : MonoBehaviour
{
    [SerializeField] private Transform[] buttons;

    private Graphic graphic;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    private void Update()
    {
        if (EventSystem.current == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        bool ours = selected != null && Array.IndexOf(buttons, selected.transform) >= 0;
        graphic.enabled = ours;
        if (!ours) return;

        Vector3 position = transform.position;
        position.y = selected.transform.position.y;
        transform.position = position;
    }
}
