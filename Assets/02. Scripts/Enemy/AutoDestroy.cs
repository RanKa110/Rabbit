using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("자동 삭제 설정")]
    [SerializeField] private float destroyTime = 1f;
    [SerializeField] private bool useParticleSystemDuration = true;
    
    private ParticleSystem particleSystem;
    
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        
        // 파티클 시스템이 있고 duration 사용 옵션이 켜져있으면
        if (useParticleSystemDuration && particleSystem != null)
        {
            // 파티클 시스템의 전체 재생 시간을 계산
            float totalDuration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
            Destroy(gameObject, totalDuration);
        }
        else
        {
            // 설정된 시간 후 삭제
            Destroy(gameObject, destroyTime);
        }
    }
}

// 더 고급 버전 (오디오도 고려)
public class AdvancedAutoDestroy : MonoBehaviour
{
    [Header("자동 삭제 설정")]
    [SerializeField] private float destroyTime = 1f;
    [SerializeField] private bool waitForParticles = true;
    [SerializeField] private bool waitForAudio = true;
    
    private ParticleSystem[] particleSystems;
    private AudioSource audioSource;
    
    void Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        
        float maxDuration = destroyTime;
        
        // 파티클 시스템 시간 계산
        if (waitForParticles && particleSystems.Length > 0)
        {
            foreach (var ps in particleSystems)
            {
                float psDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                maxDuration = Mathf.Max(maxDuration, psDuration);
            }
        }
        
        // 오디오 시간 계산
        if (waitForAudio && audioSource != null && audioSource.clip != null)
        {
            maxDuration = Mathf.Max(maxDuration, audioSource.clip.length);
        }
        
        Destroy(gameObject, maxDuration);
    }
}