using UnityEngine;

namespace PlayerGroundStates
{
    public class IdleState : PlayerGroundState
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
            return PlayerState.Idle;
        }
    }

    public class MoveState : PlayerGroundState
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
            return PlayerState.Move;
        }
    }
    
    public class CrouchState : PlayerGroundState
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
            return PlayerState.Crouch;
        }
    }
    
    public class RunState : PlayerGroundState
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
            return PlayerState.Run;
        }
    }
}