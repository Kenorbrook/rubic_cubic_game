using UnityEngine;

public class SaveManager
{
    public SaveManager()
    {
        if (_gameSaves == null)
        {
            string savedData = PlayerPrefs.GetString("ActiveGame", null);
            if (!string.IsNullOrEmpty(savedData))
            {
                _gameSaves = JsonUtility.FromJson<GameSaves>(savedData);
            }
        }
    }

    
    private static GameSaves _gameSaves;
    
    public static GameSaves GetGameSaves()
    {
        return _gameSaves;
    }

    public void ChangeGameSaves(int gameSaves)
    {
        _gameSaves.ChangeCharacter(gameSaves);
        PlayerPrefs.SetString("ActiveGame", JsonUtility.ToJson(_gameSaves));
    }

    public void CreateGameSaves(int character)
    {
        _gameSaves = new GameSaves(character);
        PlayerPrefs.SetString("ActiveGame", JsonUtility.ToJson(_gameSaves));
    }
}