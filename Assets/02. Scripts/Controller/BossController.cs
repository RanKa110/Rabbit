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
    [SerializeField] public BossSO Data;
    private Rigidbody2D _rb;
    private IDamageable _target;
    private bool _isDead;

    public bool IsDead => _isDead;
    public Collider2D Collider { get; private set; }
    public float DetectionRange => Data.detectionRange;
    public StatBase AttackStat { get; private set; }
    public IDamageable Target => _target;

    public float AttackCooldownValue => Data.attackCooldown;
    public int PatternCount => Data.PatternDelays.Length;
    public float GetPatternDelay(int idx) => Data.PatternDelays[idx];

    protected override void Awake()
    {
        Debug.Log("▶▶▶ BaseController.SetupState should run now");

        base.Awake();

        _rb = GetComponent<Rigidbody2D>();
        Collider = GetComponent<Collider2D>();
    }

    protected override void Start()
    {
        BossTable bossTable = TableManager.Instance.GetTable<BossTable>();
        BossSO bossData = bossTable.GetDataByID(0);
        StatManager.Initialize(bossData, this);
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
    }

    protected override void Update()
    {
        base.Update();
        FindTarget();           //  타겟 먼저 찾기
    }

    protected override IState<BossController, BossState> GetState(BossState state) => state switch
    {
        BossState.Idle => new IdleState(),
        BossState.Chasing => new ChasingState(),
        BossState.Attack => new AttackState(1, 1), // 나중에 보스 기획 단계에서 수정하기
        BossState.Pattern1 => new PatternState(0),
        BossState.Pattern2 => new PatternState(1),
        BossState.Pattern3 => new PatternState(2),
        BossState.Die => new DieState(),
        _ => null
    };

    //  2D 플랫폼 액션 이동
    public override void Movement()
    {
        Debug.Log("보스 이동 시작!");

        if (_target == null)
        {
            return;
        }

        float speed = StatManager.GetValue(StatType.MoveSpeed);

        Vector2 origin = (Vector2)transform.position;
        Vector2 targetPos = _target.Collider.transform.position;
        Vector2 dir = (targetPos - origin).normalized;

        _rb.linearVelocity = new Vector2(dir.x * speed, _rb.linearVelocity.y);
    }

    //  보스 기본 공격
    public void BasicAttack()
    {
        Debug.Log("보스 기본 공격 시작!");

        if (_isDead || _target == null)
        {
            return;
        }

        _target.TakeDamage(this);
    }

    public void Attack()
    {
        BasicAttack();
    }

    public override void FindTarget()
    {
        Debug.Log("Boss FindTarget()");

        if (_target != null && !_target.IsDead)
        {
            return;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        _target = playerObj.GetComponent<IDamageable>();
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        ChangeState(BossState.Die);
    }

    public void Dead() 
    {
    }
}
