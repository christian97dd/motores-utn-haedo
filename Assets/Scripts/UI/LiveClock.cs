using System;
using System.Collections;
using TMPro;
using UnityEngine;

// la hora del visor de camara del menu
[RequireComponent(typeof(TMP_Text))]
public class LiveClock : MonoBehaviour
{
    [SerializeField] private string format = "dd/MM/yyyy  HH:mm:ss";

    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        StartCoroutine(TickRoutine());
    }

    private IEnumerator TickRoutine()
    {
        var wait = new WaitForSecondsRealtime(1f);

        while (true)
        {
            label.text = DateTime.Now.ToString(format);
            yield return wait;
        }
    }
}
