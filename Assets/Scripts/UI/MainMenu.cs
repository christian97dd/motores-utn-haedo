using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// carga el nivel en segundo plano para que el menu no se congele al apretar Jugar
public class MainMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup buttons;
    [SerializeField] private GameObject loadingLabel;

    private bool loading;

    private void Awake()
    {
        if (loadingLabel != null) loadingLabel.SetActive(false);
    }

    // lo usa el boton Jugar en el OnClick
    public void Play()
    {
        if (loading) return;
        loading = true;
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        buttons.interactable = false;
        if (loadingLabel != null) loadingLabel.SetActive(true);

        AsyncOperation load = SceneManager.LoadSceneAsync(GameFlow.GameScene);

        while (!load.isDone)
        {
            yield return null;
        }
    }
}
