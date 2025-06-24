using UnityEngine;
using System.Collections.Generic;

public class AreaController : MonoBehaviour
{
    [Header("이 구역을 막고 있는 Wall Tile/Tilemap GameObject")]
    [SerializeField] private GameObject wallBlocker;

    // 구역 내 모든 적을 추적할 리스트
    private List<EnemyController> _enemies = new List<EnemyController>();

    private void Awake()
    {
        // 자식으로 배치된 모든 EnemyController 수집
        _enemies.AddRange(GetComponentsInChildren<EnemyController>());

        // 적 사망 이벤트 구독
        EnemyController.OnAnyEnemyDie += HandleEnemyDie;
    }

    private void HandleEnemyDie(EnemyController dead)
    {
        if (_enemies.Remove(dead) && _enemies.Count == 0)
        {
            // 모든 적 처치 완료
            UnlockWall();
        }
    }

    private void UnlockWall()
    {
        // Wall 비활성화
        wallBlocker.SetActive(false);

        // 더 이상 이벤트가 필요 없으니 언구독
        EnemyController.OnAnyEnemyDie -= HandleEnemyDie;
    }

    private void OnDestroy()
    {
        // 혹시 누락 방지
        EnemyController.OnAnyEnemyDie -= HandleEnemyDie;
    }
}