using UnityEngine;

// 유도 투사체 클래스
public class HomingProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 5f;
    public float damage = 20f;
    public float rotationSpeed = 400f; // 회전 속도 증가 (200 -> 400)
    public float lifeTime = 5f;
    
    private Transform target;
    private Rigidbody2D rb;
    private Vector2 initialDirection; // 초기 방향 저장
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 생명 시간 후 자동 파괴
        Destroy(gameObject, lifeTime);
        
        // 타겟이 설정되어 있으면 초기 방향을 타겟 방향으로
        if (target != null)
        {
            initialDirection = (target.position - transform.position).normalized;
            
            // 즉시 타겟을 바라보도록 회전
            float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            // 타겟이 없으면 현재 방향 유지
            initialDirection = transform.right;
        }
    }
    
    void FixedUpdate()
    {
        if (target == null)
        {
            // 타겟이 없으면 초기 방향으로 직진
            rb.linearVelocity = initialDirection * speed;
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
        
        // 타겟이 설정되면 초기 방향을 타겟 방향으로 설정
        if (target != null)
        {
            initialDirection = (target.position - transform.position).normalized;
            
            // 즉시 타겟을 바라보도록 회전
            float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            // 즉시 이동 시작
            if (rb != null)
            {
                rb.linearVelocity = initialDirection * speed;
            }
        }
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