using UnityEngine.SceneManagement;

public static class GameFlow
{
    public const string GameScene = "SampleScene";
    public const string VictoryScene = "Victoria";
    public const string DefeatScene = "Derrota";

    public static void Victory()
    {
        SceneManager.LoadScene(VictoryScene);
    }

    public static void Defeat()
    {
        SceneManager.LoadScene(DefeatScene);
    }

    public static void PlayAgain()
    {
        SceneManager.LoadScene(GameScene);
    }
}
