using System.Collections;
using UnityEngine;

namespace PlayerGroundStates
{
    public class IdleState : PlayerGroundState
    {
        public override void OnEnter(PlayerController owner)
        {
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.JumpTriggered)
                return PlayerState.Jump;
            
            if (owner.VelocityY < 0)
                return PlayerState.Fall;

            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;
            
            if (owner.MoveInput.sqrMagnitude > 0.01f)
                return PlayerState.Move;
            
            return PlayerState.Idle;
        }
    }

    public class MoveState : PlayerGroundState
    {
        public override void OnEnter(PlayerController owner)
        {
            owner.CanDoubleJump = true;
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.VelocityY < 0)
                return PlayerState.Fall;
            
            if (owner.JumpTriggered)
                return PlayerState.Jump;
            
            if (owner.DashTriggered && owner.CanDash)
                return PlayerState.Dash;

            if (owner.MoveInput.sqrMagnitude < 0.01f)
                return PlayerState.Idle;
            
            return PlayerState.Move;
        }
    }
    
    public class CrouchState : PlayerGroundState
    {
        public override void OnEnter(PlayerController owner)
        {
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Crouch;
        }
    }
    
    public class DashState : PlayerGroundState
    {
        public override void OnEnter(PlayerController owner)
        {
            owner.StartCoroutine(owner.Dash());
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.DashTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.IsDashing)
                return PlayerState.Dash;

            if (!owner.IsGrounded)
                return PlayerState.Fall;

            if (owner.MoveInput.sqrMagnitude > 0.01f)
                return PlayerState.Move;

            return PlayerState.Idle;
        }
    }
}