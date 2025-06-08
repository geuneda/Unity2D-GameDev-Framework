# Unity 2D 게임 개발 프레임워크 모음집

Unity 2D 게임 개발에 필요한 핵심 프레임워크와 유틸리티를 체계적으로 정리한 레포지터리입니다.

## 📋 주요 특징

### ⚡ 성능 최적화
- **Find 사용 금지**: 모든 참조는 캐싱 또는 의존성 주입 방식 사용
- **컴포넌트 참조 캐싱**: 성능을 위한 참조 최적화
- **오브젝트 풀링**: 메모리 효율적인 오브젝트 관리
- **가비지 컬렉션 최소화**: 메모리 할당 최적화
- **ServiceLocator 패턴**: 중앙집중식 서비스 관리로 Find 사용 완전 제거
- **EventManager 시스템**: 이벤트 기반 통신으로 컴포넌트 간 결합도 최소화
- **Addressable 에셋 관리**: Resources 폴더 대신 효율적인 에셋 로딩 시스템

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
│   │   ├── ServiceLocator.cs      # 중앙집중식 서비스 관리
│   │   ├── EventManager.cs        # 이벤트 기반 통신 시스템
│   │   ├── AddressableManager.cs  # Addressable 에셋 관리
│   │   ├── GameManager.cs         # 게임 전체 관리
│   │   ├── AudioManager.cs        # 오디오 관리
│   │   └── PoolManager.cs         # 오브젝트 풀링
│   ├── Assets/             # 에셋 관리 시스템
│   │   ├── AddressableId.cs       # 타입 안전한 에셋 ID
│   │   ├── AddressableHelper.cs   # Addressable 헬퍼
│   │   └── README.md               # Addressable 시스템 가이드
│   ├── Input/              # 입력 시스템
│   ├── Audio/              # 오디오 시스템
│   └── Scene/              # 씬 관리
├── Editor/                 # 에디터 도구
│   └── AddressableIdGenerator.cs  # Addressable ID 자동 생성
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
        ├── ServiceLocatorExample.cs  # ServiceLocator 사용 예제
        ├── EventManagerExample.cs    # EventManager 사용 예제
        └── AddressableExample.cs     # Addressable 사용 예제
```

## 🚀 시작하기

### 요구사항
- Unity 2022.3 LTS 이상
- .NET Standard 2.1
- 새로운 Unity Input System 패키지
- DOTween (Pro 권장)
- Addressable Asset System
- UniTask (권장)

### 설치 방법
1. 이 레포지터리를 클론하거나 다운로드
2. Unity 프로젝트에 필요한 스크립트 복사
3. Package Manager에서 필수 패키지 설치:
   - Input System
   - Addressable Asset System
   - DOTween (Asset Store)
   - UniTask (Package Manager)

## ✨ 주요 기능

### 🏗️ ServiceLocator - 중앙집중식 서비스 관리
Find 사용을 완전히 제거하고 성능을 최적화하는 서비스 관리 시스템:

```csharp
// 서비스 등록 (게임 초기화 시)
ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
ServiceLocator.Instance.RegisterService<UIManager>(uiManager);

// 서비스 사용 (캐싱된 참조로 빠른 접근)
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

### 📡 EventManager - 이벤트 기반 통신 시스템
컴포넌트 간 결합도를 최소화하는 Publisher-Subscriber 패턴 구현:

```csharp
// 이벤트 구독 (리스너 등록)
EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
EventManager.Subscribe(GameEventType.DamageDealt, OnDamageDealt);

// 이벤트 발생 (다른 컴포넌트들에게 알림)
var damageData = new DamageData { amount = 50, source = gameObject };
EventManager.Dispatch(GameEventType.DamageDealt, damageData);

// 이벤트 처리
private void OnDamageDealt(object args)
{
    if (args is DamageData data)
    {
        // 데미지 처리 로직
        currentHealth -= data.amount;
    }
}
```

### 📦 Addressable 에셋 관리 시스템
Resources 폴더를 대체하는 효율적이고 타입 안전한 에셋 관리:

