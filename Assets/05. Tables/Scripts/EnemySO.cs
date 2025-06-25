using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    Melee,
    Ranged,
    Homing,
    MultiShot,
    Boss
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject, IStatProvider
{
    public EnemyType Type;

    [Tooltip("Detection range for transitioning from Idle to Chasing")]
    public float DetectionRange = 10f;

    [Header("Attack Prefabs")]
    [Tooltip("Projectile prefab for ranged attacks")]
    public GameObject RangedProjectilePrefab;
    
    [Tooltip("Projectile prefab for homing attacks")]
    public GameObject HomingProjectilePrefab;

    [Tooltip("Projectile prefab for homing attacks")]
    public GameObject MultiShotProjectilePrefab;

    // 기존 필드는 호환성을 위해 유지 (deprecated)
    [HideInInspector]
    public GameObject ProjectilePrefab;

    [Header("Base Stats and Modifiers")]
    public List<StatData> EnemyStats;
    public List<StatData> Stats => EnemyStats;
    
    // 타입에 따른 프리팹 반환 메서드
    public GameObject GetProjectilePrefab()
    {
        switch (Type)
        {
            case EnemyType.Ranged:
                return RangedProjectilePrefab != null ? RangedProjectilePrefab : ProjectilePrefab;
            case EnemyType.Homing:
                return HomingProjectilePrefab != null ? HomingProjectilePrefab : ProjectilePrefab;
            default:
                return null;
        }
    }
}