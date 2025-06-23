using UnityEngine;

namespace PlayerGroundedStates
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