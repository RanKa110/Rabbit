using UnityEngine;

// 유도 투사체 클래스
public class HomingProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 5f;
    public float damage = 20f;
    public float rotationSpeed = 200f;
    public float lifeTime = 5f;
    
    private Transform target;
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 생명 시간 후 자동 파괴
        Destroy(gameObject, lifeTime);
    }
    
    void FixedUpdate()
    {
        if (target == null)
        {
            // 타겟이 없으면 직진
            rb.linearVelocity = transform.right * speed;
            return;
        }
        
        // 타겟을 향한 방향 계산
        Vector2 direction = (target.position - transform.position).normalized;
        
        // 현재 방향에서 타겟 방향으로 회전
        float rotateAmount = Vector3.Cross(direction, transform.right).z;
        rb.angularVelocity = -rotateAmount * rotationSpeed;
        
        // 앞으로 이동
        rb.linearVelocity = transform.right * speed;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어에게 데미지
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // 임시 공격자 객체 생성 (데미지만 전달)
                var tempAttacker = new TempAttacker(damage);
                damageable.TakeDamage(tempAttacker);
            }
            
            // 투사체 파괴
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            // 벽이나 땅에 충돌시 파괴
            Destroy(gameObject);
        }
    }
} 