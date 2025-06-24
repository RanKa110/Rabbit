using UnityEngine;
using System.Collections;
using TMPro;

// 근거리 몬스터 클래스
public class MeleeMonster : MonsterBase
{
    [Header("근거리 공격 설정")]
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;
    
    // EnemyController 참조
    private EnemyController enemyController;
    
    protected override void Start()
    {
        base.Start();
        attackRange = 1.5f; // 근거리 공격 범위
        
        // EnemyController 참조 가져오기
        enemyController = GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("EnemyController component not found on MeleeMonster!");
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
            StartCoroutine(MeleeAttack());
        }
    }
    
    private IEnumerator MeleeAttack()
    {
        isAttacking = true;
        canAttack = false;
        lastAttackTime = Time.time;
        
        // 공격 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 공격 애니메이션 대기
        yield return new WaitForSeconds(0.3f);
        
        // 공격 범위 내의 플레이어 탐지
        if (attackPoint != null)
        {
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
            
            foreach (Collider2D hit in hitPlayers)
            {
                if (hit.CompareTag("Player"))
                {
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null && enemyController != null)
                    {
                        // EnemyController의 SetAttackTarget과 Attack 사용
                        enemyController.SetAttackTarget(damageable);
                        enemyController.Attack();
                    }
                    else if (damageable == null)
                    {
                        // 안전한 데미지 처리 (호환성 유지)
                        DealDamageToPlayer(hit.gameObject, attackDamage);
                    }
                    
                    // 넉백 효과
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
                    }
                }
            }
        }
        
        // 공격 애니메이션 종료 대기
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
        
        // 쿨다운 대기
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    // 공격 범위 시각화 (에디터용)
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}