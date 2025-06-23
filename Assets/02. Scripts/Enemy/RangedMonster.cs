using UnityEngine;
using System.Collections;
using TMPro;

// 원거리 몬스터 클래스
public class RangedMonster : MonsterBase
{
    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float minAttackDistance = 3f; // 최소 공격 거리
    
    protected override void Start()
    {
        base.Start();
        attackRange = 8f; // 원거리 공격 범위
        moveSpeed = 1.5f; // 원거리 몬스터는 조금 느림
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
        // 이동 멈춤
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // 공격 쿨다운 체크
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        if (canAttack && !isAttacking)
        {
            StartCoroutine(RangedAttack());
        }
    }
    
    private IEnumerator RangedAttack()
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
        
        // 투사체 발사
        if (projectilePrefab != null && firePoint != null && player != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            // 플레이어 방향으로 발사
            Vector2 direction = (player.position - firePoint.position).normalized;
            
            // 투사체 컴포넌트 설정
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(direction, projectileSpeed, attackDamage);
            }
            else
            {
                // 기본 투사체 움직임
                Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
                if (projRb != null)
                {
                    projRb.linearVelocity = direction * projectileSpeed;
                }
            }
            
            // 투사체 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // 공격 애니메이션 종료 대기
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
        
        // 쿨다운 대기
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}