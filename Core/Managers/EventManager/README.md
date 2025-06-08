# EventManager - 성능 최적화된 이벤트 시스템

## 개요

EventManager는 Unity 2D 게임 개발에서 **Find 사용을 금지**하고 **컴포넌트 간 결합도를 최소화**하는 Publisher-Subscriber 패턴 기반의 이벤트 시스템입니다. 성능 최적화를 위한 캐시 배열 사용과 메모리 할당 최소화로 고성능 게임 개발을 지원합니다.

## 주요 특징

- ✅ **Find 사용 금지**: 컴포넌트 간 직접 참조 대신 이벤트 기반 통신
- ✅ **성능 최적화**: 캐시 배열 사용으로 GC 부담 최소화
- ✅ **결합도 최소화**: Publisher-Subscriber 패턴으로 느슨한 결합
- ✅ **예외 안전성**: 한 리스너의 예외가 다른 리스너에 영향 없음
- ✅ **디버깅 지원**: 상세한 통계 정보와 로깅 기능
- ✅ **메모리 효율성**: 중복 구독 방지 및 자동 정리

## 핵심 아키텍처

### Publisher-Subscriber 패턴
```csharp
// 기존 방식 (Find 사용) - ❌
GameObject player = GameObject.Find("Player");
PlayerController playerController = player.GetComponent<PlayerController>();

// EventManager 방식 - ✅
EventManager.Dispatch(GameEventType.PlayerDeath, playerData);
```

### 성능 최적화 구조
- **캐시 배열**: 리스너 배열을 캐싱하여 매번 ToArray() 호출 방지
- **지연 업데이트**: 캐시가 무효화될 때만 배열 재생성
- **메모리 풀링**: 불필요한 메모리 할당 최소화

## 사용법

### 1. 이벤트 구독 (Subscribe)

```csharp
public class HealthSystem : MonoBehaviour
{
    private void Start()
    {
        // 플레이어 데미지 이벤트 구독
        EventManager.Subscribe(GameEventType.DamageDealt, OnDamageDealt);
        EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
    }
    
    private void OnDamageDealt(object args)
    {
        if (args is DamageData damageData)
        {
            Debug.Log($"데미지 받음: {damageData.amount}");
            // 체력 감소 로직
        }
    }
    
    private void OnPlayerDeath(object args)
    {
        Debug.Log("플레이어 사망 처리");
        // 사망 처리 로직
    }
    
    private void OnDestroy()
    {
        // 반드시 구독 해제 (메모리 누수 방지)
        EventManager.Unsubscribe(GameEventType.DamageDealt, OnDamageDealt);
        EventManager.Unsubscribe(GameEventType.PlayerDeath, OnPlayerDeath);
    }
}
```

### 2. 이벤트 발생 (Dispatch)

```csharp
public class WeaponController : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    
    public void Attack()
    {
        // 데미지 데이터 생성
        var damageData = new DamageData
        {
            amount = damage,
            source = gameObject,
            position = transform.position
        };
        
        // 데미지 이벤트 발생
        EventManager.Dispatch(GameEventType.DamageDealt, damageData);
        
        // 사운드 이벤트 발생
        EventManager.Dispatch(GameEventType.PlaySound, "AttackSound");
    }
}
```

### 3. 데이터 클래스 정의

```csharp
/// <summary>
/// 데미지 이벤트 데이터
/// </summary>
[System.Serializable]
public class DamageData
{
    public int amount;
    public GameObject source;
    public Vector3 position;
    public DamageType type;
}

/// <summary>
/// 플레이어 상태 변경 데이터
/// </summary>
[System.Serializable]
public class PlayerStatusData
{
    public int currentHealth;
    public int maxHealth;
    public int currentMana;
    public int maxMana;
    public int level;
}
```

## 실제 사용 예제

### 1. 플레이어 시스템

```csharp
public class Player : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    private void Start()
    {
        currentHealth = maxHealth;
        
        // 데미지 이벤트 구독
        EventManager.Subscribe(GameEventType.DamageDealt, OnTakeDamage);
    }
    
    private void OnTakeDamage(object args)
    {
        if (args is DamageData damageData)
        {
            currentHealth -= damageData.amount;
            
            // 체력 변경 이벤트 발생
            var statusData = new PlayerStatusData
            {
                currentHealth = currentHealth,
                maxHealth = maxHealth
            };
            EventManager.Dispatch(GameEventType.PlayerHealthChanged, statusData);
            
            // 사망 체크
            if (currentHealth <= 0)
            {
                EventManager.Dispatch(GameEventType.PlayerDeath, this);
            }
        }
    }
    
    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEventType.DamageDealt, OnTakeDamage);
    }
}
```

