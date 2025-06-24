using UnityEngine;
using System.Collections;
using TMPro;

// 원거리 몬스터 클래스
public class RangedMonster : MonsterBase
{
    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float minAttackDistance = 3f; // 최소 공격 거리
    
    // EnemyController 참조
    private EnemyController enemyController;
    
    protected override void Start()
    {
        base.Start();
        attackRange = 8f; // 원거리 공격 범위
        moveSpeed = 1.5f; // 원거리 몬스터는 조금 느림
        
        // EnemyController 참조 가져오기
        enemyController = GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("RangedMonster: EnemyController component not found!");
        }
        else if (enemyController.Data != null)
        {
            // SO에서 프리팹 가져오기 - 새로운 메서드 사용
            GameObject prefabFromSO = enemyController.Data.GetProjectilePrefab();
            if (prefabFromSO != null)
            {
                projectilePrefab = prefabFromSO;
                Debug.Log($"RangedMonster: Projectile prefab loaded from SO: {projectilePrefab.name}");
            }
            else
            {
                Debug.LogWarning("RangedMonster: No projectile prefab found in EnemySO! Make sure RangedProjectilePrefab is set.");
                
                // 디버그: SO의 값들 확인
                Debug.LogWarning($"RangedProjectilePrefab: {enemyController.Data.RangedProjectilePrefab}");
                Debug.LogWarning($"Legacy ProjectilePrefab: {enemyController.Data.ProjectilePrefab}");
                Debug.LogWarning($"Enemy Type: {enemyController.Data.Type}");
            }
        }
        
        // 현재 projectilePrefab 상태 확인
        Debug.Log($"RangedMonster Start - Final projectilePrefab: {projectilePrefab}");
        
        // firePoint가 없으면 자동으로 찾기
        if (firePoint == null)
        {
            // 먼저 자식에서 FirePoint 찾기
            firePoint = transform.Find("FirePoint");
            
            // 없으면 AttackPoint 찾기
            if (firePoint == null)
            {
                firePoint = transform.Find("AttackPoint");
            }
            
            // 그래도 없으면 자동 생성
            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = new Vector3(0.5f, 0, 0); // 캐릭터 앞쪽
                firePoint = firePointObj.transform;
                Debug.LogWarning("RangedMonster: FirePoint was created automatically. Please adjust position in prefab.");
            }
        }
        
        Debug.Log($"RangedMonster Start - FirePoint: {firePoint}");
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
            StartCoroutine(RangedAttack());
        }
    }
    
    // 공용 메서드로 변경하여 EnemyController에서 호출 가능하도록 함
    public void PerformRangedAttack()
    {
        // 조건 체크 없이 바로 공격 실행
        FireProjectile();
    }
    
    private void FireProjectile()
    {
        // 디버그 로그
        Debug.Log($"FireProjectile called. Prefab: {projectilePrefab}, FirePoint: {firePoint}");
        
        // null 체크를 더 자세히
        if (projectilePrefab == null)
        {
            Debug.LogError("RangedMonster: projectilePrefab is null!");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("RangedMonster: firePoint is null!");
            return;
        }
        
        // 투사체 발사
        Transform targetTransform = null;
        
        // EnemyController에서 타겟 가져오기
        if (enemyController != null && enemyController.Target != null)
        {
            targetTransform = enemyController.Target.Collider.transform;
        }
        // 폴백: player 직접 사용
        else if (player != null)
        {
            targetTransform = player;
        }
        
        if (targetTransform == null)
        {
            Debug.LogWarning("RangedMonster: No target found for projectile!");
            return;
        }
        
        // firePoint 위치를 약간 앞으로 조정하여 투사체 생성
        Vector2 direction = (targetTransform.position - firePoint.position).normalized;
        Vector3 spawnPosition = firePoint.position + (Vector3)(direction * 0.5f); // 0.5 유닛 앞에 생성
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Projectile instantiated: {projectile.name} at position {spawnPosition}");
        
        // 투사체에 발사한 몬스터 설정
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            // 발사한 몬스터 설정 (충돌 무시를 위해)
            proj.SetOwner(gameObject);
            
            // EnemyController의 스탯을 사용하거나 자체 스탯 사용
            float damage = enemyController != null ? 
                enemyController.StatManager.GetValue(StatType.AttackPow) : 
                attackDamage;
            
            proj.Initialize(direction, projectileSpeed, damage);
            Debug.Log($"Projectile initialized with damage: {damage}, speed: {projectileSpeed}");
        }
        else
        {
            Debug.LogWarning("Projectile component not found, using Rigidbody2D directly");
            // 기본 투사체 움직임
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }
            else
            {
                Debug.LogError("No Rigidbody2D found on projectile!");
            }
        }
        
        // 투사체 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    private IEnumerator RangedAttack()
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
        
        // 투사체 발사
        FireProjectile();
        
        // 공격 애니메이션 종료 대기
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
        
        // 쿨다운 대기
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}