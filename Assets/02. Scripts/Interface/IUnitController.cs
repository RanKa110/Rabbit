using System.Collections;
using UnityEngine;

//  Boss, Enemy 핸들러 관련 필요 인터페이스
public interface IUnitController : IAttackable, IDamageable
{
    StatManager StatManager { get; }
    Collider2D Collider { get; }
    IDamageable Target { get; }
    bool IsDead { get; }

    void SetAnimationAttack();
    void SetAnimationMoving(bool isMoving);
    Coroutine StartCoroutine(IEnumerator routine);
}
