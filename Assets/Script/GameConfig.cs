using UnityEngine;
[CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    public Character[] Character;
    public Character[] enemyCharacter;
    
    public Character GetRandomEnemy()
    {
        return enemyCharacter[Random.Range(0, enemyCharacter.Length)];
    }
}