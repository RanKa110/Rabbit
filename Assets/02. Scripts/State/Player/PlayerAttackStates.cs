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
        private float _time = 0f;

        public override void OnEnter(PlayerController owner)
        {
            owner.IsComboAttacking = true;
            owner.StopMoving();
            _alreadyAppliedCombo = false;
            _attackInfoData = owner.ComboAttackInfoDatas[owner.ComboIndex];
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
            Debug.Log(_attackInfoData.AttackName);
            _time = 0f;
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.Animator;
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
            _time += Time.deltaTime;
            TryComboAttack(owner);
        }

        public override void OnExit(PlayerController owner)
        {
            owner.ComboAttackTriggered = false;
            if (_attackCoroutine != null)
                owner.StopCoroutine(_attackCoroutine);
            owner.IsComboAttacking = false;
            owner.JumpTriggered = false;
            owner.DashTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            if (!owner.IsComboAttacking)
                return PlayerState.Idle;

            return PlayerState.ComboAttack;
        }
        
        protected float GetNormalizedTime(Animator animator, string tag)
        {
            // 애니메이션 연결할 때 지워야함
            return _time;
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
        
        private void TryComboAttack(PlayerController owner)
        {
            if (_alreadyAppliedCombo) return;
            if (_attackInfoData.ComboStateIndex == -1) return;
            if (!owner.IsComboAttacking) return;

            if (owner.ComboAttackTriggered && GetNormalizedTime(owner.Animator, _attackInfoData.AttackName) >=
                _attackInfoData.ComboTransitionTime)
            {
                _alreadyAppliedCombo = true;
                owner.ComboAttackTriggered = false;
            }
        }
    }
}