using UnityEngine;

namespace PlayerAirStates
{
    public class JumpState : PlayerAirState
    {
        public override void OnEnter(PlayerController owner)
        {
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
            if (owner.AirAttackTriggered)
                return PlayerState.AirAttack;
            
            if (owner.ParryingTriggered)
                return PlayerState.Parrying;
            
            if (owner.JumpTriggered && owner.CanDoubleJump)
                return PlayerState.DoubleJump;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
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
            if (owner.AirAttackTriggered)
                return PlayerState.AirAttack;
            
            if (owner.ParryingTriggered)
                return PlayerState.Parrying;

            if (owner.JumpTriggered && owner.CanDoubleJump)
                return PlayerState.DoubleJump;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
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
            if (owner.AirAttackTriggered)
                return PlayerState.AirAttack;
            
            if (owner.ParryingTriggered)
                return PlayerState.Parrying;

            if (owner.VelocityY < 0)
                return PlayerState.Fall;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.IsGrounded)
                return PlayerState.Idle;

            return PlayerState.DoubleJump;
        }
    }
}