using UnityEngine;
using System.Collections;


namespace BossStates
{
    public class IdleState : IState<BossController, BossState>
    {
        public void OnEnter(BossController owner)
        {
            Debug.Log("IdleState.OnEnter");
        }

        public void OnUpdate(BossController owner)
        {
            Debug.Log("IdleState.OnUpdate");
        }

        public void OnExit(BossController entity)
        {
            Debug.Log("IdleState.OnExit");
        }


        public BossState CheckTransition(BossController owner)
        {
            Debug.Log("▶ IdleState.CheckTransition");

            if (owner.Target != null)
            {
                float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.transform.position);

                if (distance <= owner.DetectionRange)
                {
                    return BossState.Chasing;
                }
            }

            return BossState.Idle;
        }
    }

    public class ChasingState : IState<BossController, BossState>
    {
        public void OnEnter(BossController owner)
        {
            Debug.Log("ChasingState.OnEnter");
        }

        public void OnUpdate(BossController owner)
        {
            Debug.Log("ChasingState.OnUpdate → Movement()");
            owner.Movement();
        }

        public void OnExit(BossController entity)
        {
            Debug.Log("ChasingState.OnExit");
        }

        public BossState CheckTransition(BossController owner)
        {
            //  사망 체크 우선
            if (owner.IsDead)
            {
                Debug.Log("→ Chasing → Die");
                return BossState.Die;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.transform.position);

            Debug.Log($"[Chasing.CheckTransition] dist = {distance}, atkRange = {owner.StatManager.GetValue(StatType.AttackRange)}");

            if (distance <= owner.StatManager.GetValue(StatType.AttackRange))
            {
                Debug.Log("→ Chasing → Attack");
                return BossState.Attack;
            }

            return BossState.Chasing;
        }
    }

    //  공격 속도, 사거리, 쿨타임 반영
    public class AttackState : IState<BossController, BossState>
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        //public AttackState()
        //{
        //    _atkSpd = atkSpd;
        //    _atkRange = atkRange;
        //}

        public void OnEnter(BossController owner)
        {
            Debug.Log("보스 공격 상태 진입!");

            float atkSpd = owner.StatManager.GetValue(StatType.AttackSpd);
            float atkRange = owner.StatManager.GetValue(StatType.AttackRange);

            owner.StartCoroutine(DoAttack(owner));
        }

        private IEnumerator DoAttack(BossController owner)
        {
            _attackDone = false;

            yield return new WaitForSeconds(1f / _atkSpd);

            owner.BasicAttack();

            yield return new WaitForSeconds(owner.AttackCooldownValue);

            _attackDone = true;
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController entity)
        {
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            if (!_attackDone)
            {
                return BossState.Attack;
            }

            //  쿨타임 끝나면 랜덤 패턴 혹은 다시 추격
            int idx = Random.Range(0, owner.PatternCount);

            return (BossState)((int)BossState.Pattern1 + idx);
        }
    }

    //  delay 끝나면 다시 추격 시작
    public class PatternState : IState<BossController, BossState>
    {
        private readonly int _index;
        private float _timer;

        public PatternState(int index)
        {
            _index = index;
        }

        public void OnEnter(BossController owner)
        {
            _timer = 0f;
            Debug.Log($"패턴 {_index + 1} 상태 진입!");

            //  TODO: 여기에 패턴 로직을 추가해야 합니다.
        }

        public void OnUpdate(BossController owner)
        {
            _timer += Time.deltaTime;
        }

        public void OnExit(BossController entity)
        {
            Debug.Log($"패턴 {_index + 1} 상태 종료!");
        }

        public BossState CheckTransition(BossController owner)
        {
            if (_timer >= owner.Data.PatternDelays[_index])
            {
                return BossState.Chasing;
            }

            return (BossState)((int)BossState.Pattern1 + _index);
        }
    }

    public class DieState : IState<BossController, BossState>
    {
        public void OnEnter(BossController owner)
        {
            owner.Collider.enabled = false;
            Object.Destroy(owner.gameObject, 2f);
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController entity)
        {
        }

        public BossState CheckTransition(BossController owner)
        {
            return BossState.Die;
        }
    }
}
