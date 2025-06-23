using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "BossData", menuName = "Scriptable Objects/BossSO")]
public class BossSO : ScriptableObject, IStatProvider
{
    public EnemyType Type;

    [Tooltip("Detection range for transition from Idle to Chasing")]
    public float detectionRange = 10f;

    [Tooltip("Optional projectile prefab for ranged attacks")]
    public GameObject projectilePrefab;

    [Range(0f, 1f)]
    [Tooltip("Chance to parry a player's attack")]
    public float parryChance = 0.1f;

    [Tooltip("Delay after each attack before next state transition")]
    public float attackCooldown = 1f;

    [Tooltip("Custom cooldown / timing for each special pattern")]
    public float[] PatternDelays = new float[3];

    [Header("Base Stats and Modifiers")]
    public List<StatData> enemyStats;
    public List<StatData> Stats => enemyStats;
}
