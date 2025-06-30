using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "BossData", menuName = "Scriptable Objects/BossSO")]
public class BossSO : ScriptableObject, IStatProvider
{
    public int ID;
    public EnemyType Type;

    [Tooltip("추격을 위한 적 감지 거리")]
    public float detectionRange = 10f;

    [Tooltip("원거리 공격을 위한 투사체 프리팹 세팅")]
    public GameObject projectilePrefab;
    public float projectileSpeed;

    [Range(0f, 1f)]
    [Tooltip("패링 확률")]
    public float parryChance = 0.1f;

    [Tooltip("기본 공격 쿨타임")]
    public float attackCooldown = 1f;

    [Tooltip("패턴 딜레이 (단위:초)")]
    public float[] PatternDelays = new float[3];

    [Header("Base Stats and Modifiers")]
    public List<StatData> enemyStats;
    public List<StatData> Stats => enemyStats;

    [Header("Pattern2: 총알 난사")]
    [Tooltip("패턴2: 한 번에 발사할 총알 수")]
    public int pattern2ShotCount;
    [Tooltip("패턴2: 총알 연사 간격(초)")]
    public float pattern2ShotInterval;
}
