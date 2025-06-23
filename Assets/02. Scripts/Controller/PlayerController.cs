using System.Collections.Generic;
using System.Linq;
using PlayerStates;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable
{
    private CharacterController _characterController;

    private Vector2 _moveInput;
    private bool _isRunning;
    private bool _attackTriggered;

    private List<IDamageable> _targets = new List<IDamageable>();
    public bool IsTargetExists => _targets.Count > 0;

    public Vector2 MoveInput => _moveInput;
    public bool IsRunning => _isRunning;

    public bool AttackTriggered
    {
        get => _attackTriggered;
        set => _attackTriggered = value;
    }

    public StatBase AttackStat { get; private set; }
    public IDamageable Target { get; private set; }
    public Transform Transform  => transform;



    protected override void Awake()
    {
        base.Awake();
        _characterController = GetComponent<CharacterController>();
        
        PlayerTable playerTable = TableManager.Instance.GetTable<PlayerTable>();
        PlayerSO playerData = playerTable.GetDataByID(0);
        StatManager.Initialize(playerData, null);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        Rotate();
        FindTarget();
    }

    /// <summary>
    /// 플레이어의 State를 생성해주는 팩토리 입니다.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    protected override IState<PlayerController, PlayerState> GetState(PlayerState state)
    {
        return state switch
        {
            PlayerState.Idle => new IdleState(),
            PlayerState.Move => new MoveState(),
            PlayerState.Attack => new AttackState(StatManager.GetValue(StatType.AttackSpd), StatManager.GetValue(StatType.AttackRange)),
            PlayerState.Run => new RunState(),
            _ => null
        };
    }

    public override void Movement()
    {
        
    }

    public void Rotate()
    {
        
    }

    public void Attack()
    {
        Target?.TakeDamage(this);
    }

    public void AttackAllTargets()
    {
        
    }

    public override void FindTarget()
    {
        
    }
    

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}