using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(StatManager))]
[RequireComponent(typeof(StatusEffectManager))]

public class EnemyController : BaseController<EnemyController, EnemyState>, IAttackable, IDamageable
{
    private Rigidbody2D _rb;
    private IDamageable _target;
    private bool _isDead;

    public bool IsDead => _isDead;
    public Collider2D Collider { get; private set; }
    public StatBase AttackStat { get; private set; }
    public IDamageable Target => _target;

    [SerializeField] private EnemySO _data;
    public EnemySO Data => _data;

    protected override void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody2D>();
        Collider = GetComponent<Collider2D>();
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
    }

    protected override IState<EnemyController, EnemyState> GetState(EnemyState state) => state switch
    {
        EnemyState.Idle => new EnemyStates.IdleState(),
        EnemyState.Chasing => new EnemyStates.ChasingState(),
        EnemyState.Attack => new EnemyStates.AttackState(
            StatManager.GetValue(StatType.AttackSpd),
            StatManager.GetValue(StatType.AttackRange)),
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

        _rb.linearVelocity = new Vector2(dir.x * speed, _rb.linearVelocity.y);
    }


    public void Attack()
    {
        _target?.TakeDamage(this);
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
        }
    }

    public void Dead()
    {

    }

    public void TakeDamage(IAttackable attacker)
    {
    }
}