### 2. UI 시스템

```csharp
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    
    private void Start()
    {
        // 플레이어 체력 변경 이벤트 구독
        EventManager.Subscribe(GameEventType.PlayerHealthChanged, OnHealthChanged);
    }
    
    private void OnHealthChanged(object args)
    {
        if (args is PlayerStatusData statusData)
        {
            // UI 업데이트
            float healthRatio = (float)statusData.currentHealth / statusData.maxHealth;
            healthSlider.value = healthRatio;
            healthText.text = $"{statusData.currentHealth}/{statusData.maxHealth}";
        }
    }
    
    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEventType.PlayerHealthChanged, OnHealthChanged);
    }
}
```

### 3. 오디오 시스템

```csharp
public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] soundClips;
    
    private void Start()
    {
        // 오디오 이벤트들 구독
        EventManager.Subscribe(GameEventType.PlaySound, OnPlaySound);
        EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
    }
    
    private void OnPlaySound(object args)
    {
        if (args is string soundName)
        {
            // 사운드 재생 로직
            PlaySound(soundName);
        }
    }
    
    private void OnPlayerDeath(object args)
    {
        PlaySound("PlayerDeathSound");
    }
    
    private void PlaySound(string soundName)
    {
        // 사운드 클립 찾기 및 재생
        var clip = Array.Find(soundClips, c => c.name == soundName);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEventType.PlaySound, OnPlaySound);
        EventManager.Unsubscribe(GameEventType.PlayerDeath, OnPlayerDeath);
    }
}
```

### 4. 게임 매니저 통합

```csharp
public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // 게임 시작 이벤트 발생
        EventManager.Dispatch(GameEventType.GameStart);
        
        // 게임 관련 이벤트들 구독
        EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
        EventManager.Subscribe(GameEventType.WaveComplete, OnWaveComplete);
    }
    
    private void OnPlayerDeath(object args)
    {
        // 게임 오버 처리
        StartCoroutine(GameOverSequence());
    }
    
    private void OnWaveComplete(object args)
    {
        // 다음 웨이브 시작
        StartNextWave();
    }
    
    private IEnumerator GameOverSequence()
    {
        // 게임 종료 이벤트 발생
        EventManager.Dispatch(GameEventType.GameEnd);
        
        yield return new WaitForSeconds(2f);
        
        // 씬 전환 등 처리
    }
    
    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEventType.PlayerDeath, OnPlayerDeath);
        EventManager.Unsubscribe(GameEventType.WaveComplete, OnWaveComplete);
    }
}
```

## 고급 사용 패턴

### 1. 조건부 이벤트 처리

```csharp
public class ConditionalListener : MonoBehaviour
{
    [SerializeField] private bool isActive = true;
    
    private void Start()
    {
        EventManager.Subscribe(GameEventType.DamageDealt, OnDamageDealt);
    }
    
    private void OnDamageDealt(object args)
    {
        // 조건부 처리
        if (!isActive) return;
        
        if (args is DamageData damageData)
        {
            // 특정 조건에서만 처리
            if (damageData.amount > 50)
            {
                // 큰 데미지일 때만 특별 효과
                EventManager.Dispatch(GameEventType.PlaySound, "CriticalHitSound");
            }
        }
    }
}
```

### 2. 이벤트 체이닝

```csharp
public class EventChainExample : MonoBehaviour
{
    private void Start()
    {
        EventManager.Subscribe(GameEventType.EnemyDeath, OnEnemyDeath);
    }
    
    private void OnEnemyDeath(object args)
    {
        // 적 사망 시 연쇄 이벤트 발생
        EventManager.Dispatch(GameEventType.PlaySound, "EnemyDeathSound");
        EventManager.Dispatch(GameEventType.ItemDrop, transform.position);
        
        // 경험치 획득 이벤트
        var expData = new ExperienceData { amount = 100 };
        EventManager.Dispatch(GameEventType.PlayerLevelUp, expData);
    }
}
```

