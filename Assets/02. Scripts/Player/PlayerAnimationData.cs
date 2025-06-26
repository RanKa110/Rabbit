using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerAnimationData
{
    [Header("Ground")] 
    [SerializeField] private string groundParameterName = "@Ground";
    [SerializeField] private string idleParameterName = "Idle";
    [SerializeField] private string moveParameterName = "Move";
    
    [Header("Air")]
    [SerializeField] private string airParameterName = "@Air";
    [SerializeField] private string jumpParameterName = "Jump";
    [SerializeField] private string fallParameterName = "Fall";
    [SerializeField] private string doubleJumpParameterName = "DoubleJump";
    
    [Header("Attack")]
    [SerializeField] private string attackParameterName = "@Attack";
    [SerializeField] private string comboAttackParameterName = "ComboAttack";
    [SerializeField] private string airAttackParameterName = "AirAttack";
    
    [Header("Action")]
    [SerializeField] private string actionParameterName = "@Action";
    [SerializeField] private string defenseParameterName = "Defense";
    [SerializeField] private string parryingParameterName = "Parrying";
    [SerializeField] private string evasionParameterName = "Evasion";
    [SerializeField] private string dashParameterName = "Dash";
    
    public int GroundParameterHash { get; private set; }
    public int IdleParameterHash { get; private set; }
    public int MoveParameterHash { get; private set; }
    public int AirParameterHash { get; private set; }
    public int JumpParameterHash { get; private set; }
    public int FallParameterHash { get; private set; }
    public int DoubleJumpParameterHash { get; private set; }
    public int AttackParameterHash { get; private set; }
    public int ComboAttackParameterHash { get; private set; }
    public int AirAttackParameterHash { get; private set; }
    public int ActionParameterHash { get; private set; }
    public int DefenseParameterHash { get; private set; }
    public int ParryingParameterHash { get; private set; }
    public int EvasionParameterHash { get; private set; }
    public int DashParameterHash { get; private set; }

    public void Initialize()
    {
        GroundParameterHash = Animator.StringToHash(groundParameterName);
        IdleParameterHash = Animator.StringToHash(idleParameterName);
        MoveParameterHash = Animator.StringToHash(moveParameterName);
        
        AirParameterHash = Animator.StringToHash(airParameterName);
        JumpParameterHash = Animator.StringToHash(jumpParameterName);
        FallParameterHash = Animator.StringToHash(fallParameterName);
        DoubleJumpParameterHash = Animator.StringToHash(doubleJumpParameterName);
        
        AttackParameterHash = Animator.StringToHash(attackParameterName);
        ComboAttackParameterHash = Animator.StringToHash(comboAttackParameterName);
        AirAttackParameterHash = Animator.StringToHash(airAttackParameterName);

        ActionParameterHash = Animator.StringToHash(actionParameterName);
        DefenseParameterHash = Animator.StringToHash(defenseParameterName);
        ParryingParameterHash = Animator.StringToHash(parryingParameterName);
        EvasionParameterHash = Animator.StringToHash(evasionParameterName);
        DashParameterHash = Animator.StringToHash(dashParameterName);

    }
}