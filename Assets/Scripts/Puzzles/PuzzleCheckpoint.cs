using System;
using UnityEngine;

// va en cada panel binario, al resolverse guarda un checkpoint, y si ese puzzle ya estaba resuelto cuando se carga la escena, lo vuelve a resolver 
 
public class PuzzleCheckpoint : MonoBehaviour
{
    [SerializeField] private BinaryPanel panel;

    [Tooltip("Donde reaparece el jugador (punto vacio frente al panel)")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("Opcional. Si esta vacio se usa la ruta del objeto en el Hierarchy")]
    [SerializeField] private string puzzleId;

    private bool restoring;

    private string Id => string.IsNullOrEmpty(puzzleId) ? GetHierarchyPath() : puzzleId;

    private void Awake()
    {
        if (panel == null) panel = GetComponent<BinaryPanel>();

        if (panel == null)
            Debug.LogError("PuzzleCheckpoint: falta el BinaryPanel en " + gameObject.name, this);
        if (respawnPoint == null)
            Debug.LogError("PuzzleCheckpoint: falta el punto de reaparicion en " + gameObject.name, this);
    }

    private void OnEnable()
    {
        if (panel != null) panel.onSolved.AddListener(SaveCheckpoint);
    }

    private void OnDisable()
    {
        if (panel != null) panel.onSolved.RemoveListener(SaveCheckpoint);
    }

    // start corre despues de todos los Awake, asi la puerta ya sabe donde esta cerrada
    private void Start()
    {
        if (panel == null || panel.Data == null || !Checkpoint.IsSolved(Id)) return;

        // se vuelve a resolver con el codigo correcto: dispara el On Solved (abre la puerta)
        restoring = true;
        string binary = Convert.ToString(panel.Data.TargetValue, 2).PadLeft(panel.Data.BitCount, '0');
        panel.SubmitBinaryString(binary);
        restoring = false;
    }

    private void SaveCheckpoint()
    {
        // al restaurar no se pisa el checkpoint con el de un puzzle anterior
        if (restoring || respawnPoint == null) return;

        Checkpoint.Save(Id, respawnPoint.position, respawnPoint.rotation);
    }

    private string GetHierarchyPath()
    {
        string path = gameObject.name;
        Transform current = transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}