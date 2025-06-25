using UnityEngine;

namespace PlayerActionStates
{
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