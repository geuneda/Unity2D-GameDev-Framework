# Unity 2D 게임 개발 프레임워크 모음집

Unity 2D 게임 개발에 필요한 핵심 프레임워크와 유틸리티를 체계적으로 정리한 레포지터리입니다.

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

## 🎯 주요 특징

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

## 📖 사용 가이드

각 폴더별 상세한 사용법은 해당 폴더의 README.md 파일을 참조하세요.

### 핵심 매니저 초기화
```csharp
// GameManager를 통한 시스템 초기화
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // 매니저들 초기화
        AudioManager.Instance.Initialize();
        InputManager.Instance.Initialize();
        SceneManager.Instance.Initialize();
    }
}
```

### 입력 처리 예제
```csharp
public class PlayerInputController : MonoBehaviour
{
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            // 이동 처리 로직
        }
    }
}
```

## 🤝 기여하기

이 프레임워크는 지속적으로 개선되고 있습니다. 기여를 원하시면:

1. 이슈를 통해 버그 리포트나 기능 제안
2. Pull Request를 통한 코드 기여
3. 사용 후기나 개선 사항 공유

## 📄 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자유롭게 사용하고 수정하세요.

## 📞 연락처

질문이나 제안사항이 있으시면 이슈를 통해 연락해주세요.

---

**Unity 2D 게임 개발을 더 효율적으로! 🎮**