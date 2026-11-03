using TMPro;
using UnityEngine;
using UnityEngine.UI;

// slot de abajo a la izquierda: si se lleva la botella y como tirarla
public class DistractionSlotUI : MonoBehaviour
{
    [Header("Partes")]
    [SerializeField] private Graphic[] frameParts; // esquinas y recuadro del icono
    [SerializeField] private GameObject fullIcon;
    [SerializeField] private GameObject emptyIcon;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text countLabel;
    [SerializeField] private GameObject keyHint;
    [SerializeField] private TMP_Text hintLabel;
    [SerializeField] private TMP_Text emptyLabel;

    [Header("Textos")]
    [SerializeField] private string itemName = "BOTELLA";
    [SerializeField] private string emptyName = "VACÍO";
    [SerializeField] private string holdHint = "MANTENÉ PARA APUNTAR";
    [SerializeField] private string aimHint = "SOLTÁ LANZA · DER. CANCELA";

    [Header("Colores")]
    [SerializeField] private Color activeColor = new Color(0.55f, 0.84f, 0.94f);
    [SerializeField] private Color dimColor = new Color(0.27f, 0.43f, 0.51f);

    private ThrowDistraction holder;

    private void Start()
    {
        holder = ThrowDistraction.Current;
        if (holder != null) holder.Changed += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (holder != null) holder.Changed -= Refresh;
    }

    private void Refresh()
    {
        bool full = holder != null && holder.HasDistraction;
        Color color = full ? activeColor : dimColor;

        foreach (Graphic part in frameParts) part.color = color;
        fullIcon.SetActive(full);
        emptyIcon.SetActive(!full);

        nameLabel.text = full ? itemName : emptyName;
        nameLabel.color = full ? Color.white : dimColor;
        countLabel.text = full ? "1/1" : "0/1";
        countLabel.color = color;

        keyHint.SetActive(full);
        emptyLabel.gameObject.SetActive(!full);
        hintLabel.text = holder != null && holder.IsAiming ? aimHint : holdHint;
    }
}
