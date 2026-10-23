using UnityEngine.SceneManagement;

public enum DefeatCause
{
    Unknown,
    SecurityCamera,
    Android
}

public static class GameFlow
{
    public const string GameScene = "SampleScene";
    public const string VictoryScene = "Victoria";
    public const string DefeatScene = "Derrota";

    // lo lee la escena de derrota para mostrar el motivo
    public static DefeatCause LastDefeatCause { get; private set; } = DefeatCause.Unknown;

    public static void Victory()
    {
        SceneManager.LoadScene(VictoryScene);
    }

    public static void Defeat(DefeatCause cause)
    {
        LastDefeatCause = cause;
        SceneManager.LoadScene(DefeatScene);
    }

    public static void PlayAgain()
    {
        SceneManager.LoadScene(GameScene);
    }
}
