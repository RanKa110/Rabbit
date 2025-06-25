using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerAttackStates
{
    public class ComboAttackState : PlayerAttackState
    {
        private readonly AttackInfoData _attackInfoDatas;
        private Coroutine _attackCoroutine;

        public ComboAttackState(AttackInfoData attackInfoDatas)
        {
            _attackInfoDatas = attackInfoDatas;
            Debug.Log(attackInfoDatas.AttackName);
        }

        public override void OnEnter(PlayerController owner)
        {
            _attackCoroutine = owner.StartCoroutine(DoAttack(owner));
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            Animator anim = owner.Animator;
            
            yield return new WaitUntil(() => GetNormalizedTime(anim, "Attack") >= _attackInfoDatas.DealingStartTransitionTime);
            owner.AttackAllTargets();
            yield return new WaitUntil(() => GetNormalizedTime(anim, "Attack") >= _attackInfoDatas.DealingEndTransitionTime);
            
            yield return new WaitUntil(() => GetNormalizedTime(anim, "Attack") >= _attackInfoDatas.ComboTransitionTime);
        }

        public override void OnUpdate(PlayerController owner)
        {
        }

        public override void OnExit(PlayerController owner)
        {
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
    }
}