using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RandomEncounterZone2D : MonoBehaviour
{
    [Header("확률이 증가되는 주기(시간)")]
    public float intervalSeconds = 30f;

    [Header("기본 확률"), Range(0f, 1f)]
    public float baseSpawnChance = 0.2f; // 2%

    [Tooltip("확률이 증가될때 마다 올라가는 값"), Range(0f, 1f)]
    public float perCounterBonus = 0.05f; // 5%

    [Tooltip("최대 증가되는 확률")]
    public int maxCounter = 10; 

    [Header("Spawn되는 Enemy의 종류 설정")]
    public GameObject[] enemyPrefabs;

    [Tooltip("Enemy가 Spawn되는 Zone 설정")]
    public Transform[] spawnPoints;

    [Tooltip("생성되는 Enemy의 수")]
    public int maxSpawnPerEvent = 3;

    // Internal state
    private int encounterCounter = 0;
    private bool playerInside = false;
    private Coroutine encounterRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!playerInside && other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("[RandomEncounterZone2D] Player entered zone");
            TrySpawn();
            encounterRoutine = StartCoroutine(EncounterLoop());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playerInside && other.CompareTag("Player"))
        {
            playerInside = false;
            if (encounterRoutine != null)
                StopCoroutine(encounterRoutine);
            encounterCounter = 0;
            Debug.Log("[RandomEncounterZone2D] Player exited zone");
        }
    }

    private IEnumerator EncounterLoop()
    {
        while (playerInside)
        {
            yield return new WaitForSeconds(intervalSeconds);
            IncrementCounter();
            TrySpawn();
        }
    }

    private void IncrementCounter()
    {
        encounterCounter = Mathf.Min(encounterCounter + 1, maxCounter);
        Debug.Log($"[RandomEncounterZone2D] Counter: {encounterCounter}");
    }

    private void TrySpawn()
    {
        float chance = Mathf.Clamp01(baseSpawnChance + encounterCounter * perCounterBonus);
        Debug.Log($"[RandomEncounterZone2D] Spawn chance: {chance}");

        if (Random.value <= chance)
        {
            int toSpawn = Random.Range(1, maxSpawnPerEvent + 1);
            SpawnEnemies(toSpawn);
            encounterCounter = 0;
        }
    }

    private void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[RandomEncounterZone2D] No enemyPrefabs or spawnPoints assigned.");
                return;
            }

            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"[RandomEncounterZone2D] Spawned {prefab.name} at {spawnPoint.position}");
        }
    }
}
