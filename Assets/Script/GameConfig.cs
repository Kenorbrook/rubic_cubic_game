using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [FormerlySerializedAs("Character")]
    [SerializeField] private PlayerCharacter[] _playerCharacters;
    [FormerlySerializedAs("enemyCharacter")]
    [SerializeField] private EnemyCharacter[] _enemyCharacters;
    [SerializeField] private CombatConfig _combatConfig;

    public CombatConfig CombatConfig => _combatConfig;

    public PlayerCharacter GetPlayerCharacter(int index)
    {
        if (_playerCharacters == null || _playerCharacters.Length == 0)
            return null;

        var safeIndex = Mathf.Clamp(index, 0, _playerCharacters.Length - 1);
        var character = _playerCharacters[safeIndex];
        return character == null ? null : character;
    }

    public EnemyCharacter GetRandomEnemy()
    {
        if (_enemyCharacters == null || _enemyCharacters.Length == 0)
            return null;

        return _enemyCharacters[Random.Range(0, _enemyCharacters.Length)];
    }
}
