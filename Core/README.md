# Core 시스템

Unity 2D 게임의 핵심 시스템들을 모아놓은 폴더입니다.

## 📁 구조

- **Managers/**: 게임의 전반적인 관리를 담당하는 매니저들
  - **ServiceLocator.cs**: 중앙집중식 서비스 관리 시스템 (Find 사용 금지)
  - **EventManager.cs**: 이벤트 기반 통신 시스템 (Publisher-Subscriber 패턴)
  - **GameManager.cs**: 게임 전체 상태 및 흐름 관리
  - **AudioManager.cs**: 오디오 재생 및 관리
  - **PoolManager.cs**: 오브젝트 풀링 시스템
- **Input/**: 새로운 Unity Input System 기반 입력 관리
- **Audio/**: 오디오 재생 및 관리 시스템
- **Scene/**: 씬 전환 및 로딩 관리

## 🏗️ 핵심 아키텍처

### ServiceLocator - 중앙집중식 서비스 관리

**Find 사용을 완전히 제거**하고 성능을 최적화하는 중앙집중식 서비스 관리 시스템입니다.

#### 주요 특징
- ✅ **Find 사용 금지**: GameObject.Find 대신 서비스 등록/조회 방식
- ✅ **성능 최적화**: 캐싱된 참조를 통한 빠른 서비스 접근
- ✅ **자동 생성**: 씬에 없으면 자동으로 생성되는 안전한 구조
- ✅ **씬 간 유지**: DontDestroyOnLoad로 씬 전환 시에도 유지

#### 기본 사용법

```csharp
// 1. 서비스 등록 (게임 초기화 시)
ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
ServiceLocator.Instance.RegisterService<UIManager>(uiManager);

// 2. 서비스 사용 (캐싱된 참조로 빠른 접근)
public class WeaponController : MonoBehaviour
{
    private AudioManager audioManager;
    
    private void Start()
    {
        // 한 번만 가져와서 캐싱
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
    }
    
    public void FireWeapon()
    {
        // 캐싱된 참조 사용으로 빠른 접근
        audioManager?.PlaySFX("WeaponFire");
    }
}
```

### EventManager - 이벤트 기반 통신 시스템

**컴포넌트 간 결합도를 최소화**하는 Publisher-Subscriber 패턴 기반의 이벤트 시스템입니다.

#### 주요 특징
- ✅ **결합도 최소화**: 컴포넌트 간 직접 참조 대신 이벤트 기반 통신
- ✅ **성능 최적화**: 캐시 배열 사용으로 GC 부담 최소화
- ✅ **예외 안전성**: 한 리스너의 예외가 다른 리스너에 영향 없음
- ✅ **디버깅 지원**: 상세한 통계 정보와 로깅 기능

#### 기본 사용법

```csharp
// 1. 이벤트 구독 (리스너 등록)
EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
EventManager.Subscribe(GameEventType.DamageDealt, OnDamageDealt);

// 2. 이벤트 발생 (다른 컴포넌트들에게 알림)
var damageData = new DamageData { amount = 50, source = gameObject };
EventManager.Dispatch(GameEventType.DamageDealt, damageData);

// 3. 이벤트 처리
private void OnDamageDealt(object args)
{
    if (args is DamageData data)
    {
        // 데미지 처리 로직
        currentHealth -= data.amount;
    }
}

// 4. 구독 해제 (메모리 누수 방지)
private void OnDestroy()
{
    EventManager.Unsubscribe(GameEventType.PlayerDeath, OnPlayerDeath);
    EventManager.Unsubscribe(GameEventType.DamageDealt, OnDamageDealt);
}
```

## 🔄 시스템 간 연동

### ServiceLocator + EventManager 통합 사용

두 시스템을 함께 사용하여 최적의 아키텍처를 구성할 수 있습니다:

```csharp
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;
    
    private void Awake()
    {
        // 1. ServiceLocator에 서비스 등록
        ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
        ServiceLocator.Instance.RegisterService<UIManager>(uiManager);
        
        // 2. EventManager로 초기화 완료 알림
        EventManager.Dispatch(GameEventType.GameStart);
    }
}

public class PlayerController : MonoBehaviour
{
    private AudioManager audioManager;
    
    private void Start()
    {
        // ServiceLocator에서 서비스 가져오기
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        
        // EventManager로 이벤트 구독
        EventManager.Subscribe(GameEventType.DamageDealt, OnTakeDamage);
    }
    
    private void OnTakeDamage(object args)
    {
        // ServiceLocator로 가져온 서비스 사용
        audioManager?.PlaySFX("PlayerHurt");
        
        // EventManager로 다른 이벤트 발생
        EventManager.Dispatch(GameEventType.PlayerHealthChanged, currentHealth);
    }
    
    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEventType.DamageDealt, OnTakeDamage);
    }
}
```

## 🎮 주요 매니저들

### GameManager
게임의 전반적인 상태와 흐름을 관리하는 핵심 매니저입니다.

```csharp
// 게임 시작
GameManager.Instance.StartGame();

// 게임 일시정지
GameManager.Instance.PauseGame();

// 게임 재개
GameManager.Instance.ResumeGame();

// 게임 속도 조절
GameManager.Instance.SetGameSpeed(0.5f); // 슬로우 모션
```

### AudioManager
BGM, SFX, UI 사운드를 분리하여 관리합니다.

```csharp
// BGM 재생 (페이드 인 효과 포함)
AudioManager.Instance.PlayBGM(bgmClip, fadeIn: true);

// SFX 재생
AudioManager.Instance.PlaySFX(jumpSoundClip, volume: 0.8f);

// UI 사운드 재생
AudioManager.Instance.PlayUISound(buttonClickClip);

// 볼륨 조절
AudioManager.Instance.SetMasterVolume(0.7f);
AudioManager.Instance.SetBGMVolume(0.5f);
```

## ⚙️ 설정 방법

### 1. ServiceLocator + EventManager 기반 초기화
```csharp
public class GameBootstrap : MonoBehaviour
{
    [Header("Core Managers")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;
    
    private void Awake()
    {
        // 모든 서비스를 ServiceLocator에 등록
        ServiceLocator.Instance.RegisterService<GameManager>(gameManager);
        ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
        ServiceLocator.Instance.RegisterService<UIManager>(uiManager);
        
        // 이벤트 시스템 초기화 완료 알림
        EventManager.Dispatch(GameEventType.GameStart);
    }
}
```

### 2. 이벤트 기반 시스템 초기화
```csharp
public class EventSystemInitializer : MonoBehaviour
{
    private void Start()
    {
        // 핵심 이벤트들 구독
        EventManager.Subscribe(GameEventType.GameStart, OnGameStart);
        EventManager.Subscribe(GameEventType.GameEnd, OnGameEnd);
        EventManager.Subscribe(GameEventType.SceneLoad, OnSceneLoad);
    }
    
    private void OnGameStart(object args)
    {
        Debug.Log("게임 시작됨");
    }
    
    private void OnGameEnd(object args)
    {
        Debug.Log("게임 종료됨");
        // 모든 이벤트 정리
        EventManager.UnsubscribeAll();
    }
    
    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEventType.GameStart, OnGameStart);
        EventManager.Unsubscribe(GameEventType.GameEnd, OnGameEnd);
        EventManager.Unsubscribe(GameEventType.SceneLoad, OnSceneLoad);
    }
}
```

## 🎯 핵심 설계 원칙

### 1. Find 사용 금지
```csharp
// ❌ 잘못된 방법 - Find 사용
GameObject player = GameObject.Find("Player");

// ✅ 올바른 방법 - ServiceLocator 사용
PlayerController player = ServiceLocator.Instance.GetService<PlayerController>();

// ✅ 또는 EventManager 사용
EventManager.Dispatch(GameEventType.PlayerSpawn, playerData);
```

### 2. 참조 캐싱
```csharp
// ✅ 컴포넌트 참조는 반드시 캐싱
public class HealthSystem : MonoBehaviour
{
    private AudioManager audioManager; // 캐싱된 참조
    
    private void Start()
    {
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        EventManager.Subscribe(GameEventType.DamageDealt, OnDamageDealt);
    }
}
```

### 3. 이벤트 기반 통신
```csharp
// ✅ 컴포넌트 간 직접 참조 대신 이벤트 사용
public class Enemy : MonoBehaviour
{
    private void Die()
    {
        // 직접 참조 대신 이벤트로 알림
        EventManager.Dispatch(GameEventType.EnemyDeath, this);
    }
}
```

### 4. 방어적 프로그래밍
```csharp
// ✅ 안전한 컴포넌트 접근
if (gameObject.TryGetComponent<Rigidbody2D>(out var rb))
{
    rb.AddForce(Vector2.up * jumpForce);
}
```

## 📊 성능 최적화

### ServiceLocator 최적화
- **싱글톤 패턴**: 전역 접근 가능한 단일 인스턴스
- **Dictionary 캐싱**: 빠른 서비스 조회
- **자동 생성**: 필요 시에만 인스턴스 생성

### EventManager 최적화
- **캐시 배열**: 리스너 배열을 캐싱하여 GC 부담 감소
- **지연 업데이트**: 캐시가 무효화될 때만 배열 재생성
- **예외 격리**: 개별 리스너 예외 처리로 안정성 확보

## 🔧 확장 가능성

### 커스텀 서비스 추가
```csharp
// 새로운 서비스 클래스 생성
public class CustomService : MonoBehaviour
{
    public void DoSomething() { /* 로직 */ }
}

// ServiceLocator에 등록
ServiceLocator.Instance.RegisterService<CustomService>(customService);

// 다른 곳에서 사용
var customService = ServiceLocator.Instance.GetService<CustomService>();
```

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

## 📚 상세 문서

- **[ServiceLocator 상세 가이드](Managers/ServiceLocator/README.md)**: 중앙집중식 서비스 관리 시스템
- **[EventManager 상세 가이드](Managers/EventManager/README.md)**: 이벤트 기반 통신 시스템

## 🎮 실제 사용 예제

Core 시스템의 실제 사용 예제는 `Examples/Scripts/` 폴더에서 확인할 수 있습니다:

- **ServiceLocatorExample.cs**: ServiceLocator 사용 예제
- **EventManagerExample.cs**: EventManager 사용 예제

---

Core 시스템을 활용하여 Find 사용 없이도 효율적이고 안전한 Unity 2D 게임을 개발하세요! 🚀