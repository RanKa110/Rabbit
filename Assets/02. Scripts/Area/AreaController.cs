using UnityEngine;
using System.Collections.Generic;

public class AreaController : MonoBehaviour
{
    [Header("이 Zone 전용 Wall")]
    [SerializeField] private GameObject wallBlocker;

    // 이 ZoneRoot 하위에 있는 EnemyController 만 추적
    private List<EnemyController> _enemies;

    private void Awake()
    {
        // ZoneRoot(=이 GameObject) 하위 EnemyController 전부 모으기
        _enemies = new List<EnemyController>(GetComponentsInChildren<EnemyController>());

        // 전역 이벤트 구독
        EnemyController.OnAnyEnemyDie += HandleEnemyDie;
    }

    private void HandleEnemyDie(EnemyController dead)
    {
        // “자기 Zone의 적”이 맞다면 리스트에서 제거
        if (_enemies.Remove(dead) && _enemies.Count == 0)
        {
            // 전부 잡았다 -> 현재 진행중인 Wall만 연다
            wallBlocker.SetActive(false);
            // 더 이상 이벤트 필요 없으니 언구독
            EnemyController.OnAnyEnemyDie -= HandleEnemyDie;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        //Player가 해당 Wall에 진입 시 Wall을 다시 닫음
        wallBlocker.SetActive(true);
        Destroy(wallBlocker);
    }

    private void OnDestroy()
    {
        EnemyController.OnAnyEnemyDie -= HandleEnemyDie;
    }
}