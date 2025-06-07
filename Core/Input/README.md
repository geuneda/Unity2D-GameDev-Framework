# Unity2D 프레임워크 입력 시스템 가이드

## 개요
이 입력 시스템은 Unity의 새로운 Input System을 기반으로 구축되었습니다. 플레이어 입력을 감지하고 처리하는 표준화된 방법을 제공합니다.

## 주요 기능

- 게임 플레이와 UI 입력을 분리하여 관리
- 싱글톤 패턴을 통한 전역 접근성
- 이벤트 기반 입력 처리
- 런타임 시 입력 활성화/비활성화 지원
- 모든 플랫폼(키보드, 게임패드, 모바일 터치 등) 지원

## 설치 및 설정 방법

1. InputManager 생성하기:
   - Hierarchy 창에서 우클릭 > Create Empty 선택
   - 새로 생성된 게임 오브젝트의 이름을 "InputManager"로 변경
   - InputManager 컴포넌트 추가: Add Component > Unity2DFramework > Core > Input > InputManager
   - "Don't Destroy On Load" 설정이 활성화되어 있는지 확인

2. InputSystem_Actions 에셋 설정하기:
   - InputSystem_Actions.inputactions 파일을 Resources 폴더에 복사 또는 생성
   - 필요한 액션 맵과 액션들을 정의 (Player, UI 등)
   - 기본 제공되는 액션들: Move, Jump, Attack, Interact, Pause, Next, Previous, Sprint

3. PlayerInputController 연결하기:
   - 플레이어 게임 오브젝트에 PlayerInputController 컴포넌트 추가
   - 필요한 이벤트 핸들러 연결 (OnMoveInput, OnJumpInput 등)

## 사용 예제

### 기본 설정

```csharp
// 플레이어 컨트롤러 코드에서 입력 이벤트 연결
private PlayerInputController inputController;

private void Awake()
{
    inputController = GetComponent<PlayerInputController>();
}

private void OnEnable()
{
    // 입력 이벤트 구독
    if (inputController != null)
    {
        inputController.OnMoveInput += HandleMove;
        inputController.OnJumpInput += HandleJump;
        inputController.OnAttackInput += HandleAttack;
    }
}

private void OnDisable()
{
    // 입력 이벤트 구독 해제
    if (inputController != null)
    {
        inputController.OnMoveInput -= HandleMove;
        inputController.OnJumpInput -= HandleJump;
        inputController.OnAttackInput -= HandleAttack;
    }
}

// 입력 처리 메서드들
private void HandleMove(Vector2 moveInput)
{
    // 이동 로직
}

private void HandleJump()
{
    // 점프 로직
}

private void HandleAttack()
{
    // 공격 로직
}
```

### UI 모드와 게임 모드 전환

```csharp
// 메뉴 열기
public void OpenMenu()
{
    // UI 모드로 전환 (UI 입력만 활성화)
    inputController.EnableUIMode();
    
    // 메뉴 UI 표시
    menuPanel.SetActive(true);
}

// 메뉴 닫기
public void CloseMenu()
{
    // 게임 모드로 전환 (게임 입력 활성화)
    inputController.EnableGameMode();
    
    // 메뉴 UI 숨기기
    menuPanel.SetActive(false);
}
```

## 주의사항

- 이 시스템은 Unity의 새로운 Input System에 의존합니다.
- 프로젝트에 Input System 패키지가 설치되어 있어야 합니다.
- 스크립트에 정의된 입력 액션 이름이 InputSystem_Actions.inputactions 파일의 액션 이름과 일치해야 합니다.
- InputManager는 씬당 하나만 존재해야 합니다.

## 커스터마이징

입력 시스템은 다음과 같은 방법으로 확장할 수 있습니다:

1. InputSystem_Actions 파일에 새로운 액션 추가
2. InputManager 클래스에 새 액션 처리 로직 추가
3. PlayerInputController에 새 이벤트 추가
4. 플레이어 컨트롤러에서 새 이벤트 구독 및 처리 