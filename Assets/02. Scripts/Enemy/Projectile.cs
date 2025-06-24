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
    
    private bool isInitialized = false;
    private GameObject owner; // 발사한 몬스터 참조
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        // Initialize가 호출되지 않았다면 경고
        if (!isInitialized)
        {
            Debug.LogWarning("Projectile started without initialization!");
        }
        
        Destroy(gameObject, lifeTime);
    }
    
    public void Initialize(Vector2 dir, float spd, float dmg)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        isInitialized = true;
        
        // Awake에서 rb를 가져왔으므로 바로 사용 가능
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            Debug.Log($"Projectile velocity set to: {rb.linearVelocity}");
        }
        else
        {
            Debug.LogError("Projectile: Rigidbody2D not found!");
        }
    }
    
    // 발사한 몬스터 설정
    public void SetOwner(GameObject ownerObject)
    {
        owner = ownerObject;
        
        // 발사한 몬스터와의 충돌 무시
        if (owner != null)
        {
            Collider2D ownerCollider = owner.GetComponent<Collider2D>();
            Collider2D projectileCollider = GetComponent<Collider2D>();
            
            if (ownerCollider != null && projectileCollider != null)
            {
                Physics2D.IgnoreCollision(ownerCollider, projectileCollider);
                Debug.Log($"Ignoring collision between projectile and owner: {owner.name}");
            }
        }
    }
    
    private void FixedUpdate()
    {
        // 만약 velocity가 0이면 다시 설정
        if (isInitialized && rb != null && rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 자기 자신을 발사한 몬스터와의 충돌은 무시
        if (owner != null && collision.gameObject == owner)
        {
            Debug.Log("Projectile ignoring collision with owner");
            return;
        }
        
        // Enemy 태그와의 충돌도 무시 (같은 팀)
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Projectile ignoring collision with other enemy");
            return;
        }
        
        // 플레이어에게 맞았을 때
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"Projectile hit player! Damage: {damage}");
            
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
            Debug.Log($"Projectile hit {collision.tag}");
            
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
        else
        {
            // 다른 태그와 충돌 시 로그
            Debug.Log($"Projectile hit object with tag: {collision.tag}");
        }
    }
    
    private void OnBecameInvisible()
    {
        // 화면 밖으로 나가면 삭제
        Destroy(gameObject);
    }
    
    // 디버그용
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}