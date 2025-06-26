using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using BossStates;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(Collider2D))]

public class BossController : BaseController<BossController, BossState>, IAttackable, IDamageable, IUnitController, IHasGauge
{
    [Header("Boss 데이터")]
    [SerializeField] private BossSO _data;
    public BossSO Data => _data;

    //  스탯 & 범위
    public StatBase AttackStat { get; private set; }
    public float DetectionRange => _data.detectionRange;
    public float AttackRange => StatManager.GetValueSafe(StatType.AttackRange, 1f);
    public float MoveSpeed => StatManager.GetValueSafe(StatType.MoveSpeed, 3f);
    public float AttackCooldown => _data.attackCooldown;

    //  게이지 관련 → 보스가 기본 공격 시 채워지는 게이지
    [Header("기본 공격 게이지 설정")]
    [SerializeField] private float maxBasicGauge = 100f;
    [SerializeField] private float gaugePerBasicAttack = 35f;

    //  이동 잠금용 플래그
    private bool _canMove = true;
    public bool CanMove => _canMove;

    //  내부 컴포넌트 참조
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private IDamageable _target;
    private bool _isDead;

    private GaugeManager _gauge;
    private MovementHandler _movementHandler;
    private AttackHandler _attackHandler;
    private DamageReceiver _damageReceiver;
    private AnimationHandler _animationHandler;

    public MovementHandler MovementHandler => _movementHandler;
    public AttackHandler AttackHandler => _attackHandler;

    public float GetPatternDelay(int idx) => Data.PatternDelays[idx];

    //  IUnitController
    public bool IsDead => _isDead;
    public Collider2D Collider => _collider;
    public IDamageable Target => _target;

    protected override void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        //  Rigidbody2D 세팅
        _rb.linearDamping = 0f;
        _rb.angularDamping = 0f;
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        //  핸들러 초기화
        _animationHandler = new AnimationHandler(GetComponent<Animator>());
        _movementHandler = new MovementHandler(_rb, this);
        _attackHandler = new AttackHandler(this, () => Target, GetComponent<Animator>());
    }

    protected override void Start()
    {
        if (_data == null)
        {
            Debug.LogError("Boss SO가 할당되지 않았습니다!");
            return;
        }

        //  스탯 초기화
        StatManager.Initialize(_data, this);
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);

        //  게이지 매니저
        _gauge = new GaugeManager(maxBasicGauge, gaugePerBasicAttack);

        //  데미지 수신자
        _damageReceiver = new DamageReceiver(StatManager, this, OnDeathCoroutine(), this);

        base.Start();
    }

    protected override void Update()
    {
        FindTarget();
        base.Update();
    }

    protected override IState<BossController, BossState> GetState(BossState state) => state switch
    {
        BossState.Idle => new IdleState(),
        BossState.Chasing => new ChasingState(),
        BossState.Attack => new AttackState(),
        BossState.Pattern1 => new PatternState(0),
        BossState.Pattern2 => new PatternState(1),
        BossState.Pattern3 => new PatternState(2),
        BossState.Evade => new EvadeState(),
        BossState.Die => new DieState(),
        _ => null
    };

    public override void Movement()
    {
        //  공격 중에는 이동하지 않도록!
        if (!CanMove)
        {
            SetAnimationMoving(false);
            return;
        }

        FaceToTarget();
        _movementHandler.Chase();
        SetAnimationMoving(true);
    }

    public void SetCanMove(bool value) => _canMove = value;

    public override void FindTarget()
    {
        if (_target != null && !_target.IsDead)
        {
            return;
        }

        var player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            _target = player.GetComponent<IDamageable>();
        }
    }

    public void Attack()
    {
        StartCoroutine(_attackHandler.BasicAttackCoroutine());
    }

    public void TakeDamage(IAttackable attacker) => _damageReceiver.TakeDamage(attacker);

    public void Dead() => _isDead = true;

    public void SetAnimationMoving(bool isMoving) => _animationHandler.SetMoveAnimation(isMoving);

    public void SetAnimationAttack() => _animationHandler.PlayAttackAnimation();

    public void SetAnimationDeath() => _animationHandler.PlayDeathAnimation();

    private IEnumerator OnDeathCoroutine()
    {
        SetAnimationDeath();
        yield return new WaitForSeconds(0.5f);
        ChangeState(BossState.Die);
    }


    public void AddBasicGauge() => _gauge.Add();

    public bool IsBasicGaugeFull() => _gauge.IsFull();

    public void ResetBasicGauge() => _gauge.Reset();
   
    public void FaceToTarget()
    {
        if (Target == null)
        {
            return;
        }

        var direction = Target.Collider.bounds.center - _collider.bounds.center;

        var rotate = transform.localScale;

        transform.localScale = new Vector2(direction.x >= 0 ? 1 : -1, rotate.y);
    }
}
