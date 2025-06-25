using System;
using System.Collections;
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
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    //[SerializeField] private TrailRenderer trailRenderer;
    
    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D _boxCollider2D;
    private InputController _inputController;
    private SpriteRenderer _spriteRenderer;
    
    private Vector2 _moveInput;
    private bool _dashTriggered;
    private bool _isDashing;
    private bool _isCrouch;
    private bool _attackTriggered;
    private bool _jumpTriggered;
    private bool _doubleJumpAvailable = true;

    private List<IDamageable> _targets = new List<IDamageable>();

    private bool _isDead;

    public AudioClip Jumpcilp;

    public Vector2 MoveInput => _moveInput;
    public bool IsCrouch => _isCrouch;
    public bool IsGrounded =>
        Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    
    public float VelocityY => _rigidbody2D.linearVelocity.y;

    public bool JumpTriggered
    {
        get => _jumpTriggered;
        set => _jumpTriggered = value;
    }

    public bool CanDoubleJump
    {
        get => _doubleJumpAvailable;
        set => _doubleJumpAvailable = value;
    }

    public bool DashTriggered
    {
        get => _dashTriggered;
        set => _dashTriggered = value;
    }

    public bool IsDashing
    {
        get => _isDashing;
        set => _isDashing = value;
    }

    public bool CanDash = true;

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
        PlayerTable playerTable = TableManager.Instance.GetTable<PlayerTable>();
        PlayerSO playerData = playerTable.GetDataByID(0);
        StatManager.Initialize(playerData, null);
        
        base.Start();

        var action = _inputController.PlayerActions;
        action.Move.performed += context => _moveInput = context.ReadValue<Vector2>();
        action.Move.canceled += _ => _moveInput = Vector2.zero;

        action.Dash.started += _ => _dashTriggered = true; 

        action.Crouch.performed += _ => _isCrouch = true;
        action.Crouch.canceled += _ => _isCrouch = false; 
        
        action.Jump.started += _ => _jumpTriggered = true;

        action.Attack.started += _ => _attackTriggered = true;
    }

    protected override void Update()
    {
        base.Update();

        if (_isDashing)
            return;
        
        Rotate();
    }
    
    private void FixedUpdate()
    {
        if (_isDashing)
        {
            return;
        }

        if (!CanDash)
            _dashTriggered = false;
        
        Movement();
        if (!IsGrounded)
            Fall();
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
            PlayerState.Dash => new DashState(),

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
            _rigidbody2D.linearVelocity += Vector2.up * Physics2D.gravity.y * 2.3f * Time.fixedDeltaTime;
        }
        else
        {
            _rigidbody2D.linearVelocity += Vector2.up * Physics2D.gravity.y * 0.43f * Time.fixedDeltaTime;
        }
    }

    public void Jump()
    {
        _jumpTriggered = false;
        if (IsGrounded)
        {
            _rigidbody2D.AddForceY(StatManager.GetValue(StatType.JumpForce), ForceMode2D.Impulse);
        } 
        else if (_doubleJumpAvailable)
        {
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocityX, 0);
            _rigidbody2D.AddForceY(StatManager.GetValue(StatType.JumpForce), ForceMode2D.Impulse);
            _doubleJumpAvailable = false;
        }
    }

    public IEnumerator Dash()
    {
        CanDash = false;
        IsDashing = true;

        float originGravity = _rigidbody2D.gravityScale;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.linearVelocity = StatManager.GetValue(StatType.DashForce) *
                                      (_spriteRenderer.flipX ? Vector2.left : Vector2.right);

        yield return new WaitForSeconds(0.2f);

        _rigidbody2D.gravityScale = originGravity;
        IsDashing = false;

        yield return new WaitForSeconds(1.5f);

        CanDash = true;
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