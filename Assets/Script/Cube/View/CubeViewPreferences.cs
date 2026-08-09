using UnityEngine;

public static class CubeViewPreferences
{
    private const string DisplayModeKey = "CubeDisplayMode";
    private const string InvertVerticalKey = "CubeInvertVertical";

    public static bool Use3D
    {
        get => PlayerPrefs.GetInt(DisplayModeKey, 0) == 1;
        set => SetBool(DisplayModeKey, value);
    }

    public static bool InvertVertical
    {
        get => PlayerPrefs.GetInt(InvertVerticalKey, 0) == 1;
        set => SetBool(InvertVerticalKey, value);
    }

    private static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