```csharp
// 타입 안전한 에셋 로드
var playerPrefab = await AddressableHelper.LoadAssetAsync<GameObject>(AddressableId.Player_Character);

// 프리팹 인스턴스화
var player = await AddressableHelper.InstantiateAsync(AddressableId.Player_Character);

// 라벨별 배치 로드
var uiAssets = await AddressableHelper.LoadAssetsByLabelAsync<GameObject>(AddressableLabel.UI);

// 필수 에셋 미리 로드
bool success = await AddressableHelper.PreloadEssentialAssetsAsync(
    progress => Debug.Log($"로드 진행률: {progress * 100:F1}%")
);

// 에셋 해제
AddressableManager.Instance.ReleaseAsset(AddressableId.Player_Character.GetAddress());
```

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
// Addressable 씬 전환
await AddressableManager.Instance.LoadSceneAsync(AddressableId.Scene_GameLevel1.GetAddress());

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

### 2. Resources 폴더 사용 금지
```csharp
// ❌ 잘못된 방법 - Resources 사용
GameObject prefab = Resources.Load<GameObject>("Prefabs/Player");

// ✅ 올바른 방법 - Addressable 사용
GameObject prefab = await AddressableHelper.LoadAssetAsync<GameObject>(AddressableId.Player_Character);
```

### 3. 참조 캐싱
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

### 4. 이벤트 기반 통신
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

### 5. 방어적 프로그래밍
```csharp
// ✅ 안전한 컴포넌트 접근
if (gameObject.TryGetComponent<Rigidbody2D>(out var rb))
{
    rb.AddForce(Vector2.up * jumpForce);
}
```

## 🔄 시스템 간 연동 예제

### ServiceLocator + EventManager + Addressable 통합 사용
```csharp
public class GameBootstrap : MonoBehaviour
{
    private async void Awake()
    {
        // 1. Addressable 시스템 초기화
        await AddressableManager.Instance.InitializeAsync();
        
        // 2. 필수 에셋 미리 로드
        await AddressableHelper.PreloadEssentialAssetsAsync();
        
        // 3. 매니저들 로드 및 ServiceLocator 등록
        var audioManager = await AddressableHelper.LoadAssetAsync<AudioManager>(AddressableId.Config_AudioSettings);
        var uiManager = await AddressableHelper.LoadAssetAsync<UIManager>(AddressableId.Config_UISettings);
        
        ServiceLocator.Instance.RegisterService<AudioManager>(audioManager);
        ServiceLocator.Instance.RegisterService<UIManager>(uiManager);
        
        // 4. EventManager로 초기화 완료 알림
        EventManager.Dispatch(GameEventType.GameStart);
    }
}

public class PlayerController : MonoBehaviour
{
    private AudioManager audioManager;
    
    private async void Start()
    {
        // ServiceLocator에서 서비스 가져오기
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        
        // EventManager로 이벤트 구독
        EventManager.Subscribe(GameEventType.DamageDealt, OnTakeDamage);
        
        // Addressable로 플레이어 무기 로드
        var weapon = await AddressableHelper.LoadAssetAsync<GameObject>(AddressableId.Weapon_Sword);
        if (weapon != null)
        {
            Instantiate(weapon, transform);
        }
    }
    
    private void OnTakeDamage(object args)
    {
        // ServiceLocator로 가져온 서비스 사용
        audioManager?.PlaySFX("PlayerHurt");
        
        // EventManager로 다른 이벤트 발생
        EventManager.Dispatch(GameEventType.PlayerHealthChanged, currentHealth);
    }
}
```

## 🛠️ 에디터 도구

### Addressable ID 자동 생성
- **메뉴**: `Unity2D Framework > Tools > Generate Addressable IDs`
- **기능**: Addressable 그룹의 에셋들을 분석하여 타입 안전한 ID 생성
- **결과**: AddressableId.cs 파일 자동 생성

## 📚 상세 가이드

