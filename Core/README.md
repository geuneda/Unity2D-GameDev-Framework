# Core 시스템

Unity 2D 게임의 핵심 시스템들을 모아놓은 폴더입니다.

## 📁 구조

- **Managers/**: 게임의 전반적인 관리를 담당하는 매니저들
  - **ServiceLocator.cs**: 중앙집중식 서비스 관리 시스템 (Find 사용 금지)
  - **GameManager.cs**: 게임 전체 상태 및 흐름 관리
  - **AudioManager.cs**: 오디오 재생 및 관리
  - **PoolManager.cs**: 오브젝트 풀링 시스템
- **Input/**: 새로운 Unity Input System 기반 입력 관리
- **Audio/**: 오디오 재생 및 관리 시스템
- **Scene/**: 씬 전환 및 로딩 관리

## 🏗️ ServiceLocator - 핵심 아키텍처

**Find 사용을 완전히 제거**하고 성능을 최적화하는 중앙집중식 서비스 관리 시스템입니다.

### 주요 특징
- ✅ **Find 사용 금지**: GameObject.Find 대신 서비스 등록/조회 방식
- ✅ **성능 최적화**: 캐싱된 참조를 통한 빠른 서비스 접근
- ✅ **자동 생성**: 씬에 없으면 자동으로 생성되는 안전한 구조
- ✅ **씬 간 유지**: DontDestroyOnLoad로 씬 전환 시에도 유지

### 기본 사용법

```csharp
// 1. 서비스 등록 (게임 초기화 시)
ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
ServiceLocator.Instance.RegisterService<UIManager>(uiManager);

// 2. 서비스 사용 (캐싱된 참조로 빠른 접근)
public class WeaponController : MonoBehaviour
{
    private AudioManager audioManager; // 캐싱된 참조
    
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

### 자동 탐지 기능

```csharp
// 서비스가 등록되어 있지 않으면 씬에서 자동으로 찾아서 등록
PlayerController player = ServiceLocator.Instance.GetOrFindService<PlayerController>();
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

### InputManager
새로운 Unity Input System을 기반으로 한 입력 관리자입니다.

```csharp
// 입력 이벤트 구독
InputManager.Instance.OnMove += HandlePlayerMove;
InputManager.Instance.OnJump += HandlePlayerJump;
InputManager.Instance.OnAttack += HandlePlayerAttack;

// 현재 이동 입력값 가져오기
Vector2 moveInput = InputManager.Instance.GetMoveInput();

// 특정 액션이 눌려있는지 확인
bool isJumpPressed = InputManager.Instance.IsActionPressed("Jump");

// 입력 모드 전환
InputManager.Instance.EnablePlayerInput(); // 플레이어 입력 활성화
InputManager.Instance.EnableUIInput();     // UI 입력 활성화
```

### SceneTransitionManager
부드러운 씬 전환과 로딩 화면을 제공합니다.

```csharp
// 씬 로드 (페이드 효과와 로딩 화면 포함)
SceneTransitionManager.Instance.LoadScene("GameScene", showLoadingScreen: true);

// 현재 씬 재시작
SceneTransitionManager.Instance.RestartCurrentScene();

// 메인 메뉴로 이동
SceneTransitionManager.Instance.LoadMainMenu();

// 씬 로딩 이벤트 구독
SceneTransitionManager.Instance.OnSceneLoadStart += (sceneName) => 
{
    Debug.Log($"씬 로딩 시작: {sceneName}");
};
```

## ⚙️ 설정 방법

### 1. ServiceLocator 기반 초기화
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
    }
}
```

### 2. InputManager 설정
1. Package Manager에서 Input System 패키지 설치
2. Input Actions 에셋 생성 (Create > Input Actions)
3. 다음 액션 맵들을 생성:
   - **Player**: Move, Jump, Attack, Interact, Pause
   - **UI**: Navigate, Submit, Cancel

### 3. AudioManager 설정
1. Audio Mixer 생성 및 그룹 설정:
   - Master
   - BGM
   - SFX
   - UI
2. AudioManager 컴포넌트에 Mixer Group들 할당
3. AudioSource들을 설정

### 4. SceneTransitionManager 설정
1. 페이드용 Canvas와 CanvasGroup 생성
2. 로딩 화면 UI 설정 (Slider, Text 포함)
3. SceneTransitionManager 컴포넌트에 UI 요소들 할당

## 🔧 커스터마이징

각 매니저들은 확장 가능하도록 설계되었습니다:

```csharp
// GameManager 확장 예제
public class MyGameManager : GameManager
{
    protected override void InitializeManagers()
    {
        base.InitializeManagers();
        // 추가 초기화 로직
    }
}

// AudioManager에 새로운 기능 추가
public static class AudioManagerExtensions
{
    public static void PlayRandomSFX(this AudioManager audioManager, AudioClip[] clips)
    {
        if (clips.Length > 0)
        {
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            audioManager.PlaySFX(randomClip);
        }
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
    }
}
```

### 3. 방어적 프로그래밍
```csharp
// ✅ 안전한 컴포넌트 접근
if (gameObject.TryGetComponent<Rigidbody2D>(out var rb))
{
    rb.AddForce(Vector2.up * jumpForce);
}
```

## 📝 주의사항

1. **Find 사용 금지**: 모든 참조는 ServiceLocator를 통해 관리됩니다
2. **참조 캐싱**: 서비스 참조는 Start()나 Awake()에서 한 번만 가져와서 캐싱
3. **싱글톤 패턴**: 모든 매니저는 싱글톤으로 구현되어 있습니다
4. **이벤트 구독 해제**: OnDestroy에서 이벤트 구독을 해제해야 합니다
5. **성능 최적화**: 애니메이터 파라미터는 해시 ID를 사용합니다

## 🚀 시작하기

1. ServiceLocator 기반 초기화 스크립트 작성
2. GameManager를 씬에 배치
3. 필요한 매니저들을 ServiceLocator에 등록
4. 각 매니저의 설정 완료
5. 게임 시작 시 `GameManager.Instance.StartGame()` 호출

## 📚 상세 가이드

- **[ServiceLocator 상세 가이드](Managers/ServiceLocator/README.md)**: 중앙집중식 서비스 관리 시스템 완전 가이드