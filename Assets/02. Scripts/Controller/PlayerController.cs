using System;
using System.Collections.Generic;
using System.Linq;
using PlayerGroundStates;
using PlayerAirStates;
using PlayerAttackStates;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(InputController))]
public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable, IDamageable
{
    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D _boxCollider2D;
    private InputController _inputController;
    private SpriteRenderer _spriteRenderer;
    
    private Vector2 _moveInput;
    private bool _isCrouch;
    private bool _attackTriggered;
    private bool _jumpTriggered;
    private bool _doubleJumpAvailable = true;

    private List<IDamageable> _targets = new List<IDamageable>();

    public bool _isDead;
    
    public Vector2 MoveInput => _moveInput;
    public bool IsCrouch => _isCrouch;
    public bool IsGrounded => _boxCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground"));
    
    public float VelocityY => _rigidbody2D.linearVelocity.y;
    public bool JumpTriggered => _jumpTriggered;

    public bool CanDoubleJump
    {
        get => _doubleJumpAvailable;
        set => _doubleJumpAvailable = value;
    }

    public bool AttackTriggered
    {
        get => _attackTriggered;
        set => _attackTriggered = value;
    }

    public StatBase AttackStat { get; private set; }
    public IDamageable Target { get; private set; }
    public Transform Transform  => transform;

    public bool IsDead
    {
        get => _isDead;
        private set => _isDead = value;
    }

    public Collider2D Collider { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _inputController = GetComponent<InputController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Collider = GetComponent<Collider2D>();
    }

    protected override void Start()
    {
        base.Start();
        PlayerTable playerTable = TableManager.Instance.GetTable<PlayerTable>();
        PlayerSO playerData = playerTable.GetDataByID(0);
        StatManager.Initialize(playerData, null);

        var action = _inputController.PlayerActions;
        action.Move.performed += context => _moveInput = context.ReadValue<Vector2>();
        action.Move.canceled += _ => _moveInput = Vector2.zero;

        action.Crouch.performed += _ => _isCrouch = true;
        action.Crouch.canceled += _ => _isCrouch = false; 
        
        action.Jump.started += _ => _jumpTriggered = true;

        action.Attack.started += _ => _attackTriggered = true;
    }

    protected override void Update()
    {
        base.Update();
        
        Rotate();
        Fall();
        if (JumpTriggered)
        {
            Jump();
            _jumpTriggered = false;
        }
    }

    private void FixedUpdate()
    {
        Movement();
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

            PlayerState.Jump => new JumpState(),
            PlayerState.Fall => new FallState(),
            PlayerState.DoubleJump => new DoubleJumpState(),
            
            PlayerState.Attack => new AttackState(1, 1),
            _ => null
        };
    }

    public override void Movement()
    {
        float speed = StatManager.GetValue(StatType.MoveSpeed);
        _rigidbody2D.linearVelocity =
            new Vector2(MoveInput.x * speed, _rigidbody2D.linearVelocityY);
    }

    public void Rotate()
    {
        if (MoveInput.x > 0.01f)
            _spriteRenderer.flipX = false;
        else if (MoveInput.x < -0.01f)
            _spriteRenderer.flipX = true;
    }

    public void Fall()
    {
        if (VelocityY < 0)
        {
            _rigidbody2D.linearVelocity += Vector2.up * Physics2D.gravity.y * 2f * Time.deltaTime;
        }
        else
        {
            _rigidbody2D.linearVelocity += Vector2.up * Physics2D.gravity.y * 0.5f * Time.deltaTime;
        }
    }

    public void Jump()
    {
        if (IsGrounded)
        {
            _rigidbody2D.AddForceY(StatManager.GetValue(StatType.JumpForce), ForceMode2D.Impulse);
            CanDoubleJump = true;
        } 
        else if (CanDoubleJump)
        {
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocityX, 0);
            _rigidbody2D.AddForceY(StatManager.GetValue(StatType.JumpForce), ForceMode2D.Impulse);
            CanDoubleJump = false;
        }
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

    public void TakeDamage(IAttackable attacker)
    {
        Debug.Log("Player ▶ TakeDamage()");
    }

    public void Dead()
    {
        IsDead = true;
        Debug.Log("Player ▶ Dead()");
    }
}