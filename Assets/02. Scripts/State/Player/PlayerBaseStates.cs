using System.Collections;
using UnityEngine;

public abstract class PlayerGroundState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirParameterHash, false);
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.GroundParameterHash, true);
    }
    public abstract void OnUpdate(PlayerController owner);

    public virtual void OnExit(PlayerController owner)
    {
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
}

public abstract class PlayerAirState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.GroundParameterHash, false);
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirParameterHash, true);
    }
    public abstract void OnUpdate(PlayerController owner);

    public virtual void OnExit(PlayerController owner)
    {
        owner.ComboIndex = 0;
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
}

public abstract class PlayerAttackState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AttackParameterHash, true);
    }
    protected abstract IEnumerator DoAttack(PlayerController owner);
    public virtual void OnUpdate(PlayerController owner) { }

    public virtual void OnExit(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AttackParameterHash, false);
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
    
    protected float GetNormalizedTime(Animator animator, string tag)
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
            
        if (animator.IsInTransition(0) && nextInfo.IsTag(tag))
        {
            return nextInfo.normalizedTime;
        }
        else if (!animator.IsInTransition(0) && currentInfo.IsTag(tag))
        {
            return currentInfo.normalizedTime;
        }
        else
        {
            return 0f;
        }
    }
}

public abstract class PlayerActionState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ActionParameterHash, true);
    }
    public abstract void OnUpdate(PlayerController owner);

    public virtual void OnExit(PlayerController owner)
    {
        owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ActionParameterHash, false);
        owner.ComboIndex = 0;
    }
    public abstract PlayerState CheckTransition(PlayerController owner);
}