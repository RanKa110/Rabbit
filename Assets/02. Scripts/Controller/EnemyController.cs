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
    private EnemyHealthBar _healthBar; // HP바 컴포넌트 추가

    public bool IsDead => _isDead;
    public Collider2D Collider { get; private set; }
    public StatBase AttackStat { get; private set; }
    public IDamageable Target => _target;

    [SerializeField] private EnemySO _data;
    public EnemySO Data => _data;

    [Header("공격 설정")]
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;
    public float minAttackDistance = 3f;
    
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
        Collider = GetComponent<Collider2D>();
        
        // HP바 컴포넌트 가져오기 또는 추가
        _healthBar = GetComponent<EnemyHealthBar>();
        if (_healthBar == null)
        {
            _healthBar = gameObject.AddComponent<EnemyHealthBar>();
            Debug.Log($"{gameObject.name}: EnemyHealthBar component added automatically");
        }
        
        StatManager.Initialize(Data, this);
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
    }

    protected override void Start()
    {
        base.Start();
        
        // HP바 초기화
        if (_healthBar != null)
        {
            float maxHp = StatManager.GetValueSafe(StatType.MaxHp, 100f);
            float curHp = StatManager.GetValueSafe(StatType.CurHp, maxHp);
            _healthBar.SetHealth(curHp, maxHp);
        }
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
        EnemyState.Attack => new EnemyStates.AttackState(
            StatManager.GetValueSafe(StatType.AttackSpd, 1.0f), // 기본 공격 속도 1.0
            StatManager.GetValueSafe(StatType.AttackRange, 3.0f)), // 기본 공격 범위 3.0
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

        float speed = StatManager.GetValueSafe(StatType.MoveSpeed, 5f);
        Vector3 dir = (_target.Collider.transform.position - transform.position).normalized;

        if (_characterController != null && _characterController.enabled)
        {
            _characterController.Move(dir * speed * Time.deltaTime);
        }
        else if (_rigidbody2D != null)
        {
            // X축만 이동, Y축은 중력에 맡김
            _rigidbody2D.linearVelocity = new Vector2(dir.x * speed, _rigidbody2D.linearVelocity.y);
        }
    }

    public void MovementWithDistance(float minDistance)
    {
        if (_target == null) return;

        float speed = StatManager.GetValueSafe(StatType.MoveSpeed, 5f);
        float distanceToTarget = Vector3.Distance(transform.position, _target.Collider.transform.position);
        
        Vector3 dir;
        
        // 너무 가까우면 뒤로 이동
        if (distanceToTarget < minDistance)
        {
            dir = (transform.position - _target.Collider.transform.position).normalized;
        }
        // 적정 거리보다 멀면 접근
        else if (distanceToTarget > StatManager.GetValueSafe(StatType.AttackRange, 3.0f) * 0.8f)
        {
            dir = (_target.Collider.transform.position - transform.position).normalized;
        }
        // 적정 거리면 정지
        else
        {
            if (_rigidbody2D != null)
            {
                // X축만 정지, Y축은 중력에 맡김
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
            // X축만 이동, Y축은 중력에 맡김
            _rigidbody2D.linearVelocity = new Vector2(dir.x * speed, _rigidbody2D.linearVelocity.y);
        }
    }

    public void Attack()
    {
        _target?.TakeDamage(this);
    }

    // Melee 공격을 위한 타겟 설정 메서드
    public void SetAttackTarget(IDamageable target)
    {
        _target = target;
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
        
        // HP바 숨기기
        if (_healthBar != null)
        {
            _healthBar.HideHealthBar();
        }
        
        // 피격 효과 시작
        StartCoroutine(DeathEffect());
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (_isDead) return;
        
        float damage = attacker.AttackStat?.Value ?? 0;
        
        // HP 감소 (데미지 처리)
        StatManager.Consume(StatType.CurHp, StatModifierType.Base, damage);
        
        // HP바 업데이트
        if (_healthBar != null)
        {
            float maxHp = StatManager.GetValueSafe(StatType.MaxHp, 100f);
            float currentHp = StatManager.GetValueSafe(StatType.CurHp, maxHp);
            _healthBar.SetHealth(currentHp, maxHp);
            _healthBar.ShowHealthBar(); // 데미지를 받으면 HP바 표시
        }
        
        // 피격 효과
        StartCoroutine(HitEffect());
        
        // HP 체크 (StatManager의 Consume에서 자동으로 Dead()가 호출되지만 상태 변경은 여기서)
        float currentHealth = StatManager.GetValueSafe(StatType.CurHp, 0f);
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
    
    private System.Collections.IEnumerator DeathEffect()
    {
        if (_spriteRenderer != null)
        {
            // 페이드 아웃 효과
            Color originalColor = _spriteRenderer.color;
            float elapsed = 0f;
            float duration = 1f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                _spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            // 완전히 투명해지면 오브젝트 비활성화 또는 삭제
            gameObject.SetActive(false);
        }
    }

    // 공격 범위 시각화 (에디터용)
    private void OnDrawGizmosSelected()
    {
        // 근거리 공격 범위 (MeleeMonster가 있을 때만)
        if (GetComponent<MeleeMonster>() != null && attackPoint != null)
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