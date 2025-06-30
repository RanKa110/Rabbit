using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private IAttackable _attacker;
    private float _speed;

    public void Initialize(Vector2 direction, IAttackable attacker, float speed)
    {
        _attacker = attacker;
        _speed = speed;
        GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  패링 시도
        if (collision.TryGetComponent<IParryable>(out var parryTarget))
        {
            if (parryTarget.TryParrying())
            {
                parryTarget.OnParrySuccess();
                Destroy(gameObject);
                return;
            }
        }

        if (collision.TryGetComponent<IDamageable>(out var dmg) && !dmg.IsDead)
        {
            dmg.TakeDamage(_attacker);
            Destroy(gameObject);
        }
    }
}
