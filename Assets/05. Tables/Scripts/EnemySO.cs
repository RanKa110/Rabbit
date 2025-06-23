using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    Melee,
    Ranged,
    Boss
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject, IStatProvider
{
    public EnemyType Type;

    [Tooltip("Detection range for transitioning from Idle to Chasing")]
    public float DetectionRange = 10f;

    [Tooltip("Optional projectile prefab for ranged attacks")]
    public GameObject ProjectilePrefab;

    [Header("Base Stats and Modifiers")]
    public List<StatData> EnemyStats;
    public List<StatData> Stats => EnemyStats;
}
