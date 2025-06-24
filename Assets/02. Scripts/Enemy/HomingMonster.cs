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
            Debug.LogError("EnemyController component not found on HomingMonster!");
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
        if (canAttack && !isAttacking)
        {
            StartCoroutine(HomingAttack());
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
            }
        }
        
        // 공격 애니메이션 종료 대기
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
        
        // 쿨다운 대기 (유도 미사일은 좀 더 긴 쿨다운)
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}