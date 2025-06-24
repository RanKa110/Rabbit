using UnityEngine;

public class DamageReceiver : IDamageable
{
    private readonly StatManager _statManager;
    private readonly float _parryChance;
    private readonly System.Action onDeath;
    private bool _isDead;

    public bool IsDead => _isDead;
    public Collider2D Collider { get; }

    public DamageReceiver(StatManager statManager, Collider2D collider, float parryChance, System.Action onDeathCallback)
    {
        _statManager = statManager;
        _parryChance = parryChance;
        Collider = collider;
        onDeath = onDeathCallback;
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

        if (_statManager.GetValue(StatType.CurHp) <= 0f)
        {
            _isDead = true;
            onDeath?.Invoke();
        }
    }

    public void Dead()
    {
    }
}
