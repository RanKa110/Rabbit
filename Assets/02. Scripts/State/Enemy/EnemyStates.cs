using UnityEngine;
using System.Collections;

namespace EnemyStates
{
    public class IdleState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
            // 이동 정지 - X축 속도를 0으로
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
            // 몬스터 컴포넌트에 따른 이동
            var rangedMonster = owner.GetComponent<RangedMonster>();
            var homingMonster = owner.GetComponent<HomingMonster>();
            
            if (rangedMonster != null || homingMonster != null)
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

    // 통합 공격 상태 (각 몬스터 컴포넌트로 위임)
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
            owner.IsAttacking = true;
            owner.CanAttack = false;
            owner.LastAttackTime = Time.time;
            
            // 이동 정지 - X축 속도를 0으로
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
            
            // 몬스터 타입별 공격 로직을 각 몬스터 스크립트로 위임
            var meleeMonster = owner.GetComponent<MeleeMonster>();
            var rangedMonster = owner.GetComponent<RangedMonster>();
            var homingMonster = owner.GetComponent<HomingMonster>();
            
            if (meleeMonster != null)
            {
                // 근거리 공격: 범위 내 플레이어 탐지 후 Attack 호출
                if (owner.attackPoint != null)
                {
                    Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(owner.attackPoint.position, owner.attackRadius, owner.playerLayer);
                    
                    foreach (Collider2D hit in hitPlayers)
                    {
                        if (hit.CompareTag("Player"))
                        {
                            IDamageable damageable = hit.GetComponent<IDamageable>();
                            if (damageable != null)
                            {
                                // 타겟에 저장하고 Attack 호출
                                owner.SetAttackTarget(damageable);
                                owner.Attack();
                            }
                            
                            // 넉백 효과
                            Vector2 knockbackDir = (hit.transform.position - owner.transform.position).normalized;
                            Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                            if (playerRb != null)
                            {
                                playerRb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
                            }
                        }
                    }
                }
            }
            else if (rangedMonster != null)
            {
                // 원거리 공격: RangedMonster에게 위임
                rangedMonster.currentState = MonsterBase.MonsterState.Attack;
            }
            else if (homingMonster != null)
            {
                // 유도 공격: HomingMonster에게 위임
                homingMonster.PerformHomingAttack();
            }
            
            // 공격 애니메이션 종료 대기
            yield return new WaitForSeconds(0.2f);
            
            owner.IsAttacking = false;
            
            // 공격 속도에 따른 대기
            float cooldown = 1f / _atkSpd;
            if (homingMonster != null)
            {
                cooldown *= 1.5f; // 유도 미사일은 더 긴 쿨다운
            }
            
            yield return new WaitForSeconds(cooldown);
            
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
                
                // 원거리/유도 몬스터의 경우 최소 거리 체크
                var rangedMonster = owner.GetComponent<RangedMonster>();
                var homingMonster = owner.GetComponent<HomingMonster>();
                
                if ((rangedMonster != null || homingMonster != null) && distance < owner.minAttackDistance)
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
            owner.GetComponent<Collider2D>().enabled = false;
            
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