using UnityEngine;

public enum EnemyState
{
   Idle,     // 대기 상태
   Chasing,  // 추격 상태
   Attack,   // 공격 상태 (근거리/원거리/유도 공격)
   Die       // 사망 상태
}