using System;
using UnityEngine;

public interface IState<TOwner, TState> where TOwner : MonoBehaviour where TState : Enum
{
    void OnEnter(TOwner owner);
    void OnUpdate(TOwner owner);
    void OnExit(TOwner entity);
    TState CheckTransition(TOwner owner);
}