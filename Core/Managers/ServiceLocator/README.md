# ServiceLocator - 중앙집중식 서비스 관리 시스템

## 개요

ServiceLocator는 Unity 2D 게임 개발에서 **Find 사용을 금지**하고 **성능 최적화**를 위한 중앙집중식 서비스 관리 시스템입니다. 모든 서비스들을 한 곳에서 등록하고 관리하여 의존성 주입 패턴을 구현합니다.

## 주요 특징

- ✅ **Find 사용 금지**: GameObject.Find 대신 서비스 등록/조회 방식 사용
- ✅ **성능 최적화**: 캐싱된 참조를 통한 빠른 서비스 접근
- ✅ **싱글톤 패턴**: 전역에서 접근 가능한 단일 인스턴스
- ✅ **자동 생성**: 씬에 없으면 자동으로 생성되는 안전한 구조
- ✅ **씬 간 유지**: DontDestroyOnLoad로 씬 전환 시에도 유지
- ✅ **디버깅 지원**: 등록된 서비스 목록 확인 기능

## 사용법

### 1. 서비스 등록

```csharp
// 게임 시작 시 또는 Awake에서 서비스 등록
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController playerController;
    
    private void Awake()
    {
        // 서비스들을 ServiceLocator에 등록
        ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
        ServiceLocator.Instance.RegisterService<UIManager>(uiManager);
        ServiceLocator.Instance.RegisterService<PlayerController>(playerController);
    }
}
```

### 2. 서비스 사용

```csharp
public class WeaponController : MonoBehaviour
{
    private AudioManager audioManager;
    private UIManager uiManager;
    
    private void Start()
    {
        // ServiceLocator에서 필요한 서비스들을 가져와서 캐싱
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        uiManager = ServiceLocator.Instance.GetService<UIManager>();
    }
    
    public void FireWeapon()
    {
        // 캐싱된 참조를 사용하여 빠른 접근
        audioManager?.PlaySFX("WeaponFire");
        uiManager?.ShowDamageText("50", transform.position);
    }
}
```

### 3. 자동 탐지 및 등록

```csharp
public class EnemyAI : MonoBehaviour
{
    private PlayerController player;
    
    private void Start()
    {
        // 서비스가 등록되어 있지 않으면 씬에서 자동으로 찾아서 등록
        player = ServiceLocator.Instance.GetOrFindService<PlayerController>();
    }
    
    private void Update()
    {
        if (player != null)
        {
            // 플레이어 추적 로직
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
    }
}
```

### 4. 서비스 존재 확인

```csharp
public class SkillSystem : MonoBehaviour
{
    private void UseSkill()
    {
        // 서비스가 등록되어 있는지 먼저 확인
        if (ServiceLocator.Instance.HasService<ManaManager>())
        {
            var manaManager = ServiceLocator.Instance.GetService<ManaManager>();
            if (manaManager.ConsumeMana(50))
            {
                // 스킬 사용 로직
                CastFireball();
            }
        }
        else
        {
            Debug.LogWarning("ManaManager 서비스가 등록되지 않았습니다.");
        }
    }
}
```

## 권장 사용 패턴

### 1. 게임 초기화 시 모든 서비스 등록

```csharp
public class GameBootstrap : MonoBehaviour
{
    [Header("Core Managers")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputManager inputManager;
    
    [Header("Gameplay Services")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private QuestManager questManager;
    
    private void Awake()
    {
        RegisterCoreServices();
        RegisterGameplayServices();
    }
    
    private void RegisterCoreServices()
    {
        ServiceLocator.Instance.RegisterService<GameManager>(gameManager);
        ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
        ServiceLocator.Instance.RegisterService<UIManager>(uiManager);
        ServiceLocator.Instance.RegisterService<InputManager>(inputManager);
    }
    
    private void RegisterGameplayServices()
    {
        ServiceLocator.Instance.RegisterService<PlayerController>(playerController);
        ServiceLocator.Instance.RegisterService<InventoryManager>(inventoryManager);
        ServiceLocator.Instance.RegisterService<QuestManager>(questManager);
    }
}
```

### 2. 컴포넌트에서 서비스 캐싱

```csharp
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    // 서비스 참조 캐싱
    private AudioManager audioManager;
    private UIManager uiManager;
    private GameManager gameManager;
    
    private void Start()
    {
        currentHealth = maxHealth;
        
        // 필요한 서비스들을 한 번만 가져와서 캐싱
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        uiManager = ServiceLocator.Instance.GetService<UIManager>();
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // 캐싱된 참조 사용으로 빠른 접근
        audioManager?.PlaySFX("TakeDamage");
        uiManager?.UpdateHealthBar(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        audioManager?.PlaySFX("Death");
        gameManager?.OnPlayerDeath();
    }
}
```

## 디버깅 및 모니터링

### 1. 등록된 서비스 확인

```csharp
// Inspector에서 우클릭 메뉴로 확인 가능
[ContextMenu("등록된 서비스 목록 출력")]
public void PrintRegisteredServices()
{
    ServiceLocator.Instance.PrintRegisteredServices();
}
```

### 2. 런타임에서 서비스 상태 확인

```csharp
public class ServiceDebugger : MonoBehaviour
{
    [Header("디버그 설정")]
    [SerializeField] private bool showDebugInfo = true;
    
    private void Update()
    {
        if (showDebugInfo && Input.GetKeyDown(KeyCode.F1))
        {
            ServiceLocator.Instance.PrintRegisteredServices();
        }
    }
}
```

## 주의사항

### ❌ 잘못된 사용법

```csharp
// 매번 GetService 호출 - 성능상 비효율적
public class BadExample : MonoBehaviour
{
    private void Update()
    {
        // ❌ 매 프레임마다 서비스를 가져오는 것은 비효율적
        var audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        audioManager?.PlayBackgroundMusic();
    }
}
```

### ✅ 올바른 사용법

```csharp
// 서비스 참조를 캐싱하여 사용
public class GoodExample : MonoBehaviour
{
    private AudioManager audioManager; // 캐싱된 참조
    
    private void Start()
    {
        // ✅ 한 번만 가져와서 캐싱
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
    }
    
    private void Update()
    {
        // ✅ 캐싱된 참조 사용
        if (someCondition)
        {
            audioManager?.PlayBackgroundMusic();
        }
    }
}
```

## 확장 예제

### 커스텀 서비스 인터페이스

```csharp
// 인터페이스 정의
public interface IDataService
{
    void SaveData(string key, object data);
    T LoadData<T>(string key);
}

// 구현체
public class JsonDataService : MonoBehaviour, IDataService
{
    public void SaveData(string key, object data)
    {
        // JSON 저장 로직
    }
    
    public T LoadData<T>(string key)
    {
        // JSON 로드 로직
        return default(T);
    }
}

// 등록 및 사용
public class DataManager : MonoBehaviour
{
    private void Start()
    {
        var dataService = GetComponent<JsonDataService>();
        ServiceLocator.Instance.RegisterService<IDataService>(dataService);
    }
}
```

## 성능 최적화 팁

1. **서비스 참조 캐싱**: Start()나 Awake()에서 한 번만 가져와서 필드에 저장
2. **null 체크**: 서비스 사용 전 항상 null 체크 수행
3. **적절한 타이밍**: 서비스 등록은 가능한 한 빨리, 사용은 필요할 때만
4. **메모리 관리**: 불필요한 서비스는 UnregisterService로 해제

---

ServiceLocator를 활용하면 Find 사용 없이도 효율적이고 안전한 서비스 관리가 가능합니다. 🎮