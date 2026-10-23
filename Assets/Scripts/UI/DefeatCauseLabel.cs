using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DefeatCauseLabel : MonoBehaviour
{
    [Header("Textos por causa")]
    [SerializeField] private string securityCameraText = "Detectado por cámara de seguridad";
    [SerializeField] private string androidText = "Capturado por un androide de seguridad";
    [SerializeField] private string unknownText = "Detectado por el sistema de seguridad"; // por si se abre la escena sola

    private void Start()
    {
        GetComponent<TMP_Text>().text = GameFlow.LastDefeatCause switch
        {
            DefeatCause.SecurityCamera => securityCameraText,
            DefeatCause.Android => androidText,
            _ => unknownText
        };
    }
}
