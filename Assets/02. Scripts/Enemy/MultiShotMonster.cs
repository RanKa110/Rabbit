using UnityEngine;
using System.Collections;
using TMPro;

// 멀티샷 원거리 몬스터 클래스
public class MultiShotMonster : MonsterBase
{
    [Header("멀티샷 공격 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float minAttackDistance = 4f; // 최소 공격 거리
    
    [Header("멀티샷 패턴 설정")]
    [Range(3, 10)]
    public int projectileCount = 5; // 발사할 투사체 개수
    [Range(15f, 180f)]
    public float spreadAngle = 60f; // 전체 퍼지는 각도
    public bool useRandomSpread = false; // 랜덤하게 퍼뜨릴지 여부
    
    [Header("공격 패턴 타입")]
    public MultiShotPattern shotPattern = MultiShotPattern.Fan; // 발사 패턴
    
    // 패턴 종류
    public enum MultiShotPattern
    {
        Fan,        // 부채꼴
        Circle,     // 원형
        Cross,      // 십자가
        Random,     // 랜덤
        Wave        // 웨이브 (시간차 발사)
    }
    
    // EnemyController 참조
    private EnemyController enemyController;
    
    protected override void Start()
    {
        base.Start();
        attackRange = 10f; // 멀티샷 공격 범위
        moveSpeed = 1.3f; // 멀티샷 몬스터는 느림
        attackCooldown = 2.5f; // 더 긴 쿨다운 (여러 발 쏘니까)
        
        // EnemyController 참조 가져오기
        enemyController = GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("MultiShotMonster: EnemyController component not found!");
        }
        else if (enemyController.Data != null)
        {
            // SO에서 프리팹 가져오기
            GameObject prefabFromSO = enemyController.Data.GetProjectilePrefab();
            if (prefabFromSO != null)
            {
                projectilePrefab = prefabFromSO;
                Debug.Log($"MultiShotMonster: Projectile prefab loaded from SO: {projectilePrefab.name}");
            }
            else
            {
                Debug.LogWarning("MultiShotMonster: No projectile prefab found in EnemySO!");
            }
        }
        
        // firePoint가 없으면 자동으로 찾기
        if (firePoint == null)
        {
            // 먼저 자식에서 FirePoint 찾기
            firePoint = transform.Find("FirePoint");
            
            // 없으면 자동 생성
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = new Vector3(0.5f, 0, 0); // 캐릭터 앞쪽
                firePoint = firePointObj.transform;
                Debug.LogWarning("MultiShotMonster: FirePoint was created automatically. Please adjust position in prefab.");
            }
        }
    }
    
    protected override void ChaseBehavior()
    {
        if (player == null) return;
        
        float distanceToPlayer = GetDistanceToPlayer();
        
        // 너무 가까우면 뒤로 이동
        if (distanceToPlayer < minAttackDistance)
        {
            Vector2 direction = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
        // 적정 거리보다 멀면 접근
        else if (distanceToPlayer > attackRange * 0.8f)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }
        // 적정 거리면 정지
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        
        // 애니메이션
        if (animator != null)
        {
            animator.SetBool("isMoving", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }
    
    protected override void AttackBehavior()
    {
        // 이동 멈춤 - X축 속도를 0으로
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // 공격 쿨다운 체크
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        if (canAttack && !isAttacking)
        {
            StartCoroutine(MultiShotAttack());
        }
    }
    
    // EnemyController에서 호출할 수 있는 공용 메서드
    public void PerformMultiShotAttack()
    {
        // 조건 체크 없이 바로 공격 실행
        FireMultipleProjectiles();
    }
    
    private void FireMultipleProjectiles()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError($"MultiShotMonster: Cannot fire! Prefab: {projectilePrefab}, FirePoint: {firePoint}");
            return;
        }
        
        Transform targetTransform = GetTargetTransform();
        if (targetTransform == null)
        {
            Debug.LogWarning("MultiShotMonster: No target found!");
            return;
        }
        
        // 패턴에 따라 발사
        switch (shotPattern)
        {
            case MultiShotPattern.Fan:
                FireFanPattern(targetTransform);
                break;
            case MultiShotPattern.Circle:
                FireCirclePattern();
                break;
            case MultiShotPattern.Cross:
                FireCrossPattern();
                break;
            case MultiShotPattern.Random:
                FireRandomPattern(targetTransform);
                break;
            case MultiShotPattern.Wave:
                StartCoroutine(FireWavePattern(targetTransform));
                break;
        }
    }
    
