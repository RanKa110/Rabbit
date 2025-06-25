using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("HP바 설정")]
    [SerializeField] private GameObject healthBarPrefab; // HP바 프리팹
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0); // 월드 공간 오프셋
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 50f); // 스크린 공간 오프셋 (픽셀)
    [SerializeField] private bool hideWhenFull = true; // 체력이 가득할 때 숨기기
    [SerializeField] private float hideDelay = 3f; // 숨기기 전 대기 시간
    
    [Header("HP바 크기 설정")]
    [SerializeField] private Vector2 healthBarSize = new Vector2(100f, 15f); // HP바 크기 (픽셀)
    
    [Header("HP바 컴포넌트")]
    private GameObject healthBarInstance;
    private Slider healthSlider;
    private Image fillImage;
    private TextMeshProUGUI healthText;
    private RectTransform healthBarRect;
    
    [Header("색상 설정")]
    [SerializeField] private Gradient healthGradient; // 체력에 따른 색상 변화
    [SerializeField] private Color defaultColor = Color.green;
    [SerializeField] private Color midColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;
    
    // 내부 변수
    private Camera mainCamera;
    private Canvas screenCanvas;
    private float maxHealth;
    private float currentHealth;
    private float lastDamageTime;
    private bool isInitialized = false;
    
    // 컴포넌트 참조
    private EnemyController enemyController;
    private MonsterBase monsterBase;
    private StatManager statManager;
    
    private void Awake()
    {
        // 컴포넌트 참조 가져오기
        enemyController = GetComponent<EnemyController>();
        monsterBase = GetComponent<MonsterBase>();
        statManager = GetComponent<StatManager>();
        
        // 메인 카메라 찾기
        mainCamera = Camera.main;
        
        // Gradient 초기화 (인스펙터에서 설정하지 않은 경우)
        if (healthGradient == null || healthGradient.colorKeys.Length == 0)
        {
            InitializeDefaultGradient();
        }
    }
    
    private void Start()
    {
        // 메인 카메라 확인
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found! HP Bar will not work properly.");
                return;
            }
        }
        
        // 스크린 캔버스 찾기 또는 생성
        FindOrCreateScreenCanvas();
        
        // HP바 생성 및 초기화
        CreateHealthBar();
        InitializeHealth();
        
        Debug.Log($"{gameObject.name}: HP Bar initialized - Canvas: {screenCanvas != null}, HealthBar: {healthBarInstance != null}, Rect: {healthBarRect != null}");
    }
    
    private void InitializeDefaultGradient()
    {
        healthGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(lowColor, 0.0f);
        colorKeys[1] = new GradientColorKey(midColor, 0.5f);
        colorKeys[2] = new GradientColorKey(defaultColor, 1.0f);
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
        
        healthGradient.SetKeys(colorKeys, alphaKeys);
    }
    
    private void FindOrCreateScreenCanvas()
    {
        // 기존 스크린 캔버스 찾기
        GameObject canvasObj = GameObject.Find("EnemyHealthBarCanvas");
        
        if (canvasObj == null)
        {
            // 없으면 새로 생성
            canvasObj = new GameObject("EnemyHealthBarCanvas");
            screenCanvas = canvasObj.AddComponent<Canvas>();
            screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            screenCanvas.sortingOrder = 100; // 다른 UI 위에 표시
            
            // Canvas Scaler 추가
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Graphic Raycaster 추가 (선택사항)
            // canvasObj.AddComponent<GraphicRaycaster>();
            
            // DontDestroyOnLoad로 설정하여 씬 전환시에도 유지
            DontDestroyOnLoad(canvasObj);
            
            Debug.Log("EnemyHealthBarCanvas created successfully");
        }
        else
        {
            screenCanvas = canvasObj.GetComponent<Canvas>();
        }
        
        // Canvas가 제대로 설정되었는지 확인
        if (screenCanvas == null)
        {
            Debug.LogError("Failed to get or create screen canvas!");
        }
    }
    
    private void CreateHealthBar()
    {
        // HP바 프리팹이 설정되지 않은 경우 기본 HP바 생성
        if (healthBarPrefab == null)
        {
            CreateDefaultHealthBar();
        }
        else
        {
            // 프리팹에서 HP바 생성
            healthBarInstance = Instantiate(healthBarPrefab, screenCanvas.transform);
            
            // 컴포넌트 찾기
            healthBarRect = healthBarInstance.GetComponent<RectTransform>();
            if (healthBarRect == null)
            {
                healthBarRect = healthBarInstance.AddComponent<RectTransform>();
            }
            
            // 앵커 설정
            healthBarRect.anchorMin = new Vector2(0, 0);
            healthBarRect.anchorMax = new Vector2(0, 0);
            healthBarRect.pivot = new Vector2(0.5f, 0.5f);
            
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                fillImage = healthSlider.fillRect.GetComponent<Image>();
            }
            healthText = healthBarInstance.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // 크기 설정
        if (healthBarRect != null)
        {
            healthBarRect.sizeDelta = healthBarSize;
        }
        
        // null 체크
        if (healthBarRect == null)
        {
            Debug.LogError($"{gameObject.name}: Failed to create health bar RectTransform!");
            return;
        }
        
        isInitialized = true;
    }
    
    private void CreateDefaultHealthBar()
    {
        // HP바 GameObject 생성
        healthBarInstance = new GameObject($"HealthBar_{gameObject.name}");
        healthBarInstance.transform.SetParent(screenCanvas.transform, false);
        
        // RectTransform 설정
        healthBarRect = healthBarInstance.AddComponent<RectTransform>();
        healthBarRect.sizeDelta = healthBarSize;
        healthBarRect.pivot = new Vector2(0.5f, 0.5f);
        healthBarRect.anchorMin = new Vector2(0, 0);
        healthBarRect.anchorMax = new Vector2(0, 0);
        healthBarRect.anchoredPosition = Vector2.zero;
        
        // 배경 이미지
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarInstance.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        // 테두리
        GameObject border = new GameObject("Border");
        border.transform.SetParent(healthBarInstance.transform, false);
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        borderImage.type = Image.Type.Sliced;
        
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(4, 4);
        borderRect.anchoredPosition = Vector2.zero;
        
        // Slider 생성
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(healthBarInstance.transform, false);
        healthSlider = sliderObj.AddComponent<Slider>();
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.sizeDelta = new Vector2(-6, -6);
        sliderRect.anchoredPosition = Vector2.zero;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        fillImage = fill.AddComponent<Image>();
        fillImage.color = defaultColor;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        // Slider 설정
        healthSlider.fillRect = fillRect;
        healthSlider.targetGraphic = fillImage;
        healthSlider.interactable = false;
        
        // 체력 텍스트 (선택사항)
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(healthBarInstance.transform, false);
        healthText = textObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "";
        healthText.fontSize = 12f;
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
    }
    
    private void InitializeHealth()
    {
        // StatManager를 사용하는 경우 (EnemyController)
        if (statManager != null && enemyController != null)
        {
            maxHealth = statManager.GetValue(StatType.MaxHp);
            currentHealth = statManager.GetValue(StatType.CurHp);
        }
        // MonsterBase를 사용하는 경우
        else if (monsterBase != null)
        {
            maxHealth = monsterBase.maxHealth;
            currentHealth = monsterBase.currentHealth;
        }
        
        UpdateHealthBar();
        
        // 초기 위치 설정
        if (healthBarRect != null && mainCamera != null)
        {
            Vector3 worldPosition = transform.position + worldOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            healthBarRect.position = screenPosition + (Vector3)screenOffset;
        }
        
        // 체력이 가득하면 숨기기
        if (hideWhenFull && currentHealth >= maxHealth)
        {
            healthBarInstance.SetActive(false);
        }
        else
        {
            healthBarInstance.SetActive(true);
        }
    }
    
    private void LateUpdate()
    {
        if (!isInitialized || healthBarInstance == null || healthBarRect == null) return;
        
        // 카메라가 없으면 다시 찾기
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        // 몬스터의 월드 위치 계산
        Vector3 worldPosition = transform.position + worldOffset;
        
        // 월드 위치를 스크린 좌표로 변환
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        
        // 카메라 뒤에 있는지 체크
        if (screenPosition.z < 0)
        {
            if (healthBarInstance.activeSelf)
                healthBarInstance.SetActive(false);
            return;
        }
        
        // 화면 경계 체크 (여유를 두고)
        bool isOffScreen = screenPosition.x < -100 || screenPosition.x > Screen.width + 100 ||
                          screenPosition.y < -100 || screenPosition.y > Screen.height + 100;
        
        if (isOffScreen)
        {
            if (healthBarInstance.activeSelf)
                healthBarInstance.SetActive(false);
        }
        else
        {
            // 화면 안에 있고 HP바가 표시되어야 하는 경우
            if (currentHealth < maxHealth || !hideWhenFull)
            {
                if (!healthBarInstance.activeSelf)
                {
                    healthBarInstance.SetActive(true);
                }
                
                // HP바 위치 업데이트 (스크린 좌표 + 오프셋)
                Vector3 finalPosition = screenPosition + (Vector3)screenOffset;
                healthBarRect.position = finalPosition;
                
                // 디버그 로그 (필요시 주석 해제)
                // Debug.Log($"{gameObject.name} HP Bar - World: {worldPosition}, Screen: {screenPosition}, Final: {finalPosition}");
            }
        }
        
        // 숨기기 로직
        if (hideWhenFull && healthBarInstance.activeSelf && currentHealth >= maxHealth)
        {
            if (Time.time - lastDamageTime > hideDelay)
            {
                healthBarInstance.SetActive(false);
            }
        }
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // 체력 업데이트만 체크
        UpdateCurrentHealth();
    }
    
    private void UpdateCurrentHealth()
    {
        float newHealth = 0;
        
        // StatManager를 사용하는 경우
        if (statManager != null && enemyController != null)
        {
            newHealth = statManager.GetValue(StatType.CurHp);
        }
        // MonsterBase를 사용하는 경우
        else if (monsterBase != null)
        {
            newHealth = monsterBase.currentHealth;
        }
        
        // 체력이 변경되었는지 확인
        if (Mathf.Abs(newHealth - currentHealth) > 0.01f)
        {
            currentHealth = newHealth;
            UpdateHealthBar();
            
            // 데미지를 받았다면 HP바 표시
            if (currentHealth < maxHealth)
            {
                if (!healthBarInstance.activeSelf)
                {
                    healthBarInstance.SetActive(true);
                }
                lastDamageTime = Time.time;
            }
        }
    }
    
    private void UpdateHealthBar()
    {
        if (healthSlider == null) return;
        
        float healthPercentage = currentHealth / maxHealth;
        healthSlider.value = healthPercentage;
        
        // 색상 업데이트
        if (fillImage != null)
        {
            fillImage.color = healthGradient.Evaluate(healthPercentage);
        }
        
        // 텍스트 업데이트 (선택사항)
        if (healthText != null)
        {
            // 텍스트를 표시하려면 주석 해제
            // healthText.text = $"{Mathf.Ceil(currentHealth)}/{Mathf.Ceil(maxHealth)}";
            // 또는 퍼센트로 표시
            // healthText.text = $"{Mathf.Ceil(healthPercentage * 100)}%";
        }
    }
    
    // 외부에서 체력을 설정할 수 있는 메서드
    public void SetHealth(float current, float max)
    {
        maxHealth = max;
        currentHealth = current;
        UpdateHealthBar();
    }
    
    // HP바 표시/숨기기
    public void ShowHealthBar()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.SetActive(true);
            lastDamageTime = Time.time;
            
            // 즉시 위치 업데이트
            if (healthBarRect != null && mainCamera != null)
            {
                Vector3 worldPosition = transform.position + worldOffset;
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
                healthBarRect.position = screenPosition + (Vector3)screenOffset;
            }
        }
    }
    
    public void HideHealthBar()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // HP바 인스턴스 정리
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }
    
    private void OnBecameInvisible()
    {
        // 몬스터가 화면에서 사라지면 HP바도 숨기기
        if (healthBarInstance != null)
        {
            healthBarInstance.SetActive(false);
        }
    }
    
    private void OnBecameVisible()
    {
        // 몬스터가 화면에 나타나면 HP바 표시 (체력이 닳은 경우만)
        if (healthBarInstance != null && currentHealth < maxHealth)
        {
            healthBarInstance.SetActive(true);
        }
    }
}