using UnityEngine;

namespace PlayerAirStates
{
    public class JumpState : PlayerAirState
    {
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.JumpParameterHash, true);
            owner.Jump();
            owner.CanDoubleJump = true;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.JumpParameterHash, false);
            owner.JumpTriggered = false;
            owner.AirAttackTriggered = false;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.AirAttackTriggered)
                return PlayerState.AirAttack;
            
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
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.FallParameterHash, true);
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.FallParameterHash, false);
            owner.JumpTriggered = false;
            owner.AirAttackTriggered = false;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.AirAttackTriggered)
                return PlayerState.AirAttack;

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
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DoubleJumpParameterHash, true);
            owner.Jump();
            owner.CanDoubleJump = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DoubleJumpParameterHash, false);
            owner.JumpTriggered = false;
            owner.AirAttackTriggered = false;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.AirAttackTriggered)
                return PlayerState.AirAttack;

            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.VelocityY < 0)
                return PlayerState.Fall;
            
            if (owner.IsGrounded)
                return PlayerState.Idle;

            return PlayerState.DoubleJump;
        }
    }
}