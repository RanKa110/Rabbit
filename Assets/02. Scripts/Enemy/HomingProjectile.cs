using UnityEngine;
using TMPro;

// 유도 투사체 클래스
public class HomingProjectile : MonoBehaviour
{
    [Header("유도 설정")]
    public float speed = 8f;
    public float rotateSpeed = 200f;
    public float damage = 20f;
    public float lifeTime = 5f;
    
    private Transform target;
    private Rigidbody2D rb;
    
    [Header("이펙트")]
    public GameObject hitEffectPrefab;
    public GameObject trailEffectPrefab;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
        
        // 트레일 효과
        if (trailEffectPrefab != null)
        {
            Instantiate(trailEffectPrefab, transform);
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    private void FixedUpdate()
    {
        if (target == null)
        {
            // 타겟이 없으면 직진
            rb.linearVelocity = transform.right * speed;
            return;
        }
        
        // 타겟 방향 계산
        Vector2 direction = (Vector2)target.position - rb.position;
        direction.Normalize();
        
        // 회전값 계산
        float rotateAmount = Vector3.Cross(transform.right, direction).z;
        
        // 회전 적용
        rb.angularVelocity = -rotateAmount * rotateSpeed;
        
        // 전진
        rb.linearVelocity = transform.right * speed;
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
                Debug.Log($"Homing projectile hit player for {damage} damage (PlayerController doesn't implement IDamageable yet)");
            }
            
            // 히트 이펙트
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
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