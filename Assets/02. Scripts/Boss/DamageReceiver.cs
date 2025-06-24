using UnityEngine;
using System.Collections;
using Unity.Hierarchy;

public class DamageReceiver : IDamageable
{
    private readonly StatManager _statManager;
    private readonly float _parryChance;

    private readonly IEnumerator _onDeathCoroutine;
    private readonly MonoBehaviour _coroutineHost;

    private bool _isDead;

    public bool IsDead => _isDead;
    public Collider2D Collider { get; }

    public DamageReceiver(StatManager statManager, Collider2D collider, float parryChance, IEnumerator onDeathCoroutine, MonoBehaviour coroutineHost)
    {
        _statManager = statManager;
        _parryChance = parryChance;
        Collider = collider;
        _onDeathCoroutine = onDeathCoroutine;
        _coroutineHost = coroutineHost;
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (_isDead)
        {
            return;
        }

        if (Random.value < _parryChance)
        {
            Debug.Log("패링 성공! 데미지 무시!");
            return;
        }

        float damage = attacker.AttackStat.Value;
        _statManager.Consume(StatType.CurHp, StatModifierType.Base, damage);

        Debug.Log($"데미지: {damage} | 남은 HP: {_statManager.GetValue(StatType.CurHp)}");

        TryTriggerEvade();      //  회피 조건 처리 메서드

        if (_statManager.GetValue(StatType.CurHp) <= 0f)
        {
            _isDead = true;

            if (_onDeathCoroutine != null && _coroutineHost != null)
            {
                _coroutineHost.StartCoroutine(_onDeathCoroutine);
            }
        }
    }

    private void TryTriggerEvade()
    {
        float hpRatio = _statManager.GetValue(StatType.CurHp) / _statManager.GetValue(StatType.MaxHp);
        float evadeChance = 0.25f;

        if (hpRatio < 0.6f && Random.value < evadeChance)
        {
            Debug.Log("회피 조건 만족 → 회피 시행!");

            if (_coroutineHost is BossController boss)
            {
                boss.RequestEvade();
            }
        }
    }

    public void Dead()
    {
    }
}
