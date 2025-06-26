using UnityEngine;
using System.Collections;
using System;

public class AttackHandler
{
    private readonly IUnitController _unit;
    private readonly Func<IDamageable> _getTargetFunc;
    private readonly Animator _animator;

    public AttackHandler(IUnitController unit, Func<IDamageable> getTargetFunc, Animator animator)
    {
        _unit = unit;
        _getTargetFunc = getTargetFunc;
        _animator = animator;
    }

    public IEnumerator BasicAttackCoroutine(float delayBeforeHit = 0.3f, float postDelay = 0.5f)
    {
        _unit.SetAnimationAttack();
        yield return new WaitForSeconds(delayBeforeHit);

        var target = _getTargetFunc.Invoke();

        if (target != null && !target.IsDead)
        {
            target.TakeDamage(_unit);

            //  보스면 게이지 충전!
            if (_unit is IHasGauge gaugeUnit)
            {
                gaugeUnit.AddBasicGauge();
            }
        }

        yield return new WaitForSeconds(postDelay);

        _animator.ResetTrigger("Attack");
    }
}
