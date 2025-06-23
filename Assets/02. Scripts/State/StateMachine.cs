using System;
using UnityEngine;

public class StateMachine<T, TState> where T : MonoBehaviour where TState : Enum
{
    private T ownerEntity;
    private IState<T, TState> currentState;

    public void Setup(T owner, IState<T, TState> entryState)
    {
        ownerEntity = owner;
        ChangeState(entryState);
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdate(ownerEntity);
        }
    }

    public void ChangeState(IState<T, TState> newState)
    {
        if (newState == null)
            return;


        if (currentState != null)
        {
            currentState.OnExit(ownerEntity);
        }

        currentState = newState;
        currentState.OnEnter(ownerEntity);
    }
}