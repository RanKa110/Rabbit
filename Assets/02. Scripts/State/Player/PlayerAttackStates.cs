using System.Collections;
using UnityEngine;

namespace PlayerAttackStates
{
    public class AttackState : PlayerAttackState
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        public AttackState(float atkSpd, float atkRange)
        {
            this._atkSpd = atkSpd;
            this._atkRange = atkRange;
        }

        public void OnEnter(PlayerController owner)
        {
        }

        protected override IEnumerator DoAttack(PlayerController owner)
        {
            yield return new WaitForSeconds(1f / _atkSpd);
            owner.AttackAllTargets();
            _attackDone = true;
        }

        public void OnUpdate(PlayerController owner)
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
}