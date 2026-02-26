using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character")]
public class Character : ScriptableObject
{
    public Sprite Sprite;
    [Min(1)] public int BaseHp = 100;
    [Min(1)] public int BaseAttack = 10;
}

public enum PlayerClass
{
    Warrior = 0,
    Mage = 1,
    Rogue = 2
}
