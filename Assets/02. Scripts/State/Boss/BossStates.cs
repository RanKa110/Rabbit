using UnityEngine;
using System.Collections;


namespace BossStates
{
    public class IdleState : IState<BossController, BossState>
    {
        public void OnEnter(BossController owner)
        {
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController entity)
        {
        }


        public BossState CheckTransition(BossController owner)
        {

            if (owner.Target == null || owner.Target.IsDead)
            {
                return BossState.Idle;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.transform.position);

            if (distance <= owner.DetectionRange)
            {
                return BossState.Chasing;
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
            Debug.Log("보스 추격 시행 중..");
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

            if (owner.Target == null || owner.Target.IsDead)
            {
                return BossState.Idle;
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

            //Debug.Log($"distance = {distance}, attackRange = {atkRange + epsilon}");

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
            owner.Attack();

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

            //  패턴 진입 로직
            owner.ResetBasicGauge();

            float curHp = owner.StatManager.GetValue(StatType.CurHp);
            float maxHp = owner.StatManager.GetValue(StatType.MaxHp);
            float hpRatio = curHp / maxHp;

            int maxPatternIndex = GetMaxPatternIndex(hpRatio);
            int selectedIndex = Random.Range(0, maxPatternIndex);
            Debug.Log($"HP {hpRatio * 100}% → 패턴 {selectedIndex + 1} 진입");

            return BossState.Pattern1 + selectedIndex;
        }

        private int GetMaxPatternIndex(float hpRatio)
        {
            if (hpRatio >= 0.7f)
            {
                return 1;
            }

            if (hpRatio >= 0.45f)
            {
                return 2;
            }

            return 3;
        }
    }

    //  delay 끝나면 다시 추격 시작
    public class PatternState : IState<BossController, BossState>
    {
        private readonly int _index;
        private float _timer;
        private bool _patternStarted;

        public PatternState(int index) => _index = index;

        public void OnEnter(BossController owner)
        {
            _timer = 0f;
            _patternStarted = false;
            Debug.Log($"패턴 {_index + 1} 상태 진입!");

            //  TODO: 여기에 패턴 로직을 추가해야 합니다.
        }

        public void OnUpdate(BossController owner)
        {
            if (!_patternStarted)
            {
                _patternStarted = true;
                owner.StartCoroutine(RunPattern(owner));
            }

            _timer += Time.deltaTime;
        }

        private IEnumerator RunPattern(BossController owner)
        {
            //  여기에 실제 패턴 공격 로직 삽입할 것
            yield return new WaitForSeconds(0.5f);      //  예시용 준비 시간
            Debug.Log($"패턴 {_index + 1} 실행 중..");
        }

        public void OnExit(BossController entity)
        {
            Debug.Log($"패턴 {_index + 1} 상태 종료!");
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            if (_timer >= owner.GetPatternDelay(_index))
            {
                return BossState.Chasing;
            }


            return BossState.Pattern1 + _index;
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
