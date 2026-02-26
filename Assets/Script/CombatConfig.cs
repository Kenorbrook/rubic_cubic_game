using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatConfig", menuName = "Config/CombatConfig")]
public class CombatConfig : ScriptableObject
{
    [SerializeField] private EnemyLevelMultiplier[] _enemyLevelMultipliers = new EnemyLevelMultiplier[100];
    [SerializeField] private PlayerClassModifier[] _classModifiers;

    private void OnEnable()
    {
        EnsureDefaults();
    }

    private void OnValidate()
    {
        EnsureDefaults();
    }

    public EnemyLevelMultiplier GetEnemyMultiplierForLevel(int level)
    {
        EnsureDefaults();
        var safeLevel = Mathf.Clamp(level, 1, 100);
        return _enemyLevelMultipliers[safeLevel - 1];
    }

    public PlayerClassModifier GetClassModifier(PlayerClass playerClass)
    {
        EnsureDefaults();
        for (var i = 0; i < _classModifiers.Length; i++)
        {
            if (_classModifiers[i].PlayerClass == playerClass)
                return _classModifiers[i];
        }

        return PlayerClassModifier.Default(playerClass);
    }

    private void EnsureDefaults()
    {
        if (_enemyLevelMultipliers == null || _enemyLevelMultipliers.Length != 100)
            _enemyLevelMultipliers = new EnemyLevelMultiplier[100];

        for (var i = 0; i < _enemyLevelMultipliers.Length; i++)
        {
            var level = i + 1;
            if (_enemyLevelMultipliers[i].Level == 0)
            {
                _enemyLevelMultipliers[i] = EnemyLevelMultiplier.Default(level);
                continue;
            }

            _enemyLevelMultipliers[i].Level = level;
            if (_enemyLevelMultipliers[i].HpMultiplier <= 0f)
                _enemyLevelMultipliers[i].HpMultiplier = EnemyLevelMultiplier.Default(level).HpMultiplier;
            if (_enemyLevelMultipliers[i].AttackMultiplier <= 0f)
                _enemyLevelMultipliers[i].AttackMultiplier = EnemyLevelMultiplier.Default(level).AttackMultiplier;
        }

        if (_classModifiers == null || _classModifiers.Length == 0)
        {
            _classModifiers = new[]
            {
                new PlayerClassModifier(PlayerClass.Warrior, 1f, 0.9f),
                new PlayerClassModifier(PlayerClass.Mage, 1.2f, 1.1f),
                new PlayerClassModifier(PlayerClass.Rogue, 1.1f, 1f)
            };
        }
    }
}

[Serializable]
public struct EnemyLevelMultiplier
{
    public int Level;
    public float HpMultiplier;
    public float AttackMultiplier;

    public EnemyLevelMultiplier(int level, float hpMultiplier, float attackMultiplier)
    {
        Level = level;
        HpMultiplier = hpMultiplier;
        AttackMultiplier = attackMultiplier;
    }

    public static EnemyLevelMultiplier Default(int level)
    {
        var hp = 1f + (level - 1) * 0.12f;
        var attack = 1f + (level - 1) * 0.1f;
        return new EnemyLevelMultiplier(level, hp, attack);
    }
}

[Serializable]
public struct PlayerClassModifier
{
    public PlayerClass PlayerClass;
    public float OutgoingDamageMultiplier;
    public float IncomingDamageMultiplier;

    public PlayerClassModifier(PlayerClass playerClass, float outgoingDamageMultiplier, float incomingDamageMultiplier)
    {
        PlayerClass = playerClass;
        OutgoingDamageMultiplier = outgoingDamageMultiplier;
        IncomingDamageMultiplier = incomingDamageMultiplier;
    }

    public static PlayerClassModifier Default(PlayerClass playerClass)
    {
        return new PlayerClassModifier(playerClass, 1f, 1f);
    }
}
