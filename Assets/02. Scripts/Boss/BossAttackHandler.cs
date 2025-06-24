using UnityEngine;
using System.Collections;

public class BossAttackHandler
{
    private readonly IDamageable _target;
    private readonly IAttackable _attacker;
    private readonly MonoBehaviour monoBehaviour;

    public BossAttackHandler(IDamageable target, IAttackable attacker, MonoBehaviour mono)
    {
        _target = target;
        _attacker = attacker;
        monoBehaviour = mono;
    }

    public IEnumerator BasicAttackCoroutine(GaugeManager gauge)
    {
        if (_target == null || _target.IsDead)
        {
            yield break;
        }

        Debug.Log("보스 기본 공격 시작");
        _target.TakeDamage(_attacker);

        yield return new WaitForSeconds(0.1f);
        gauge.Add();
    }
}
