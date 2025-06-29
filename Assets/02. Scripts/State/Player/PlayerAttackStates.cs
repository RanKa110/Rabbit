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

        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetInteger("Combo", owner.ComboIndex);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ComboAttackParameterHash, true);

            owner.IsComboAttacking = true;
            owner.ComboAttackTriggered = false;
            owner.CanAttack = false;
            owner.StopMoving();
            
            _alreadyAppliedCombo = false;
            _attackInfoData = owner.ComboAttackInfoDatas[owner.ComboIndex];
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            Debug.Log(_attackInfoData.AttackName);
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.PlayerAnimation.Animator;
            string attackName = "Attack";

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
            owner.PlayerAnimation.Animator.SetInteger("Combo", owner.ComboIndex);
        }

        public override void OnUpdate(PlayerController owner)
        {
            TryComboAttack(owner);
        }

        public override void OnExit(PlayerController owner)
        {
            if (!_alreadyAppliedCombo)
                owner.ComboIndex = 0;
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.ComboAttackParameterHash, false);
            owner.ComboAttackTriggered = false;
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            owner.CanAttack = true;
            owner.IsComboAttacking = false;
            owner.JumpTriggered = false;
            owner.DashTriggered = false;
            owner.StartCoroutine(owner.AttackCounter());
            base.OnExit(owner);
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
            float normalizedTime = GetNormalizedTime(owner.PlayerAnimation.Animator, "Attack");
    
            if (normalizedTime < _attackInfoData.DealingStartTransitionTime) return;
            if (normalizedTime > _attackInfoData.DealingEndTransitionTime) return;
            owner.CanAttack = true;
            if (_attackInfoData.ComboStateIndex == -1) return;
            if (!owner.ComboAttackTriggered) return;

            _alreadyAppliedCombo = true;
            owner.ComboAttackTriggered = false;
            owner.ComboIndex = _attackInfoData.ComboStateIndex + 1;
        }
    }

    public class AirAttackState : PlayerAttackState
    {
        private AttackInfoData _attackInfoData;
        private Coroutine _attackCoroutine;

        public override void OnEnter(PlayerController owner)
        {
            base.OnEnter(owner);
            owner.PlayerAnimation.Animator.SetBool(owner.PlayerAnimation.AnimationData.AirAttackParameterHash, true);
            owner.IsAirAttacking = true;
            _attackInfoData = owner.AirAttackInfoData;
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            Debug.Log(_attackInfoData.AttackName);
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.PlayerAnimation.Animator;
            string attackName = "Attack";

            yield return new WaitUntil(() =>
                GetNormalizedTime(animator, attackName) >= _attackInfoData.DealingStartTransitionTime);
            owner.AttackAllTargets();

            yield return new WaitUntil(() => GetNormalizedTime(animator, attackName) >= 1f);

            owner.IsAirAttacking = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
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
            base.OnExit(owner);
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (!owner.IsAirAttacking)
                return PlayerState.Idle;

            return PlayerState.AirAttack;
        }
    }
}