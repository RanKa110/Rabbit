using UnityEngine;
using System.Collections;


namespace BossStates
{
    public class IdleState : IState<BossController, BossState>
    {
        public void OnEnter(BossController owner)
        {
            //Debug.Log("IdleState.OnEnter");
        }

        public void OnUpdate(BossController owner)
        {
            //Debug.Log("IdleState.OnUpdate");
        }

        public void OnExit(BossController entity)
        {
            //Debug.Log("IdleState.OnExit");
        }


        public BossState CheckTransition(BossController owner)
        {
            //Debug.Log("▶ IdleState.CheckTransition");

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
            owner.Movement();
            Debug.Log("ChasingState.OnUpdate → Movement()");
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

            //  콜라이더 중심 좌표 구하기
            Vector2 bossCenter = owner.Collider.bounds.center;
            Vector2 playerCenter = owner.Target.Collider.bounds.center;

            //  두 중심 사이 거리 계산
            float distance = Vector2.Distance(bossCenter, playerCenter);

            //  사거리 + 약간의 여유
            float atkRange = owner.StatManager.GetValue(StatType.AttackRange);
            const float epsilon = 0.1f;
            float threshold = atkRange + epsilon;

            Debug.Log($"distance = {distance}, attackRange = {atkRange + epsilon}");

            //  비교
            if (distance <= threshold)
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
        private bool _attackDone;

        public void OnEnter(BossController owner)
        {
            Debug.Log("보스 공격 상태 진입!");

            _attackDone = false;

            float atkSpd = owner.StatManager.GetValue(StatType.AttackSpd);

            owner.StartCoroutine(DoAttack(owner, atkSpd));
        }

        private IEnumerator DoAttack(BossController owner, float atkSpd)
        {
            //  공격 준비
            yield return new WaitForSeconds(1f / atkSpd);

            //  기본 공격
            owner.BasicAttack();
            owner.AddBasicGauge();          //  기본 공격 시, 해당 게이지 차징

            //  쿨타임
            yield return new WaitForSeconds(owner.AttackCooldownValue);

            Debug.Log($"{owner.AttackCooldownValue}");

            _attackDone = true;
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController owner)
        {
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            //   아직 공격 코루틴이 끝나지 않았다면 계속 이 상태
            if (!_attackDone)
            {
                return BossState.Attack;
            }

            //  게이지가 차지 않았다면 쫓아가서 다시 공격
            if (!owner.IsBasicGaugeFull())
            {
                return BossState.Chasing;
            }

            //  게이지가 가득 찼다면 패턴 공격 시행
            //  1. 게이지 초기화
            owner.ResetBasicGauge();

            //  2. 현재 체력 비율 계산
            float curHp = owner.StatManager.GetValue(StatType.CurHp);
            float maxHp = owner.StatManager.GetValue(StatType.MaxHp);
            float hpPercent = curHp / maxHp;

            //  3. HP 구간 별 패턴 풀 크기 결정
            int maxPatternCount;

            if (hpPercent >= 0.7f && hpPercent <= 100f)
            {
                maxPatternCount = 1;        //  패턴 1 시행
            }

            else if (hpPercent >= 0.45f)
            {
                maxPatternCount = 2;        //  패턴 1,2 시행
            }

            else
            {
                maxPatternCount = 3;        //  패턴 1,2,3 시행
            }

            //  4. 풀에서 랜덤 선택
            int idx = Random.Range(0, maxPatternCount);
            Debug.Log($"HP {hpPercent * 100: F0}% → 패턴 {idx + 1} 진입");

            return (BossState)((int)BossState.Pattern1 + idx);
        }
    }

    //  delay 끝나면 다시 추격 시작
    public class PatternState : IState<BossController, BossState>
    {
        private readonly int _index;
        private float _timer;

        public PatternState(int index) => _index = index;

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
