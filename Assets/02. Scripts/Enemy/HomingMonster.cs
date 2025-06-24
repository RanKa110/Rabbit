using UnityEngine;
using System.Collections;
using TMPro;

// 유도 몬스터 클래스
public class HomingMonster : MonsterBase
{
    [Header("유도 공격 설정")]
    public GameObject homingProjectilePrefab;
    public Transform firePoint;
    public float minAttackDistance = 5f; // 최소 공격 거리 (원거리보다 더 멈)
    
    // EnemyController 참조
    private EnemyController enemyController;
    
    protected override void Start()
    {
        base.Start();
        attackRange = 10f; // 유도 공격 범위 (더 긴 사거리)
        moveSpeed = 1.2f; // 유도 몬스터는 더 느림
        attackCooldown = 2f; // 더 긴 쿨다운
        
        // EnemyController 참조 가져오기
        enemyController = GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("HomingMonster: EnemyController component not found!");
        }
        else if (enemyController.Data != null)
        {
            // SO에서 프리팹 가져오기 - 새로운 메서드 사용
            GameObject prefabFromSO = enemyController.Data.GetProjectilePrefab();
            if (prefabFromSO != null)
            {
                homingProjectilePrefab = prefabFromSO;
                Debug.Log($"HomingMonster: Projectile prefab loaded from SO: {homingProjectilePrefab.name}");
            }
            else
            {
                Debug.LogWarning("HomingMonster: No projectile prefab found in EnemySO! Make sure HomingProjectilePrefab is set.");
            }
        }
        
        // firePoint가 없으면 자동으로 찾기
        if (firePoint == null)
        {
            // 먼저 자식에서 FirePoint 찾기
            firePoint = transform.Find("FirePoint");
            
            // 없으면 자동 생성
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = new Vector3(0.5f, 0, 0); // 캐릭터 앞쪽
                firePoint = firePointObj.transform;
                Debug.LogWarning("HomingMonster: FirePoint was created automatically. Please adjust position in prefab.");
            }
        }
    }
    
    protected override void ChaseBehavior()
    {
        if (player == null) return;
        
        float distanceToPlayer = GetDistanceToPlayer();
        
        // 너무 가까우면 뒤로 이동
        if (distanceToPlayer < minAttackDistance)
        {
            Vector2 direction = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
        // 적정 거리보다 멀면 접근
        else if (distanceToPlayer > attackRange * 0.8f)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
        // 적정 거리면 정지
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        
        // 애니메이션
        if (animator != null)
        {
            animator.SetBool("isMoving", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }
    
    protected override void AttackBehavior()
    {
        // 이동 멈춤 - X축 속도를 0으로
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // 공격 쿨다운 체크
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        if (canAttack && !isAttacking)
        {
            StartCoroutine(HomingAttack());
        }
    }
    
    // 공용 메서드로 변경하여 EnemyController에서 호출 가능하도록 함
    public void PerformHomingAttack()
    {
        // 조건 체크 없이 바로 공격 실행
        FireHomingProjectile();
    }
    
    private void FireHomingProjectile()
    {
        // 디버그 로그
        Debug.Log($"FireHomingProjectile called. Prefab: {homingProjectilePrefab}, FirePoint: {firePoint}");
        
        // 유도 투사체 발사
        if (homingProjectilePrefab != null && firePoint != null)
        {
            Transform targetTransform = null;
            
            // EnemyController에서 타겟 가져오기
            if (enemyController != null && enemyController.Target != null)
            {
                targetTransform = enemyController.Target.Collider.transform;
            }
            // 폴백: player 직접 사용
            else if (player != null)
            {
                targetTransform = player;
            }
            
            if (targetTransform != null)
            {
                GameObject projectile = Instantiate(homingProjectilePrefab, firePoint.position, Quaternion.identity);
                Debug.Log($"Homing projectile instantiated: {projectile.name}");
                
                // 유도 투사체가 알아서 타겟을 추적하도록 함
                HomingProjectile homing = projectile.GetComponent<HomingProjectile>();
                if (homing != null)
                {
                    homing.SetTarget(targetTransform);
                    
                    // EnemyController의 스탯을 사용하거나 자체 스탯 사용
                    homing.damage = enemyController != null ? 
                        enemyController.StatManager.GetValue(StatType.AttackPow) : 
                        attackDamage;
                }
                else
                {
                    Debug.LogWarning("HomingMonster: HomingProjectile component not found on projectile!");
                }
            }
            else
            {
                Debug.LogWarning("HomingMonster: No target found for projectile!");
            }
        }
        else
        {
            Debug.LogWarning($"HomingMonster: Cannot fire projectile! Prefab: {homingProjectilePrefab}, FirePoint: {firePoint}");
        }
    }
    
    private IEnumerator HomingAttack()
    {
        isAttacking = true;
        canAttack = false;
        lastAttackTime = Time.time;
        
        // 공격 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 애니메이션 대기
        yield return new WaitForSeconds(0.3f);
        
        // 유도 투사체 발사
        FireHomingProjectile();
        
        // 공격 애니메이션 종료 대기
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
        
        // 쿨다운 대기 (유도 미사일은 좀 더 긴 쿨다운)
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}