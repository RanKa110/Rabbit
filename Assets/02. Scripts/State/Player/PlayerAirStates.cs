using UnityEngine;

namespace PlayerAirStates
{
    public class JumpState : PlayerAirState
    {
        public override void OnEnter(PlayerController owner)
        {
            Debug.Log("Jump");
            owner.Jump();
            owner.CanDoubleJump = true;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.JumpTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.JumpTriggered && owner.CanDoubleJump)
                return PlayerState.DoubleJump;
            
            if (owner.VelocityY < 0)
                return PlayerState.Fall;

            return PlayerState.Jump;
        }
    }
    
    public class FallState : PlayerAirState
    {
        public override void OnEnter(PlayerController owner)
        {
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.JumpTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.JumpTriggered && owner.CanDoubleJump)
                return PlayerState.DoubleJump;
            
            if (owner.IsGrounded)
                return PlayerState.Idle;
            
            return PlayerState.Fall;
        }
    }
    
    public class DoubleJumpState : PlayerAirState
    {
        public override void OnEnter(PlayerController owner)
        {
            owner.Jump();
            owner.CanDoubleJump = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.JumpTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.VelocityY < 0)
                return PlayerState.Fall;
            
            if (owner.IsGrounded)
                return PlayerState.Idle;

            return PlayerState.DoubleJump;
        }
    }
}