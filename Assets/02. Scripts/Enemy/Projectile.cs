using UnityEngine;
using TMPro;

// 투사체 클래스
public class Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 direction;
    private Rigidbody2D rb;
    
    [Header("투사체 설정")]
    public float lifeTime = 5f;
    public bool destroyOnHit = true;
    public GameObject hitEffectPrefab;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }
    
    public void Initialize(Vector2 dir, float spd, float dmg)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어에게 맞았을 때
        if (collision.CompareTag("Player"))
        {
            // 안전한 데미지 처리
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // IAttackable이 필요하므로 임시 구현체 생성
                var tempAttacker = new TempAttacker(damage);
                damageable.TakeDamage(tempAttacker);
            }
            else
            {
                // PlayerController에 TakeDamage가 없으므로 로그만 출력
                Debug.Log($"Projectile hit player for {damage} damage (PlayerController doesn't implement IDamageable yet)");
            }
            
            // 히트 이펙트
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        // 벽이나 지형에 맞았을 때
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}