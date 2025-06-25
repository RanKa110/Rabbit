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
    protected EnemyHealthBar healthBar; // HP바 컴포넌트 추가
    
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
        
        // HP바 컴포넌트 가져오기 또는 추가
        healthBar = GetComponent<EnemyHealthBar>();
        if (healthBar == null)
        {
            healthBar = gameObject.AddComponent<EnemyHealthBar>();
            Debug.Log($"{gameObject.name}: EnemyHealthBar component added automatically");
        }
        
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
        // 기본 대기 동작 - X축 속도를 0으로 설정
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
        
        // HP바 업데이트
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
            healthBar.ShowHealthBar(); // 데미지를 받으면 HP바 표시
        }
        
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
        
        // HP바 숨기기
        if (healthBar != null)
        {
            healthBar.HideHealthBar();
        }
        
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
    
    // ===== Animation Event 함수들 =====
    // 애니메이션에서 호출될 수 있는 기본 함수들
    
    // Idle 애니메이션 이벤트 - 가장 중요! 이 함수가 없어서 에러가 발생합니다
    public void Idle()
    {
        // Idle 애니메이션에서 호출되는 함수
        // virtual이 아닌 일반 public 함수로 선언
        // 필요한 경우 여기에 로직 추가 가능
    }
    
    // 공격 애니메이션에서 호출
    public virtual void OnAttackHit()
    {
        Debug.Log($"{gameObject.name}: OnAttackHit called from animation");
    }
    
    // 공격 시작 시 호출
    public virtual void OnAttackStart()
    {
        Debug.Log($"{gameObject.name}: OnAttackStart called from animation");
    }
    
    // 공격 종료 시 호출
    public virtual void OnAttackEnd()
    {
        Debug.Log($"{gameObject.name}: OnAttackEnd called from animation");
    }
    
    // 발걸음 소리 등
    public virtual void OnFootstep()
    {
        Debug.Log($"{gameObject.name}: OnFootstep called from animation");
    }
    
    // 사망 애니메이션 종료
    public virtual void OnDeathAnimationEnd()
    {
        Debug.Log($"{gameObject.name}: OnDeathAnimationEnd called from animation");
        Destroy(gameObject);
    }
    
    // Move/Walk 애니메이션 이벤트
    public void Move()
    {
        // Move 애니메이션에서 호출되는 함수
    }
    
    // Run 애니메이션 이벤트
    public void Run()
    {
        // Run 애니메이션에서 호출되는 함수
    }
    
    // Attack 애니메이션 이벤트
    public void OnAttackAnimation()
    {
        // Attack 애니메이션에서 호출되는 함수
        // IAttackable의 Attack()과 구별하기 위해 이름 변경
    }
    
    // 일반적인 애니메이션 이벤트
    public void AnimationEvent()
    {
        // 범용 애니메이션 이벤트 함수
    }
    
    public void AnimationEvent(string parameter)
    {
        // 파라미터를 받는 범용 애니메이션 이벤트
        Debug.Log($"{gameObject.name}: AnimationEvent called with parameter: {parameter}");
    }
    
    // 추가 애니메이션 이벤트들 (필요시 사용)
    public void Step() { }  // 발걸음
    public void Land() { }  // 착지
    public void Jump() { }  // 점프
    public void Hit() { }   // 피격
    public void Death() { } // 사망
}