- **[ServiceLocator 사용 가이드](Core/Managers/ServiceLocator/README.md)**: 중앙집중식 서비스 관리 시스템
- **[EventManager 사용 가이드](Core/Managers/EventManager/README.md)**: 이벤트 기반 통신 시스템
- **[Addressable 시스템 가이드](Core/Assets/README.md)**: 효율적인 에셋 관리 시스템
- **[입력 시스템 가이드](Core/Input/README.md)**: 새로운 Unity Input System 활용법
- **[구글 시트 데이터 관리](#unity2d-게임-개발-프레임워크---구글-시트-데이터-관리-시스템)**: 게임 데이터 관리 시스템

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

# Unity2D 게임 개발 프레임워크 - 구글 시트 데이터 관리 시스템

## 개요

Unity2D 게임 개발 프레임워크의 구글 시트 데이터 관리 시스템은 게임 데이터를 Google Spreadsheet에서 관리하고, 이를 Unity의 ScriptableObject로 쉽게 변환할 수 있는 강력한 도구입니다. 이 시스템을 사용하면 게임 밸런싱 작업이나 콘텐츠 업데이트를 프로그래머가 아닌 기획자도 쉽게 할 수 있습니다.

## 주요 기능

- **구글 스프레드시트 데이터 가져오기**: Google Sheets API를 통해 시트 데이터를 가져옵니다.
- **JSON 파일 자동 생성**: 스프레드시트 데이터를 구조화된 JSON 파일로 변환합니다.
- **데이터 클래스 자동 생성**: JSON 데이터 구조에 맞는 C# 클래스를 자동으로 생성합니다.
- **ScriptableObject 자동 생성**: 데이터를 Unity의 ScriptableObject로 변환합니다.
- **대량 처리 지원**: 여러 시트를 한 번에 처리할 수 있는 배치 기능을 제공합니다.
- **다양한 데이터 타입 지원**: int, float, string 기본 타입부터 List, Dictionary, Enum 등 복잡한 데이터 구조까지 지원합니다.

## 시스템 구성

- **GoogleSheetManager**: 구글 시트 데이터를 가져오고 처리하는 에디터 도구
- **GoogleSheetFacade**: GoogleSheetManager의 복잡한 기능을 단순화하여 제공하는 Facade 패턴 구현체
- **DynamicMenuCreator**: JSON 데이터를 ScriptableObject로 변환하는 유틸리티 클래스

## 사용 방법

### 1. 구글 스프레드시트 설정

1. [Google Apps Script](https://script.google.com/) 에서 새 프로젝트를 생성합니다.
2. 다음 스크립트를 추가하고 배포합니다:

```javascript
function doGet() {
  var ss = SpreadsheetApp.openById("YOUR_SPREADSHEET_ID");
  var sheets = ss.getSheets();
  var sheetData = [];
  
  for (var i = 0; i < sheets.length; i++) {
    var sheet = sheets[i];
    sheetData.push({
      sheetName: sheet.getName(),
      sheetId: sheet.getSheetId()
    });
  }
  
  return ContentService.createTextOutput(JSON.stringify({sheetData: sheetData}))
    .setMimeType(ContentService.MimeType.JSON);
}
```

3. 스크립트를 웹 앱으로 배포하고 생성된 URL을 복사합니다.

### 2. 에디터 도구 사용

1. Unity 에디터에서 `Tools > Unity2D Framework > Google Sheet Parser` 메뉴를 선택합니다.
2. Sheet API URL 필드에 위에서 복사한 URL을 입력합니다.
3. Sheet URL 필드에 구글 스프레드시트 URL을 입력합니다.
4. "시트 데이터 가져오기" 버튼을 클릭하여 시트 목록을 불러옵니다.
5. "모든 시트 가져오기 및 파싱" 버튼을 클릭하여 모든 시트를 처리하거나, 특정 시트를 선택한 후 "선택한 시트 파싱 및 클래스 생성" 버튼을 클릭합니다.
6. JSON 파일, 데이터 클래스, ScriptableObject 클래스가 자동 생성됩니다.
7. "JSON에서 ScriptableObject 생성 메뉴 만들기" 버튼을 클릭하여 ScriptableObject 생성 메뉴를 만듭니다.
8. `Tools > Unity2D Framework > JsonToSO` 메뉴에서 원하는 데이터 타입을 선택하여 ScriptableObject를 생성합니다.

### 3. 스프레드시트 데이터 형식

스프레드시트는 다음과 같은 형식으로 구성해야 합니다:

1. **첫 번째 행**: 필드 설명 또는 헤더 (DB_IGNORE 값을 넣으면 해당 컬럼 무시)
2. **두 번째 행**: 필드 이름 (C# 변수명으로 사용됨)
3. **세 번째 행**: 데이터 타입 (int, float, string, List<int>, Dictionary<string, int> 등)
4. **네 번째 행부터**: 실제 데이터 값

### 4. 지원하는 데이터 타입

- 기본 타입: `int`, `float`, `double`, `bool`, `string`, `byte`, `long`
- 배열 타입: `int[]`, `float[]`, `string[]`
- 리스트 타입: `List<T>` (T는 기본 타입)
- 딕셔너리 타입: `Dictionary<K, V>` (K, V는 기본 타입)
- 유니티 타입: `Vector2`
- 특수 타입: `DateTime`, `TimeSpan`, `Guid`
- 사용자 정의 Enum 타입

### 5. 데이터 형식 예시

```
// 첫 번째 행: 설명
아이디    이름     체력    공격력   속도    아이템 목록      능력치
// 두 번째 행: 필드 이름
id        name    hp      attack   speed   items           stats
// 세 번째 행: 데이터 타입
int       string  int     float    float   List<string>    Dictionary<string, float>
// 네 번째 행부터: 실제 데이터
1         용사    100     15.5     5.2     검,방패,물약    힘:10.5;민첩:8.2;지능:5.0
2         마법사  80      20.0     4.5     지팡이,로브     힘:4.2;민첩:6.0;지능:15.5
```

## 파일 구조

- `/Utils/Editor/GoogleSheetManager.cs`: 구글 시트 관리 에디터 도구
- `/Utils/Editor/DynamicMenuCreator.cs`: JSON을 ScriptableObject로 변환하는 유틸리티
- `/Resources/JsonFiles/{SheetName}.json`: 생성된 JSON 파일
- `/Resources/DataClass/{SheetName}Data.cs`: 생성된 데이터 클래스
- `/Scripts/Data/{SheetName}SO.cs`: 생성된 ScriptableObject 클래스
- `/Scripts/Utils/JsonToSO.cs`: ScriptableObject 생성 메뉴
- `/Resources/ScriptableObjects/{SheetName}/{ID}.asset`: 생성된 ScriptableObject 에셋

## 활용 예시

```csharp
// 데이터 로드 예시
public class DataManager : MonoBehaviour
{
    // 리소스 폴더에서 직접 로드
    public ItemSO GetItem(int id)
    {
        return Resources.Load<ItemSO>($"ScriptableObjects/Item/{id}");
    }
    
    // 모든 아이템 로드
    public List<ItemSO> GetAllItems()
    {
        return Resources.LoadAll<ItemSO>("ScriptableObjects/Item").ToList();
    }
}
```

## 주의 사항

- 구글 스프레드시트는 반드시 웹에서 접근 가능하도록 공유 설정되어 있어야 합니다.
- 에디터 전용 기능이므로 빌드에는 포함되지 않습니다.
- 데이터 변경 후에는 반드시 다시 파싱 작업을 수행해야 합니다.
- 대량의 데이터를 처리할 경우 시간이 소요될 수 있습니다.

## 확장 가능성

- **커스텀 파서**: 특수한 데이터 형식을 위한 커스텀 파서 추가 가능
- **자동화 스크립트**: CI/CD 파이프라인과 연동하여 데이터 자동 업데이트 가능
- **실시간 데이터 업데이트**: 런타임에서 데이터 업데이트 기능 추가 가능

---

이 시스템을 활용하면 게임 데이터 관리를 효율적으로 할 수 있으며, 기획자와 개발자 간의 협업을 원활하게 할 수 있습니다. 또한, 데이터 구조의 변경이나 추가가 용이하여 게임 개발 과정에서 유연하게 대응할 수 있습니다.