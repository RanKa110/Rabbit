using UnityEngine;
using System.Collections;

namespace EnemyStates
{
    public class IdleState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
        }

        public void OnUpdate(EnemyController owner)
        {
        }

        public void OnExit(EnemyController owner)
        {
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.Target != null && Vector3.Distance(owner.transform.position, owner.Target.Collider.transform.position) <= owner.Data.DetectionRange)
            {
                return EnemyState.Chasing;
            }

            return EnemyState.Idle;
        }
    }

    public class ChasingState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
        }

        public void OnUpdate(EnemyController owner)
        {
            owner.Movement();
        }

        public void OnExit(EnemyController entity)
        {
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.IsDead)
            {
                return EnemyState.Die;
            }

            if (owner.Target == null)
            {
                return EnemyState.Idle;
            }

            float distance = Vector3.Distance(owner.transform.position, owner.Target.Collider.transform.position);

            if (distance <= owner.StatManager.GetValue(StatType.AttackRange))
            {
                return EnemyState.Attack;
            }

            return EnemyState.Chasing;
        }
    }

    public class AttackState : IState<EnemyController, EnemyState>
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        public AttackState(float atkSpd, float atkRange)
        {
            _atkSpd = atkSpd;
            _atkRange = atkRange;
        }

        public void OnEnter(EnemyController owner)
        {
            owner.StartCoroutine(DoAttack(owner));
        }


        private IEnumerator DoAttack(EnemyController owner)
        {
            _attackDone = false;

            yield return new WaitForSeconds(1f / _atkSpd);

            owner.Attack();

            _attackDone = true;
        }

        public void OnUpdate(EnemyController owner)
        {
        }

        public void OnExit(EnemyController owner)
        {
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.IsDead)
            {
                return EnemyState.Die;
            }

            if (!_attackDone)
            {
                return EnemyState.Attack;
            }

            return EnemyState.Chasing;
        }
    }

    public class DieState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
            owner.GetComponent<Collider>().enabled = false;
            Object.Destroy(owner.gameObject, 1f);
        }

        public void OnUpdate(EnemyController owner)
        {
        }

        public void OnExit(EnemyController owner)
        {
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            return EnemyState.Die;
        }
    }
}
