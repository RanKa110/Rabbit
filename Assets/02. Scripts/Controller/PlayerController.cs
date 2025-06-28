using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerGroundStates;
using PlayerAirStates;
using PlayerAttackStates;
using PlayerActionStates;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(InputController))]
[RequireComponent(typeof(PlayerAnimation))]
public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable, IDamageable
{
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private PlayerAnimation playerAnimation;

    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D _boxCollider2D;
    private InputController _inputController;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _moveInput;
    private bool _dashTriggered;
    private bool _isDashing;
    private bool _isCrouch;
    private bool _attackTriggered;
    private bool _isAttacking;
    private bool _jumpTriggered;
    private bool _doubleJumpAvailable = true;
    private bool _isDefensing;
    private bool _isDead;
    private bool _tookDamage = false;

    private List<IDamageable> _targets = new List<IDamageable>();
    public int ComboIndex = 0;
    public GameObject HitBox;
    [SerializeField] public List<AttackInfoData> ComboAttackInfoDatas;
    [SerializeField] public AttackInfoData AirAttackInfoData;

    public Vector2 MoveInput => _moveInput;
    public bool IsCrouch => _isCrouch;
    public bool IsGrounded => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    public float VelocityY => _rigidbody2D.linearVelocity.y;
    public bool JumpTriggered { get => _jumpTriggered; set => _jumpTriggered = value; }
    public bool CanDoubleJump { get => _doubleJumpAvailable; set => _doubleJumpAvailable = value; }
    public bool DashTriggered { get => _dashTriggered; set => _dashTriggered = value; }
    public bool IsDashing { get => _isDashing; set => _isDashing = value; }
    public bool IsDefensing { get => _isDefensing; set => _isDefensing = value; }
    public bool CanDash { get; set; } = true;
    public bool CanDefense { get; set; } = true;
    public bool CanAttack { get; set; } = true;
    public bool ComboAttackTriggered { get => _attackTriggered && IsGrounded; set => _attackTriggered = value; }
    public bool IsComboAttacking { get => _isAttacking && IsGrounded; set => _isAttacking = value; }
    public bool AirAttackTriggered { get => _attackTriggered && !IsGrounded; set => _attackTriggered = value; }
    public bool IsAirAttacking { get => _isAttacking && !IsGrounded; set => _isAttacking = value; }
    public bool IsParryingOrDodging { get; set; } = false;
    public StatBase AttackStat { get; private set; }
    public IDamageable Target { get; private set; }
    public Transform Transform => transform;
    public PlayerAnimation PlayerAnimation => playerAnimation;
    public bool IsDead { get => _isDead; private set => _isDead = value; }
    public Collider2D Collider { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
    }

    protected override void Start()
    {
        InitializePlayerData();
        base.Start();
        BindInput();
        InitializeAttackStat();
    }

    protected override void Update()
    {
        base.Update();
        if (_isDashing || _isAttacking || _isDefensing) return;
        Rotate();
    }
    
    private void FixedUpdate()
    {
        if (_isDashing) return;
        if (!CanDash) _dashTriggered = false;
        if (!IsComboAttacking && !_isDefensing) Movement();
        if (!IsGrounded) Fall();
    }

    protected override IState<PlayerController, PlayerState> GetState(PlayerState state)
    {
        return state switch
        {
            PlayerState.Idle => new IdleState(),
            PlayerState.Move => new MoveState(),

            PlayerState.Jump => new JumpState(),
            PlayerState.Fall => new FallState(),
            PlayerState.DoubleJump => new DoubleJumpState(),
            
            PlayerState.ComboAttack => new ComboAttackState(),
            PlayerState.AirAttack => new AirAttackState(),
            
            PlayerState.Defense => new DefenseState(),
            PlayerState.Dash => new DashState(),
            _ => null
        };
    }
    
    private void InitializeComponents()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _inputController = GetComponent<InputController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimation = GetComponent<PlayerAnimation>();
        Collider = GetComponent<Collider2D>();
    }

    private void InitializePlayerData()
    {
        PlayerTable playerTable = TableManager.Instance.GetTable<PlayerTable>();
        PlayerSO playerData = playerTable.GetDataByID(0);
        StatManager.Initialize(playerData, null);
    }

    private void BindInput()
    {
        var action = _inputController.PlayerActions;
        action.Move.performed += context => _moveInput = context.ReadValue<Vector2>();
        action.Move.canceled += _ => _moveInput = Vector2.zero;

        action.Crouch.performed += _ => _isCrouch = true;
        action.Crouch.canceled += _ => _isCrouch = false; 
        
        action.Jump.started += _ => _jumpTriggered = true;

        action.Attack.started += _ =>
        {
            if (CanAttack)
                _attackTriggered = true;
        };
        
        action.Defense.performed += _ => _isDefensing = true;
        action.Defense.canceled += _ => _isDefensing = false;
        
        action.Dash.started += _ => _dashTriggered = true; 
    }

    private void InitializeAttackStat()
    {
        AttackStat = StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
    }

    public override void Movement()
    {
        float speed = StatManager.GetValue(StatType.MoveSpeed);
        _rigidbody2D.linearVelocity =
            new Vector2(MoveInput.x * speed, _rigidbody2D.linearVelocityY);
    }

    public void Rotate()
    {
        const float moveThreshold = 0.01f;
        if (_moveInput.x > moveThreshold)
        {
            _spriteRenderer.flipX = false;
            SetHitBoxPosition(1f);
        }
        else if (_moveInput.x < -moveThreshold)
        {
            _spriteRenderer.flipX = true;
            SetHitBoxPosition(-1f);
        }
    }
    
    private void SetHitBoxPosition(float xPosition)
    {
        Vector3 pos = HitBox.transform.localPosition;
        pos.x = xPosition;
        HitBox.transform.localPosition = pos;
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
        _isDashing = true;

        float originGravity = _rigidbody2D.gravityScale;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.linearVelocity = StatManager.GetValue(StatType.DashForce) *
                                      (_spriteRenderer.flipX ? Vector2.left : Vector2.right);

        yield return new WaitForSeconds(0.2f);

        _rigidbody2D.gravityScale = originGravity;
        _isDashing = false;

        yield return new WaitForSeconds(0.5f);

        CanDash = true;
    }

    public IEnumerator Defense()
    {
        CanDefense = false;
        yield return new WaitForSeconds(0.5f);
        CanDefense = true;
    }

    public void Attack()
    {
        Target?.TakeDamage(this);
    }

    public void AttackAllTargets()
    {
        _attackTriggered = false;
        FindTarget();
        foreach (var target in _targets)
        {
            Target = target;
            Attack();
        }
    }

    public override void FindTarget()
    {
        _targets = HitBox.GetComponent<DamageableSensor>().Damageables;
    }

    public void StopMoving()
    {
        _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocity.y);
    }

    public void Parry()
    {
        Debug.Log("Parry");
    }

    public bool TryParrying() => _tookDamage;

    public void Dodge()
    {
        Debug.Log("Dodge");
    }

    public bool TryDodging() => _tookDamage;

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (IsParryingOrDodging)
        {
            _tookDamage = true;
            return;
        }
        
        Debug.Log("Player ▶ TakeDamage()");
    }

    public void Dead()
    {
        IsDead = true;
        Debug.Log("Player ▶ Dead()");
    }
}