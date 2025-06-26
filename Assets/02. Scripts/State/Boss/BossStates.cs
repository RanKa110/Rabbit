using UnityEngine;
using System.Collections;
using System.Threading;


namespace BossStates
{
    public class IdleState : IState<BossController, BossState>
    {
        public void OnEnter(BossController owner)
        {
            owner.SetAnimationMoving(false);
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

            owner.SetAnimationMoving(true);
            owner.FaceToTarget();
        }

        public void OnUpdate(BossController owner)
        {
            Debug.Log("보스 추격 시행 중..");

            //  사거리 안이면 바로 멈춤
            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);

            if (distance <= owner.AttackRange)
            {
                owner.MovementHandler.StopXMovement();
                owner.SetAnimationMoving(false);
                return;
            }

            owner.FaceToTarget();
            owner.Movement();
        }

        public void OnExit(BossController owner)
        {
            Debug.Log("ChasingState.OnExit");
            owner.SetAnimationMoving(false);
        }

        public BossState CheckTransition(BossController owner)
        {
            //  사망 체크 우선
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            if (owner.Target == null || owner.Target.IsDead)
            {
                return BossState.Idle;
            }

            float distance = Vector2.Distance(owner.transform.position, owner.Target.Collider.bounds.center);

            if (distance <= owner.AttackRange)
            {
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

            //  공격 시작 직전까지 추격 / 이동 잠금
            owner.SetCanMove(false);
            owner.MovementHandler.StopXMovement();
            owner.SetAnimationMoving(false);

            //  플레이어를 바라보는건 유지
            owner.FaceToTarget();

            _attackDone = false;
            owner.StartCoroutine(DoAttack(owner));
        }

        private IEnumerator DoAttack(BossController owner)
        {
            float attackSpeed = owner.StatManager.GetValue(StatType.AttackSpd);

            yield return new WaitForSeconds(1f / attackSpeed);

            yield return owner.StartCoroutine(owner.AttackHandler.BasicAttackCoroutine(delayBeforeHit: 0.2f, postDelay: owner.AttackCooldown));

            _attackDone = true;
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController owner)
        {
            owner.SetCanMove(true);
            owner.SetAnimationMoving(true);
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
            //  공격 시작 직전까지 추격 / 이동 잠금
            owner.SetCanMove(false);
            owner.MovementHandler.StopXMovement();

            _timer = 0f;
            _patternStarted = false;
            owner.SetAnimationMoving(false);

            Debug.Log($"패턴 {_index + 1} 상태 진입!");

            //  TODO: 여기에 패턴 로직을 추가할 예정
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
            yield return new WaitForSeconds(0.5f);     
            Debug.Log($"패턴 {_index + 1} 실행 중..");
        }

        public void OnExit(BossController owner)
        {
            Debug.Log($"패턴 {_index + 1} 상태 종료!");

            owner.SetCanMove(true);
            owner.SetAnimationMoving(true);
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

    public class EvadeState : IState<BossController, BossState>
    {
        private bool _done;

        public void OnEnter(BossController owner)
        {
            owner.SetAnimationMoving(false);

            _done = false;

            owner.StartCoroutine(DoEvade(owner));
        }

        private IEnumerator DoEvade(BossController owner)
        {
            Debug.Log("보스 회피 시작");

            Vector2 start = owner.transform.position;

            Vector2 targetPos = (Vector2)owner.Target.Collider.bounds.center;

            Vector2 end = start + targetPos * 3f;

            float time = 0f;
            float duration = 0.15f;

            while (time < 1f)
            {
                time += Time.deltaTime / duration;
                owner.transform.position = Vector2.Lerp(start, end, time);
                yield return null;
            }

            _done = true;
        }

        public void OnUpdate(BossController owner)
        {
        }

        public void OnExit(BossController owner)
        {
            Debug.Log("보스 회피 상태 종료");
            owner.SetAnimationMoving(true);
        }

        public BossState CheckTransition(BossController owner)
        {
            if (owner.IsDead)
            {
                return BossState.Die;
            }

            if (_done)
            {
                return BossState.Chasing;   //  회피 후 다시 추격 상태로!
            }

            return BossState.Evade;
        }
    }

    public class DieState : IState<BossController, BossState>
    {
        public void OnEnter(BossController owner)
        {
            owner.Collider.enabled = false;
            owner.SetAnimationDeath();
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
