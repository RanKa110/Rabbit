using UnityEngine;

namespace PlayerActionStates
{
    public class DefenseState : PlayerActionState
    {
        public override void OnEnter(PlayerController owner)
        {
            
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.DashTriggered = false;
            owner.JumpTriggered = false;
            owner.ComboAttackTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.IsDefensing)
                return PlayerState.Defense;
            
            return PlayerState.Idle;
        }
    }
    
    public class ParryingState : PlayerActionState
    {
        public override void OnEnter(PlayerController owner)
        {
            
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.DashTriggered = false;
            owner.JumpTriggered = false;
            owner.ComboAttackTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Idle;
        }
    }
    
    public class DashState : PlayerActionState
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
    
    public class EvasionState : PlayerActionState
    {
        public override void OnEnter(PlayerController owner)
        {
            
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
            owner.DashTriggered = false;
            owner.JumpTriggered = false;
            owner.ComboAttackTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            return PlayerState.Idle;
        }
    }
}