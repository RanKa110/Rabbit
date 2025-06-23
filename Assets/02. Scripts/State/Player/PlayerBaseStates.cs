using System.Collections;
using UnityEngine;

public abstract class PlayerGroundedState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner) { }
    public abstract void OnUpdate(PlayerController owner);
    public virtual void OnExit(PlayerController owner) { }
    public abstract PlayerState CheckTransition(PlayerController owner);
}

public abstract class PlayerAirState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner) { }
    public abstract void OnUpdate(PlayerController owner);
    public virtual void OnExit(PlayerController owner) { }
    public abstract PlayerState CheckTransition(PlayerController owner);
}

public abstract class PlayerAttackState : IState<PlayerController, PlayerState>
{
    public virtual void OnEnter(PlayerController owner) { }
    protected abstract IEnumerator DoAttack(PlayerController owner);
    public virtual void OnUpdate(PlayerController owner) { }
    public virtual void OnExit(PlayerController owner) { }
    public abstract PlayerState CheckTransition(PlayerController owner);
}