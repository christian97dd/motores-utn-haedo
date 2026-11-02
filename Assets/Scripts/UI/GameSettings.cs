using UnityEngine;

// opciones del jugador guardadas en PlayerPrefs, asi se mantienen entre partidas
public static class GameSettings
{
    private const string MasterKey = "settings.master";
    private const string MusicKey = "settings.music";
    private const string SfxKey = "settings.sfx";
    private const string AmbienceKey = "settings.ambience";
    private const string VoiceKey = "settings.voice";
    private const string FullscreenKey = "settings.fullscreen";

    public static float Master { get; set; }
    public static float Music { get; set; }
    public static float Sfx { get; set; }
    public static float Ambience { get; set; }
    public static float Voice { get; set; }
    public static bool Fullscreen { get; set; }

    public static void Load()
    {
        Master = PlayerPrefs.GetFloat(MasterKey, 1f);
        Music = PlayerPrefs.GetFloat(MusicKey, 0.8f);
        Sfx = PlayerPrefs.GetFloat(SfxKey, 1f);
        Ambience = PlayerPrefs.GetFloat(AmbienceKey, 0.8f);
        Voice = PlayerPrefs.GetFloat(VoiceKey, 1f);
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(MasterKey, Master);
        PlayerPrefs.SetFloat(MusicKey, Music);
        PlayerPrefs.SetFloat(SfxKey, Sfx);
        PlayerPrefs.SetFloat(AmbienceKey, Ambience);
        PlayerPrefs.SetFloat(VoiceKey, Voice);
        PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
