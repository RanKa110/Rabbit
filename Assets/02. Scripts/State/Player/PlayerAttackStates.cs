using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerAttackStates
{
    public class ComboAttackState : PlayerAttackState
    {
        private AttackInfoData _attackInfoData;
        private Coroutine _attackCoroutine;
        private bool _alreadyAppliedCombo;
        private float timer = 0f;

        public override void OnEnter(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ComboAttackParameterHash, true);
            owner.IsComboAttacking = true;
            owner.StopMoving();
            
            _alreadyAppliedCombo = false;
            _attackInfoData = owner.ComboAttackInfoDatas[owner.ComboIndex];
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            Debug.Log(_attackInfoData.AttackName);
            
            owner.PlayerAnimation.Animator.SetInteger("Combo", owner.ComboIndex);
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.PlayerAnimation.Animator;
            string attackName = _attackInfoData.AttackName;

            yield return new WaitUntil(() =>
                GetNormalizedTime(animator, attackName) >= _attackInfoData.DealingStartTransitionTime);
            owner.AttackAllTargets();

            yield return new WaitUntil(() => _alreadyAppliedCombo || GetNormalizedTime(animator, attackName) >= 1f);

            if (_alreadyAppliedCombo)
            {
                owner.ComboIndex = _attackInfoData.ComboStateIndex != -1 ? _attackInfoData.ComboStateIndex + 1 : 0;
            }
            else
            {
                owner.ComboIndex = 0;
            }

            owner.IsComboAttacking = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
            timer +=  Time.deltaTime;
            TryComboAttack(owner);
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ComboAttackParameterHash, false);
            owner.ComboAttackTriggered = false;
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            owner.IsComboAttacking = false;
            owner.JumpTriggered = false;
            owner.DashTriggered = false;
            timer = 0f;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (!owner.IsComboAttacking)
                return PlayerState.Idle;
            
            if (owner.IsDefensing && owner.CanDefense)
                return PlayerState.Defense;

            return PlayerState.ComboAttack;
        }
        
        private void TryComboAttack(PlayerController owner)
        {
            if (_alreadyAppliedCombo) return;
            if (_attackInfoData.ComboStateIndex == -1) return;
            if (!owner.IsComboAttacking) return;

            if (owner.ComboAttackTriggered && GetNormalizedTime(owner.PlayerAnimation.Animator, _attackInfoData.AttackName) >=
                _attackInfoData.ComboTransitionTime)
            {
                _alreadyAppliedCombo = true;
                owner.ComboAttackTriggered = false;
            }
        }
        
        protected float GetNormalizedTime(Animator animator, string tag)
        {
            return timer;
            // 애니메이션 연결 시 지워야함
            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
            
            if (animator.IsInTransition(0) && nextInfo.IsTag(tag))
            {
                return nextInfo.normalizedTime;
            }
            else if (!animator.IsInTransition(0) && currentInfo.IsTag(tag))
            {
                return currentInfo.normalizedTime;
            }
            else
            {
                return 0f;
            }
        }
    }

    public class AirAttackState : PlayerAttackState
    {
        private AttackInfoData _attackInfoData;
        private Coroutine _attackCoroutine;
        private float timer = 0f;

        public override void OnEnter(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirAttackParameterHash, true);
            owner.IsAirAttacking = true;
            _attackInfoData = owner.AirAttackInfoData;
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            Debug.Log(_attackInfoData.AttackName);
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.PlayerAnimation.Animator;
            string attackName = _attackInfoData.AttackName;

            yield return new WaitUntil(() =>
                GetNormalizedTime(animator, attackName) >= _attackInfoData.DealingStartTransitionTime);
            owner.AttackAllTargets();

            yield return new WaitUntil(() => GetNormalizedTime(animator, attackName) >= 1f);

            owner.IsAirAttacking = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
            timer +=  Time.deltaTime;
        }

        public override void OnExit(PlayerController owner)
        {
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirAttackParameterHash, false);
            owner.AirAttackTriggered = false;
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            owner.IsAirAttacking = false;
            owner.JumpTriggered = false;
            owner.DashTriggered = false;
            owner.ComboIndex = 0;
            timer = 0f;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (!owner.IsAirAttacking)
                return PlayerState.Idle;

            return PlayerState.AirAttack;
        }
        
        protected float GetNormalizedTime(Animator animator, string tag)
        {
            return timer;
            // 애니메이션 연결 시 지워야함
            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
            
            if (animator.IsInTransition(0) && nextInfo.IsTag(tag))
            {
                return nextInfo.normalizedTime;
            }
            else if (!animator.IsInTransition(0) && currentInfo.IsTag(tag))
            {
                return currentInfo.normalizedTime;
            }
            else
            {
                return 0f;
            }
        }
    }
}