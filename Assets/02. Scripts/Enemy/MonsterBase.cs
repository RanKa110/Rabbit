using UnityEngine;
using System.Collections;
using TMPro;

// 몬스터 기본 클래스
public abstract class MonsterBase : MonoBehaviour, IAttackable
{
    [Header("기본 스탯")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    
    [Header("컴포넌트")]
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected Transform player;
    
    [Header("상태")]
    protected bool isAttacking = false;
    protected bool isDead = false;
    protected bool canAttack = true;
    protected float lastAttackTime;
    
    // IAttackable 구현
    public StatBase AttackStat { get; protected set; }
    public IDamageable Target { get; protected set; }
    
    // 몬스터 상태 enum
    public enum MonsterState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }
    
    public MonsterState currentState = MonsterState.Idle;
    
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
        
        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // AttackStat 초기화
        AttackStat = new CalculatedStat(StatType.AttackPow, attackDamage);
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        // 플레이어와의 거리 체크
        float distanceToPlayer = GetDistanceToPlayer();
        
        // 상태 머신
        switch (currentState)
        {
            case MonsterState.Idle:
                IdleBehavior();
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = MonsterState.Chase;
                }
                break;
                
            case MonsterState.Chase:
                ChaseBehavior();
                if (distanceToPlayer > detectionRange)
                {
                    currentState = MonsterState.Idle;
                }
                else if (distanceToPlayer <= attackRange)
                {
                    currentState = MonsterState.Attack;
                }
                break;
                
            case MonsterState.Attack:
                AttackBehavior();
                if (distanceToPlayer > attackRange)
                {
                    currentState = MonsterState.Chase;
                }
                break;
        }
        
        // 스프라이트 방향 전환
        UpdateSpriteDirection();
    }
    
    protected virtual void IdleBehavior()
    {
        // 기본 대기 동작
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    protected virtual void ChaseBehavior()
    {
        if (player == null) return;
        
        // 플레이어 방향으로 이동
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        
        // 애니메이션
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }
    
    protected abstract void AttackBehavior();
    
    protected virtual void UpdateSpriteDirection()
    {
        if (player == null) return;
        
        // 플레이어가 왼쪽에 있으면 왼쪽을 봄
        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
    
    protected float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector2.Distance(transform.position, player.position);
    }
    
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // 피격 효과
        StartCoroutine(HitEffect());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual IEnumerator HitEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    
    protected virtual void Die()
    {
        isDead = true;
        currentState = MonsterState.Dead;
        
        // 충돌체 비활성화
        GetComponent<Collider2D>().enabled = false;
        
        // 사망 애니메이션
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // 일정 시간 후 제거
        Destroy(gameObject, 2f);
    }
    
    // IAttackable 구현
    public virtual void Attack()
    {
        if (Target != null)
        {
            Target.TakeDamage(this);
        }
    }
    
    // 안전한 플레이어 데미지 처리 메서드
    protected virtual void DealDamageToPlayer(GameObject playerObject, float damage)
    {
        if (playerObject == null) return;
        
        // IDamageable 인터페이스 확인
        IDamageable damageable = playerObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(this);
            return;
        }
        
        // PlayerController 직접 접근 (임시 호환성)
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // PlayerController에 TakeDamage가 없으므로 로그만 출력
            Debug.Log($"Monster dealt {damage} damage to player (PlayerController doesn't implement TakeDamage yet)");
            // 나중에 팀원이 PlayerController에 IDamageable을 구현하면 위의 IDamageable 체크가 작동할 것입니다.
        }
    }
    
    // 플레이어와 충돌 시
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            // 안전한 데미지 처리
            DealDamageToPlayer(collision.gameObject, attackDamage);
        }
    }
}