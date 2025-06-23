using UnityEngine;
using System.Collections;

namespace EnemyStates
{
    public class IdleState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
            // 이동 정지
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            // 애니메이션
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
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
            // 애니메이션
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isMoving", true);
            }
        }

        public void OnUpdate(EnemyController owner)
        {
            // 몬스터 타입에 따른 이동
            if (owner.monsterType == EnemyController.MonsterType.Ranged || 
                owner.monsterType == EnemyController.MonsterType.Homing)
            {
                owner.MovementWithDistance(owner.minAttackDistance);
            }
            else
            {
                owner.Movement();
            }
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

    // 근거리 공격 상태
    public class MeleeAttackState : IState<EnemyController, EnemyState>
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        public MeleeAttackState(float atkSpd, float atkRange)
        {
            _atkSpd = atkSpd;
            _atkRange = atkRange;
        }

        public void OnEnter(EnemyController owner)
        {
            owner.StartCoroutine(DoMeleeAttack(owner));
        }

        private IEnumerator DoMeleeAttack(EnemyController owner)
        {
            _attackDone = false;
            owner.IsAttacking = true;
            owner.CanAttack = false;
            owner.LastAttackTime = Time.time;
            
            // 이동 정지
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            // 공격 애니메이션 트리거
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // 공격 애니메이션 대기
            yield return new WaitForSeconds(0.3f);
            
            // 근거리 공격 실행
            owner.MeleeAttack();
            
            // 공격 애니메이션 종료 대기
            yield return new WaitForSeconds(0.2f);
            
            owner.IsAttacking = false;
            
            // 공격 속도에 따른 대기
            yield return new WaitForSeconds(1f / _atkSpd);
            
            owner.CanAttack = true;
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

    // 원거리 공격 상태
    public class RangedAttackState : IState<EnemyController, EnemyState>
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        public RangedAttackState(float atkSpd, float atkRange)
        {
            _atkSpd = atkSpd;
            _atkRange = atkRange;
        }

        public void OnEnter(EnemyController owner)
        {
            owner.StartCoroutine(DoRangedAttack(owner));
        }

        private IEnumerator DoRangedAttack(EnemyController owner)
        {
            _attackDone = false;
            owner.IsAttacking = true;
            owner.CanAttack = false;
            owner.LastAttackTime = Time.time;
            
            // 이동 정지
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            // 공격 애니메이션 트리거
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // 애니메이션 대기
            yield return new WaitForSeconds(0.3f);
            
            // 투사체 발사
            owner.RangedAttack();
            
            // 공격 애니메이션 종료 대기
            yield return new WaitForSeconds(0.2f);
            
            owner.IsAttacking = false;
            
            // 공격 속도에 따른 대기
            yield return new WaitForSeconds(1f / _atkSpd);
            
            owner.CanAttack = true;
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

            // 타겟과의 거리 확인
            if (owner.Target != null)
            {
                float distance = Vector3.Distance(owner.transform.position, owner.Target.Collider.transform.position);
                
                // 타겟이 너무 가까우면 추격 상태로 (거리 조절)
                if (distance < owner.minAttackDistance)
                {
                    return EnemyState.Chasing;
                }
                
                // 타겟이 범위를 벗어났으면 추격
                if (distance > _atkRange)
                {
                    return EnemyState.Chasing;
                }
                
                // 범위 내에 있으면 다시 공격
                return EnemyState.Attack;
            }

            return EnemyState.Chasing;
        }
    }

    // 유도 공격 상태
    public class HomingAttackState : IState<EnemyController, EnemyState>
    {
        private readonly float _atkSpd;
        private readonly float _atkRange;
        private bool _attackDone;

        public HomingAttackState(float atkSpd, float atkRange)
        {
            _atkSpd = atkSpd;
            _atkRange = atkRange;
        }

        public void OnEnter(EnemyController owner)
        {
            owner.StartCoroutine(DoHomingAttack(owner));
        }

        private IEnumerator DoHomingAttack(EnemyController owner)
        {
            _attackDone = false;
            owner.IsAttacking = true;
            owner.CanAttack = false;
            owner.LastAttackTime = Time.time;
            
            // 이동 정지
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            
            // 공격 애니메이션 트리거
            var animator = owner.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // 애니메이션 대기
            yield return new WaitForSeconds(0.3f);
            
            // 유도 투사체 발사
            owner.HomingAttack();
            
            // 공격 애니메이션 종료 대기
            yield return new WaitForSeconds(0.2f);
            
            owner.IsAttacking = false;
            
            // 공격 속도에 따른 대기 (유도 미사일은 좀 더 긴 쿨다운)
            yield return new WaitForSeconds(1.5f / _atkSpd);
            
            owner.CanAttack = true;
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

            // 타겟과의 거리 확인
            if (owner.Target != null)
            {
                float distance = Vector3.Distance(owner.transform.position, owner.Target.Collider.transform.position);
                
                // 타겟이 너무 가까우면 추격 상태로 (거리 조절)
                if (distance < owner.minAttackDistance)
                {
                    return EnemyState.Chasing;
                }
                
                // 타겟이 범위를 벗어났으면 추격
                if (distance > _atkRange)
                {
                    return EnemyState.Chasing;
                }
                
                // 범위 내에 있으면 다시 공격
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
            
            // CharacterController가 있으면 비활성화
            var characterController = owner.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            // Rigidbody2D가 있으면 정지
            var rb = owner.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
            
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