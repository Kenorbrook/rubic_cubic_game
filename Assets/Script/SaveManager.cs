using UnityEngine;

public interface IGameSaveService
{
    GameSaves Load();
    GameSaves Create(int character);
    GameSaves UpdateCharacter(int character);
}

public class SaveManager : IGameSaveService
{
    private const string ActiveGameKey = "ActiveGame";
    private GameSaves _gameSaves;

    public GameSaves Load()
    {
        if (_gameSaves != null)
            return _gameSaves;

        var savedData = PlayerPrefs.GetString(ActiveGameKey, null);
        if (string.IsNullOrEmpty(savedData))
            return null;

        _gameSaves = JsonUtility.FromJson<GameSaves>(savedData);
        return _gameSaves;
    }

    public GameSaves Create(int character)
    {
        _gameSaves = new GameSaves(character);
        Persist();
        return _gameSaves;
    }

    public GameSaves UpdateCharacter(int character)
    {
        if (_gameSaves == null)
            Load();

        if (_gameSaves == null)
            return Create(character);

        _gameSaves.ChangeCharacter(character);
        Persist();
        return _gameSaves;
    }

    private void Persist()
    {
        PlayerPrefs.SetString(ActiveGameKey, JsonUtility.ToJson(_gameSaves));
        PlayerPrefs.Save();
    }
}
