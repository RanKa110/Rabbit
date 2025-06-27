using System.Collections;
using UnityEngine;

namespace PlayerActionStates
{
    public class DefenseState : PlayerActionState
    {
        private float _parryTimer = 0.2f;
        private bool _parrySuccess = false;
        private bool _parryDone = false;
        private Coroutine _parryCoroutine;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DefenseParameterHash, true);
            Debug.Log("방어");
            owner.StopMoving();
            _parryCoroutine = owner.StartCoroutine(ParryCoroutine(owner));
        }

        public override void OnUpdate(PlayerController owner)
        {
            if (_parrySuccess)
            {
                owner.Parry();
                _parryDone = true;
            }
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DefenseParameterHash, false);
            owner.DashTriggered = false;
            owner.JumpTriggered = false;
            owner.ComboAttackTriggered = false;
            _parrySuccess = false;
            _parryDone = false;
            //owner.StopCoroutine(_parryCoroutine);
            owner.IsParryingOrDodging = false;
            owner.StartCoroutine(owner.Defense());
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (_parryDone)
                return PlayerState.Idle;
            
            if (owner.IsDefensing)
                return PlayerState.Defense;
            
            return PlayerState.Idle;
        }

        private IEnumerator ParryCoroutine(PlayerController owner)
        {
            Debug.Log("패링 여부 확인 중");
            owner.IsParryingOrDodging = true;
            
            float timer = 0f;
            while (timer < _parryTimer)
            {
                if (owner.TryParrying())
                {
                    _parrySuccess = true;
                    break;
                }
                
                timer += Time.deltaTime;
                yield return null;
            }

            owner.IsParryingOrDodging = false;
            Debug.Log("패링 불가능");
        }
    }
    
    public class DashState : PlayerActionState
    {
        private float _dodgeTimer = 0.2f;
        private bool _dodgeSuccess = false;
        private bool _dodgeDone = false;
        private Coroutine _dodgeCoroutine;
        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DashParameterHash, true);
            _dodgeCoroutine = owner.StartCoroutine(DodgeCoroutine(owner));
            owner.StartCoroutine(owner.Dash());
        }

        public override void OnUpdate(PlayerController owner)
        {
            if (_dodgeSuccess)
            {
                owner.Dodge();
                _dodgeDone = true;
            }
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.DashParameterHash, false);
            owner.DashTriggered = false;
            owner.ComboAttackTriggered = false;
            owner.AirAttackTriggered = false;
            owner.IsDefensing = false;
            _dodgeSuccess = false;
            _dodgeDone = false;
            //owner.StopCoroutine(_dodgeCoroutine);
            owner.IsParryingOrDodging = false;
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (_dodgeDone)
                return PlayerState.Idle;
            
            if (owner.IsDashing)
                return PlayerState.Dash;

            if (!owner.IsGrounded)
                return PlayerState.Fall;

            if (owner.MoveInput.sqrMagnitude > 0.01f)
                return PlayerState.Move;

            return PlayerState.Idle;
        }
        
        private IEnumerator DodgeCoroutine(PlayerController owner)
        {
            Debug.Log("회피 가능 여부 확인 중");
            owner.IsParryingOrDodging = true;

            float timer = 0f;
            while (timer < _dodgeTimer)
            {
                if (owner.TryDodging())
                {
                    _dodgeSuccess = true;
                    break;
                }
                
                timer += Time.deltaTime;
                yield return null;
            }

            owner.IsParryingOrDodging = false;
            Debug.Log("회피 불가능");
        }
    }
}