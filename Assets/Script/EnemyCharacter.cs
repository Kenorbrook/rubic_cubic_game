using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCharacter", menuName = "Character/EnemyCharacter")]
public class EnemyCharacter : Character
{
    [Min(1)] public int AttackFrequencyTurns = 1;
}
