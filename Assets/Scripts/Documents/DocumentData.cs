using System;
using UnityEngine;

public enum DocumentFieldStyle
{
    Normal,
    Warning,
    Danger
}

[Serializable]
public class DocumentField
{
    public string label;
    public string value;
    public DocumentFieldStyle style;
}

// un asset por ficha (Create > Protocol HIDE > Document)
// {codigo} se reemplaza por el valor del puzzle
[CreateAssetMenu(fileName = "NewDocument", menuName = "Protocol HIDE/Document")]
public class DocumentData : ScriptableObject
{
    [Header("Encabezado")]
    [SerializeField] private string title = "FICHA DE SUJETO";
    [SerializeField] private string subtitle = "PROYECTO DE TRANSFORMACIÓN // ACCESO NIVEL 3";
    [SerializeField] private string terminalLabel = "> ARCHIVO MÉDICO · LABORATORIO B-2";
    [SerializeField] private string headline = "SUJETO N.º {codigo}";
    [Tooltip("Vacío para no mostrar sello")]
    [SerializeField] private string stamp = "CONFIDENCIAL";

    [Header("Foto (opcional)")]
    [SerializeField] private Sprite photo;
    [SerializeField] private string photoCaption = "";

    [Header("Datos")]
    [SerializeField] private DocumentField[] fields = new DocumentField[0];

    [Header("Nota")]
    [SerializeField] private string noteAuthor = "";
    [TextArea(3, 8)]
    [SerializeField] private string noteBody = "";

    [Header("Pista")]
    [Tooltip("Si tiene texto, la primera vez que se lee aparece el aviso de NUEVA PISTA")]
    [SerializeField] private string clueText = "";
    [Tooltip("Puzzle del que sale el valor de {codigo}")]
    [SerializeField] private BinaryPuzzleData codeSource;

    public string Title => Format(title);
    public string Subtitle => Format(subtitle);
    public string TerminalLabel => Format(terminalLabel);
    public string Headline => Format(headline);
    public string Stamp => stamp;
    public Sprite Photo => photo;
    public string PhotoCaption => Format(photoCaption);
    public DocumentField[] Fields => fields;
    public string NoteAuthor => Format(noteAuthor);
    public string NoteBody => Format(noteBody);
    public string ClueText => Format(clueText);

    public string Format(string text)
    {
        if (string.IsNullOrEmpty(text) || codeSource == null) return text;
        return text.Replace("{codigo}", codeSource.TargetValue.ToString());
    }
}
