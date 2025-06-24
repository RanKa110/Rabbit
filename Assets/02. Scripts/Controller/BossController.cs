using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using BossStates;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(Collider2D))]

public class BossController : BaseController<BossController, BossState>, IAttackable, IDamageable
{
    [Header("Boss 데이터")]
    [SerializeField] public BossSO Data;

    [Header("기본 공격 게이지")]
    [SerializeField] private float maxBasicGauge = 100f;
    [SerializeField] private float gaugePerBasicAttack = 35f;

    //  내부 상태
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private IDamageable _target;
    private bool _isDead;

    //  서브 시스템
    private GaugeManager _gauge;
    private DamageReceiver _damageReceiver;
    private BossMovementHandler _movementHandler;
    private BossAttackHandler _attackHandler;

    //  외부 노출
    public bool IsDead => _isDead;
    public Collider2D Collider => _collider;
    public IDamageable Target => _target;
    public StatBase AttackStat { get; private set; }
    public float DetectionRange => Data.detectionRange;
    public BossState CurrentStateKey => _currentStateKey;

    public float AttackCooldownValue => Data.attackCooldown;
    public int PatternCount => Data.PatternDelays.Length;
    public float GetPatternDelay(int idx) => Data.PatternDelays[idx];

    protected override void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        _rb.linearDamping = 0f;
        _rb.angularDamping = 0f;
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    protected override void Start()
    {
        BossTable bossTable = TableManager.Instance.GetTable<BossTable>();
        BossSO bossData = bossTable.GetDataByID(0);

        StatManager.Initialize(bossData, this);
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);

        _gauge = new GaugeManager(maxBasicGauge, gaugePerBasicAttack);
        _damageReceiver = new DamageReceiver(StatManager, _collider, Data.parryChance, OnBossDeathCoroutine(), this);
        _movementHandler = new BossMovementHandler(_rb, this);
        _attackHandler = new BossAttackHandler(_damageReceiver, this, this);
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

    public override void Movement() => _movementHandler.Chase();

    //  플레이어를 어디서나 바라보도록 하는 메서드
    public void FaceToTarget()
    {
        if (Target == null)
        {
            return;
        }

        Vector2 scale = transform.localScale;
        float dirX = Target.Collider.bounds.center.x - transform.position.x;

        if (dirX > 0f)
        {
            scale.x = Mathf.Abs(scale.x);       //  오른쪽을 바라봄
        }

        else if (dirX < 0f)
        {
            scale.x = -Mathf.Abs(scale.x);      //  왼쪽 바라봄
        }

        transform.localScale = scale;
    }

    public void Attack() => StartCoroutine(_attackHandler.BasicAttackCoroutine(_gauge));

    public override void FindTarget()
    {
        if (_target != null && !_target.IsDead)
        {
            return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            _target = player.GetComponent<IDamageable>();
        }
    }

    public void TakeDamage(IAttackable attacker)
    {
        _damageReceiver.TakeDamage(attacker);
    }

    private IEnumerator OnBossDeathCoroutine()
    {
        Debug.Log("보스 사망 처리 시작!");

        //  사망 애니메이션, 이펙트, 사운드 등 여기에 삽입할 것!
        yield return new WaitForSeconds(0.5f);

        ChangeState(BossState.Die);
    }

    public void AddBasicGauge() => _gauge.Add();
    public bool IsBasicGaugeFull() => _gauge.IsFull();
    public void ResetBasicGauge() => _gauge.Reset();

    public void Dead()
    {
    }

    public void RequestEvade()
    {
        if (CurrentStateKey != BossState.Evade)
        {
            Debug.Log("RequestEvade() 호출 → 상태 전이");
            ChangeState(BossState.Evade);
        }
    }
}
