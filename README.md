# Unity 2D 게임 개발 프레임워크 모음집

Unity 2D 게임 개발에 필요한 핵심 프레임워크와 유틸리티를 체계적으로 정리한 레포지터리입니다.

## 📋 주요 특징

### ⚡ 성능 최적화
- **Find 사용 금지**: 모든 참조는 캐싱 또는 의존성 주입 방식 사용
- **컴포넌트 참조 캐싱**: 성능을 위한 참조 최적화
- **오브젝트 풀링**: 메모리 효율적인 오브젝트 관리
- **가비지 컬렉션 최소화**: 메모리 할당 최적화

### 🎮 입력 시스템
- **새로운 Unity Input System**: 최신 입력 시스템만 사용
- **런타임 키 리매핑**: 사용자 정의 키 설정 지원
- **멀티플랫폼 지원**: PC, 모바일, 콘솔 입력 통합 관리

### 🎨 애니메이션 시스템
- **해시 ID 최적화**: 애니메이터 파라미터 성능 최적화
- **DOTween 통합**: 강력한 트위닝 애니메이션 시스템
- **상태머신 패턴**: 체계적인 애니메이션 관리

### 🔧 개발 편의성
- **한글 주석**: 명확한 코드 이해를 위한 한글 주석
- **방어적 프로그래밍**: 안전한 컴포넌트 접근 패턴
- **확장성**: 모듈화된 구조로 쉬운 확장

## 📁 프로젝트 구조

```
Unity2D-GameDev-Framework/
├── Core/                    # 핵심 시스템
│   ├── Managers/           # 게임 매니저들
│   ├── Input/              # 입력 시스템
│   ├── Audio/              # 오디오 시스템
│   └── Scene/              # 씬 관리
├── Gameplay/               # 게임플레이 관련
│   ├── Player/             # 플레이어 시스템
│   ├── Enemy/              # 적 시스템
│   ├── Items/              # 아이템 시스템
│   └── Interaction/        # 상호작용 시스템
├── UI/                     # UI 시스템
│   ├── Framework/          # UI 프레임워크
│   ├── Components/         # UI 컴포넌트
│   └── Animations/         # UI 애니메이션
├── Animation/              # 애니메이션 시스템
│   ├── Controllers/        # 애니메이션 컨트롤러
│   ├── Tweening/           # DOTween 기반 트위닝
│   └── StateMachine/       # 애니메이션 상태머신
├── Utilities/              # 유틸리티
│   ├── Extensions/         # 확장 메서드
│   ├── Helpers/            # 헬퍼 클래스
│   ├── Patterns/           # 디자인 패턴
│   └── Tools/              # 개발 도구
├── Data/                   # 데이터 관리
│   ├── Save/               # 세이브 시스템
│   ├── Settings/           # 설정 관리
│   └── ScriptableObjects/  # ScriptableObject 기반 데이터
└── Examples/               # 사용 예제
    ├── Scenes/             # 예제 씬
    └── Scripts/            # 예제 스크립트
```

## 🚀 시작하기

### 요구사항
- Unity 2022.3 LTS 이상
- .NET Standard 2.1
- 새로운 Unity Input System 패키지
- DOTween (Pro 권장)

### 설치 방법
1. 이 레포지터리를 클론하거나 다운로드
2. Unity 프로젝트에 필요한 스크립트 복사
3. Package Manager에서 필수 패키지 설치:
   - Input System
   - DOTween (Asset Store)
   - Addressable Asset System

## ✨ 주요 기능

### 🔄 오브젝트 풀링
메모리 관리와 성능 최적화를 위한 강력한 오브젝트 풀링 시스템:

```csharp
// 풀에서 오브젝트 가져오기
GameObject enemy = PoolManager.Instance.Spawn("Enemies", "BasicEnemy", position, rotation);

// 풀에 오브젝트 반환하기
PoolManager.Instance.Despawn(enemy);
```

### 📱 UI 관리
깔끔하고 확장 가능한 UI 시스템:

```csharp
// UI 패널 표시
MainMenuPanel mainMenu = await UIManager.Instance.ShowPanel<MainMenuPanel>("UI/MainMenu", "Main");

// UI 메시지 표시
UIManager.Instance.ShowMessage("아이템을 획득했습니다!", MessageType.Success, 2.0f);
```

### 🎵 오디오 관리
직관적인 오디오 관리 시스템:

```csharp
// 배경음악 재생
AudioManager.Instance.PlayMusic("Music/MainTheme", true);

// 효과음 재생
AudioManager.Instance.PlaySFX("SFX/Explosion", 0.8f, 1.2f);
```

### 🕹️ 입력 처리
새로운 Input System을 활용한 입력 관리:

```csharp
// 입력 이벤트 등록
InputManager.Instance.RegisterAction("Player", "Jump", OnJumpInput);

// 입력 처리 콜백
private void OnJumpInput(InputAction.CallbackContext context)
{
    if (context.performed)
    {
        // 점프 로직
    }
}
```

### ⏱️ 씬 관리
원활한 씬 전환과 데이터 관리:

```csharp
// 씬 전환
await SceneController.Instance.LoadScene("GameLevel");

// 씬 간 데이터 전달
SceneController.Instance.SetPersistentData("PlayerScore", 1000);
```

### 🔧 확장 메서드
개발 편의성을 위한 다양한 확장 메서드:

```csharp
// Transform 확장
transform.SetPositionX(5f);
transform.LookAt2D(targetTransform);

// 컴포넌트 안전 접근
if (gameObject.TryGetComponent<Rigidbody2D>(out var rb))
{
    rb.AddForce(Vector2.up * 10f);
}
```

### 🎞️ DOTween 확장
애니메이션 작업을 간소화하는 DOTween 확장:

```csharp
// 오브젝트 애니메이션
await transform.ScaleSmooth(Vector3.one * 1.5f, 0.5f);

// UI 애니메이션
canvasGroup.FadeIn(0.3f);
rectTransform.BounceIn(0.5f);
```

## 📝 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자유롭게 사용하고 수정하세요.

## 📞 연락처

질문이나 제안사항이 있으시면 이슈를 통해 연락해주세요.

## 🙏 감사의 말

이 프레임워크는 다음 프로젝트에서 영감을 받았습니다:
- [Core-Game](https://github.com/CoderGamester/Core-Game)
- [Game Services](https://github.com/CoderGamester/Services)
- [UiService](https://github.com/CoderGamester/UiService)

---

**Unity 2D 게임 개발을 더 효율적으로! 🎮**