    // 부채꼴 패턴
    private void FireFanPattern(Transform target)
    {
        Vector2 baseDirection = (target.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        
        float angleStep = spreadAngle / (projectileCount - 1);
        float startAngle = baseAngle - (spreadAngle / 2);
        
        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            if (useRandomSpread)
            {
                currentAngle += Random.Range(-5f, 5f); // 약간의 랜덤성 추가
            }
            
            Vector2 direction = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            );
            
            CreateProjectile(direction);
        }
    }
    
    // 원형 패턴
    private void FireCirclePattern()
    {
        float angleStep = 360f / projectileCount;
        
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            
            CreateProjectile(direction);
        }
    }
    
    // 십자가 패턴
    private void FireCrossPattern()
    {
        // 4방향 또는 8방향
        int directions = projectileCount >= 8 ? 8 : 4;
        float angleStep = 360f / directions;
        
        for (int i = 0; i < directions; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            
            CreateProjectile(direction);
        }
    }
    
    // 랜덤 패턴
    private void FireRandomPattern(Transform target)
    {
        Vector2 baseDirection = (target.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        
        for (int i = 0; i < projectileCount; i++)
        {
            float randomAngle = baseAngle + Random.Range(-spreadAngle/2, spreadAngle/2);
            Vector2 direction = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );
            
            CreateProjectile(direction);
        }
    }
    
    // 웨이브 패턴 (시간차 발사)
    private IEnumerator FireWavePattern(Transform target)
    {
        Vector2 baseDirection = (target.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        
        float angleStep = spreadAngle / (projectileCount - 1);
        float startAngle = baseAngle - (spreadAngle / 2);
        
        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 direction = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            );
            
            CreateProjectile(direction);
            
            // 각 발사 사이에 짧은 딜레이
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // 실제 투사체 생성
    private void CreateProjectile(Vector2 direction)
    {
        // 발사 위치를 약간 앞으로 조정
        Vector3 spawnPosition = firePoint.position + (Vector3)(direction * 0.5f);
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        // 투사체 초기화
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetOwner(gameObject);
            
            float damage = enemyController != null ? 
                enemyController.StatManager.GetValue(StatType.AttackPow) : 
                attackDamage;
            
            // 멀티샷은 개별 데미지가 약간 낮을 수 있음
            float adjustedDamage = damage * 0.8f; // 80% 데미지
            
            proj.Initialize(direction, projectileSpeed, adjustedDamage);
        }
        else
        {
            // 기본 투사체 움직임
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }
        }
        
        // 투사체 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    // 타겟 가져오기
    private Transform GetTargetTransform()
    {
        // EnemyController에서 타겟 가져오기
        if (enemyController != null && enemyController.Target != null)
        {
            return enemyController.Target.Collider.transform;
        }
        // 폴백: player 직접 사용
        else if (player != null)
        {
            return player;
        }
        
        return null;
    }
    
    private IEnumerator MultiShotAttack()
    {
        isAttacking = true;
        canAttack = false;
        lastAttackTime = Time.time;
        
        // 공격 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 애니메이션 대기
        yield return new WaitForSeconds(0.3f);
        
        // 멀티샷 발사
        FireMultipleProjectiles();
        
        // 공격 애니메이션 종료 대기
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
        
        // 쿨다운 대기 (멀티샷은 더 긴 쿨다운)
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    // 공격 범위 시각화 (에디터용)
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        
        // 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 최소 거리
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
        
        // 발사 각도 시각화 (Fan 패턴일 때)
        if (shotPattern == MultiShotPattern.Fan && Application.isPlaying && player != null)
        {
            Gizmos.color = Color.green;
            Vector2 baseDirection = (player.position - firePoint.position).normalized;
            float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
            
            // 좌측 경계
            float leftAngle = (baseAngle - spreadAngle/2) * Mathf.Deg2Rad;
            Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle), 0);
            Gizmos.DrawRay(firePoint.position, leftDir * 3f);
            
            // 우측 경계
            float rightAngle = (baseAngle + spreadAngle/2) * Mathf.Deg2Rad;
            Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle), 0);
            Gizmos.DrawRay(firePoint.position, rightDir * 3f);
            
            // 중앙선
            Gizmos.color = Color.red;
            Gizmos.DrawRay(firePoint.position, baseDirection * 3f);
        }
    }
}