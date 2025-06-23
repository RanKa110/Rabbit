using UnityEngine;

namespace PlayerAirStates
{
    public class JumpState : IState<PlayerController, PlayerState>
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
            return PlayerState.Jump;
        }
    }
    
    public class FallState : IState<PlayerController, PlayerState>
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
            return PlayerState.Fall;
        }
    }
    
    public class DoubleJumpState : IState<PlayerController, PlayerState>
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
            return PlayerState.Jump;
        }
    }
}