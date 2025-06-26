using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageableSensor : MonoBehaviour
{
    private List<IDamageable> _damageables = new List<IDamageable>();
    
    public List<IDamageable> Damageables => new List<IDamageable>(_damageables);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            if (!_damageables.Contains(damageable))
            {
                _damageables.Add(damageable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            _damageables.Remove(damageable);
        }
    }
}
