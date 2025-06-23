using UnityEngine;

namespace PlayerAirStates
{
    public class JumpState : PlayerAirState
    {
        public void OnEnter(PlayerController owner)
        {
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController owner)
        {
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Jump;
        }
    }
    
    public class FallState : PlayerAirState
    {
        public void OnEnter(PlayerController owner)
        {
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController owner)
        {
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Fall;
        }
    }
    
    public class DoubleJumpState : PlayerAirState
    {
        public void OnEnter(PlayerController owner)
        {
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController owner)
        {
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Jump;
        }
    }
}