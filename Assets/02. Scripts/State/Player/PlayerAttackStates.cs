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
            owner.IsAttacking = true;
            _alreadyAppliedCombo = false;
            _attackInfoData = owner.ComboAttackInfoDatas[owner.ComboIndex];
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator animator = owner.Animator;
            string attackName = _attackInfoData.AttackName;
            //animator.SetTrigger(attackName);

            yield return new WaitUntil(() =>
                GetNormalizedTime(animator, attackName) >= _attackInfoData.DealingStartTransitionTime);
            owner.AttackAllTargets();

            yield return new WaitUntil(() => _alreadyAppliedCombo || GetNormalizedTime(animator, attackName) >= 1f);

            if (_alreadyAppliedCombo)
            {
                owner.ComboIndex = _attackInfoData.ComboStateIndex != -1 ? _attackInfoData.ComboStateIndex : 0;
            }
            else
            {
                owner.ComboIndex = 0;
            }

            owner.IsAttacking = false;
        }

        public override void OnUpdate(PlayerController owner)
        {
            TryComboAttack(owner);
        }

        public override void OnExit(PlayerController owner)
        {
            owner.AttackTriggered = false;
        }

        public override PlayerState CheckTransition(PlayerController owner)
        {
            
            return PlayerState.Idle;
        }
        
        protected float GetNormalizedTime(Animator animator, string tag)
        {
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

            if (!owner.IsAttacking) return;

            _alreadyAppliedCombo = true;
        }
    }
}