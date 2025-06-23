using UnityEngine;
using System;

//[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]

public class EnemyController : BaseController<EnemyController, EnemyState>, IAttackable, IDamageable
{
    private CharacterController _characterController;
    private IDamageable _target;
    private bool _isDead;

    public bool IsDead => _isDead;
    public Collider Collider { get; private set; }
    public StatBase AttackStat { get; private set; }
    public IDamageable Target => _target;

    [SerializeField] private EnemySO _data;
    public EnemySO Data => _data;

    [Header("공격 설정")]
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;
    
    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;
    public GameObject homingProjectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float minAttackDistance = 3f;
    
    public enum MonsterType { Melee, Ranged, Homing }
    
    [Header("몬스터 타입")]
    public MonsterType monsterType = MonsterType.Melee;
    
    [Header("이펙트")]
    public GameObject hitEffectPrefab;
    
    // 컴포넌트
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    
    // 상태 변수
    public bool IsAttacking { get; set; }
    public bool CanAttack { get; set; } = true;
    public float LastAttackTime { get; set; }

    protected override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Collider = GetComponent<Collider>();
        StatManager.Initialize(Data, this);
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        FindTarget();
        UpdateSpriteDirection();
    }

    protected override IState<EnemyController, EnemyState> GetState(EnemyState state) => state switch
    {
        EnemyState.Idle => new EnemyStates.IdleState(),
        EnemyState.Chasing => new EnemyStates.ChasingState(),
        EnemyState.Attack => monsterType switch
        {
            MonsterType.Melee => new EnemyStates.MeleeAttackState(
                StatManager.GetValue(StatType.AttackSpd),
                StatManager.GetValue(StatType.AttackRange)),
            MonsterType.Ranged => new EnemyStates.RangedAttackState(
                StatManager.GetValue(StatType.AttackSpd),
                StatManager.GetValue(StatType.AttackRange)),
            MonsterType.Homing => new EnemyStates.HomingAttackState(
                StatManager.GetValue(StatType.AttackSpd),
                StatManager.GetValue(StatType.AttackRange)),
            _ => new EnemyStates.MeleeAttackState(
                StatManager.GetValue(StatType.AttackSpd),
                StatManager.GetValue(StatType.AttackRange))
        },
        EnemyState.Die => new EnemyStates.DieState(),
        _ => null
    };

    public override void Movement()
    {
        base.Movement();

        if (_target == null)
        {
            return;
        }

        float speed = StatManager.GetValue(StatType.MoveSpeed);
        Vector3 dir = (_target.Collider.transform.position - transform.position).normalized;

        if (_characterController != null && _characterController.enabled)
        {
            _characterController.Move(dir * speed * Time.deltaTime);
        }
        else if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = new Vector2(dir.x * speed, _rigidbody2D.linearVelocity.y);
        }
    }

    public void MovementWithDistance(float minDistance)
    {
        if (_target == null) return;

        float speed = StatManager.GetValue(StatType.MoveSpeed);
        float distanceToTarget = Vector3.Distance(transform.position, _target.Collider.transform.position);
        
        Vector3 dir;
        
        // 너무 가까우면 뒤로 이동
        if (distanceToTarget < minDistance)
        {
            dir = (transform.position - _target.Collider.transform.position).normalized;
        }
        // 적정 거리보다 멀면 접근
        else if (distanceToTarget > StatManager.GetValue(StatType.AttackRange) * 0.8f)
        {
            dir = (_target.Collider.transform.position - transform.position).normalized;
        }
        // 적정 거리면 정지
        else
        {
            if (_rigidbody2D != null)
            {
                _rigidbody2D.linearVelocity = new Vector2(0, _rigidbody2D.linearVelocity.y);
            }
            return;
        }

        if (_characterController != null && _characterController.enabled)
        {
            _characterController.Move(dir * speed * Time.deltaTime);
        }
        else if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = new Vector2(dir.x * speed, _rigidbody2D.linearVelocity.y);
        }
    }

    public void Attack()
    {
        _target?.TakeDamage(this);
    }

    public void MeleeAttack()
    {
        if (attackPoint != null)
        {
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
            
            foreach (Collider2D hit in hitPlayers)
            {
                if (hit.CompareTag("Player"))
                {
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(this);
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
    }

    public void RangedAttack()
    {
        if (projectilePrefab != null && firePoint != null && _target != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            // 플레이어 방향으로 발사
            Vector2 direction = (_target.Collider.transform.position - firePoint.position).normalized;
            
            // 투사체 컴포넌트 설정
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(direction, projectileSpeed, StatManager.GetValue(StatType.AttackPow));
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
    }

    public void HomingAttack()
    {
        if (homingProjectilePrefab != null && firePoint != null && _target != null)
        {
            GameObject projectile = Instantiate(homingProjectilePrefab, firePoint.position, Quaternion.identity);
            
            // 유도 투사체 설정
            HomingProjectile homing = projectile.GetComponent<HomingProjectile>();
            if (homing != null)
            {
                homing.SetTarget(_target.Collider.transform);
                homing.damage = StatManager.GetValue(StatType.AttackPow);
            }
            
            // 초기 방향 설정
            Vector2 direction = (_target.Collider.transform.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public override void FindTarget()
    {
        if (_target != null && !_target.IsDead)
        {
            return;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            _target = playerObj.GetComponent<IDamageable>();
            if (_target == null)
            {
                // PlayerController가 IDamageable을 구현하지 않은 경우 처리
                Debug.LogWarning("Player doesn't implement IDamageable interface");
            }
        }
    }

    private void UpdateSpriteDirection()
    {
        if (_target == null || _spriteRenderer == null) return;
        
        // 타겟이 왼쪽에 있으면 왼쪽을 봄
        if (_target.Collider.transform.position.x < transform.position.x)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }
    }

    public void Dead()
    {
        _isDead = true;
        
        // 애니메이션 트리거
        if (_animator != null)
        {
            _animator.SetTrigger("Death");
        }
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (_isDead) return;
        
        float damage = attacker.AttackStat?.Value ?? 0;
        
        // HP 감소 (데미지 처리)
        StatManager.Consume(StatType.CurHp, StatModifierType.Base, damage);
        
        // 피격 효과
        StartCoroutine(HitEffect());
        
        // HP 체크 (StatManager의 Consume에서 자동으로 Dead()가 호출되지만 상태 변경은 여기서)
        float currentHealth = StatManager.GetValue(StatType.CurHp);
        if (currentHealth <= 0)
        {
            ChangeState(EnemyState.Die);
        }
    }

    private System.Collections.IEnumerator HitEffect()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = Color.white;
        }
    }

    // 공격 범위 시각화 (에디터용)
    private void OnDrawGizmosSelected()
    {
        if (monsterType == MonsterType.Melee && attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        
        // 감지 범위
        if (Data != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Data.DetectionRange);
        }
    }
}