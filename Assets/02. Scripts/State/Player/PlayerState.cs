using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public class IdleState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController owner)
        {
        }

        public void OnUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController owner)
        {
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Idle;
        }
    }

    public class MoveState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController owner)
        {
        }

        public void OnUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController owner)
        {
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Move;
        }
    }

    public class AttackState : IState<PlayerController, PlayerState>
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        public AttackState(float atkSpd, float atkRange)
        {
            this._atkSpd = atkSpd;
            this._atkRange = atkRange;
        }

        public void OnEnter(PlayerController owner)
        {
        }

        private IEnumerator DoAttack(PlayerController owner)
        {
            yield return new WaitForSeconds(1f / _atkSpd);
            owner.AttackAllTargets();
            _attackDone = true;
        }

        public void OnUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController owner)
        {
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Idle;
        }
    }

    public class RunState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController owner)
        {
        }

        public void OnUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController owner)
        {
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Run;
        }
    }
}