### 3. 씬 전환 시 정리

```csharp
public class SceneManager : MonoBehaviour
{
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 앱 일시정지 시 모든 이벤트 정리
            EventManager.UnsubscribeAll();
        }
    }
    
    private void OnDestroy()
    {
        // 씬 전환 시 모든 이벤트 정리
        EventManager.UnsubscribeAll();
    }
}
```

## 디버깅 및 모니터링

### 1. 통계 정보 확인

```csharp
public class EventDebugger : MonoBehaviour
{
    [Header("디버그 설정")]
    [SerializeField] private bool showStatistics = true;
    [SerializeField] private KeyCode statisticsKey = KeyCode.F4;
    
    private void Update()
    {
        if (showStatistics && Input.GetKeyDown(statisticsKey))
        {
            // 통계 정보 출력
            Debug.Log(EventManager.GetStatistics());
            
            // 구독 현황 출력
            EventManager.PrintAllSubscriptions();
        }
    }
}
```

### 2. 특정 이벤트 모니터링

```csharp
public class EventMonitor : MonoBehaviour
{
    [SerializeField] private GameEventType[] monitoredEvents;
    
    private void Start()
    {
        // 모니터링할 이벤트들 구독
        foreach (var eventType in monitoredEvents)
        {
            EventManager.Subscribe(eventType, OnMonitoredEvent);
        }
    }
    
    private void OnMonitoredEvent(object args)
    {
        Debug.Log($"[EventMonitor] 이벤트 감지: {args}");
    }
}
```

## 성능 최적화 팁

### 1. 이벤트 데이터 최적화
```csharp
// ❌ 매번 새 객체 생성
EventManager.Dispatch(GameEventType.DamageDealt, new DamageData { amount = 10 });

// ✅ 객체 재사용
private DamageData reusableDamageData = new DamageData();

public void DealDamage(int amount)
{
    reusableDamageData.amount = amount;
    EventManager.Dispatch(GameEventType.DamageDealt, reusableDamageData);
}
```

### 2. 구독 해제 자동화
```csharp
public abstract class EventSubscriber : MonoBehaviour
{
    protected abstract void SubscribeToEvents();
    protected abstract void UnsubscribeFromEvents();
    
    protected virtual void Start()
    {
        SubscribeToEvents();
    }
    
    protected virtual void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}
```

## 주의사항

### ❌ 잘못된 사용법

```csharp
// 1. 구독 해제 누락 - 메모리 누수 발생
public class BadExample : MonoBehaviour
{
    private void Start()
    {
        EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
        // OnDestroy에서 구독 해제 누락!
    }
}

// 2. 순환 이벤트 - 무한 루프 위험
private void OnEventA(object args)
{
    EventManager.Dispatch(GameEventType.EventB, args); // 위험!
}
```

### ✅ 올바른 사용법

```csharp
public class GoodExample : MonoBehaviour
{
    private void Start()
    {
        EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
    }
    
    private void OnPlayerDeath(object args)
    {
        // 안전한 이벤트 처리
        if (args is Player player)
        {
            // 처리 로직
        }
    }
    
    private void OnDestroy()
    {
        // 반드시 구독 해제
        EventManager.Unsubscribe(GameEventType.PlayerDeath, OnPlayerDeath);
    }
}
```

## 확장 가능성

### 커스텀 이벤트 타입 추가
```csharp
// GameEventType enum에 새 이벤트 추가
public enum GameEventType
{
    // 기존 이벤트들...
    
    // 새로운 커스텀 이벤트
    CustomSkillActivated,
    CustomBossPhaseChanged,
    CustomWeatherChanged
}
```

### 이벤트 필터링 시스템
```csharp
public static class EventFilter
{
    public static void SubscribeWithFilter<T>(GameEventType eventType, 
        EventListener listener, Func<T, bool> filter) where T : class
    {
        EventManager.Subscribe(eventType, (args) =>
        {
            if (args is T typedArgs && filter(typedArgs))
            {
                listener(args);
            }
        });
    }
}
```

---

EventManager를 활용하면 Find 사용 없이도 효율적이고 안전한 컴포넌트 간 통신이 가능합니다. 🎮