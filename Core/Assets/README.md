# Addressable 에셋 관리 시스템

Unity Addressables를 기반으로 한 효율적이고 타입 안전한 에셋 관리 시스템입니다. Resources 폴더 사용을 금지하고 모든 에셋을 Addressable로 관리하여 성능과 메모리 효율성을 극대화합니다.

## 🎯 주요 특징

- **타입 안전성**: 컴파일 타임에 에셋 주소 검증
- **자동 ID 생성**: 에디터 도구로 AddressableId 자동 생성
- **메모리 최적화**: 스마트한 에셋 로딩 및 해제
- **비동기 처리**: UniTask 기반 비동기 에셋 로딩
- **라벨 기반 관리**: 에셋 그룹화 및 배치 처리
- **통계 및 디버깅**: 로드 상태 추적 및 성능 모니터링

## 📁 구조

```
Core/Assets/
├── AddressableManager.cs      # 핵심 에셋 관리자
├── AddressableId.cs          # 에셋 ID 및 설정 정의
├── AddressableHelper.cs      # 편의 기능 제공
└── README.md                 # 이 문서

Editor/
└── AddressableIdGenerator.cs # 자동 ID 생성 도구

Examples/Scripts/
└── AddressableExample.cs     # 사용 예제
```

## 🚀 빠른 시작

### 1. Addressables 설정

```csharp
// Unity 메뉴: Window > Asset Management > Addressables > Groups
// "Create Addressables Settings" 클릭하여 초기 설정 생성
```

### 2. 기본 사용법

```csharp
using Unity2DFramework.Core.Assets;
using Unity2DFramework.Core.Managers;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        // 에셋 로드
        var playerPrefab = await AddressableHelper.LoadAssetAsync<GameObject>(AddressableId.Player_Character);
        
        // 프리팹 인스턴스화
        var player = await AddressableHelper.InstantiateAsync(AddressableId.Player_Character);
        
        // 오디오 클립 로드
        var bgm = await AddressableHelper.LoadAssetAsync<AudioClip>(AddressableId.Audio_BGM_MainTheme);
    }
}
```

### 3. ID 자동 생성

```csharp
// Unity 메뉴: Unity2D Framework > Tools > Generate Addressable IDs
// Addressable 그룹의 에셋들을 분석하여 AddressableId.cs 자동 생성
```

## 📚 상세 사용법

### AddressableManager

핵심 에셋 관리자로 모든 Addressable 작업을 담당합니다.

```csharp
// 싱글톤 인스턴스 접근
var manager = AddressableManager.Instance;

// 에셋 로드
var texture = await manager.LoadAssetAsync<Texture2D>("UI/MainBackground");

// 프리팹 인스턴스화
var enemy = await manager.InstantiateAsync("Enemies/BasicEnemy", transform);

// 씬 로드
var sceneInstance = await manager.LoadSceneAsync("Levels/Level1");

// 에셋 해제
manager.ReleaseAsset("UI/MainBackground");
manager.ReleaseInstance(enemy);

// 통계 정보
string stats = manager.GetStatistics();
Debug.Log(stats);
```

### AddressableHelper

편의 기능을 제공하는 정적 헬퍼 클래스입니다.

```csharp
// AddressableId를 사용한 로드
var weapon = await AddressableHelper.LoadAssetAsync<GameObject>(AddressableId.Weapon_Sword);

// 여러 에셋 병렬 로드
var assets = await AddressableHelper.LoadAssetsAsync<AudioClip>(
    AddressableId.Audio_SFX_Jump,
    AddressableId.Audio_SFX_Attack,
    AddressableId.Audio_SFX_Pickup
);

// 라벨별 에셋 로드
var uiAssets = await AddressableHelper.LoadAssetsByLabelAsync<GameObject>(AddressableLabel.UI);

// 필수 에셋 미리 로드
bool success = await AddressableHelper.PreloadEssentialAssetsAsync(
    progress => Debug.Log($"로드 진행률: {progress * 100:F1}%")
);

// 다운로드 크기 확인
long size = await AddressableHelper.GetDownloadSizeByLabelAsync(AddressableLabel.Audio);
Debug.Log($"오디오 에셋 크기: {size} bytes");
```

### AddressableId 시스템

타입 안전한 에셋 식별자를 제공합니다.

```csharp
// 에셋 ID 정의 (자동 생성됨)
public enum AddressableId
{
    UI_MainMenu,
    Player_Character,
    Audio_BGM_MainTheme,
    // ... 기타 에셋들
}

// 설정 정보 접근
var config = AddressableId.UI_MainMenu.GetConfig();
string address = AddressableId.UI_MainMenu.GetAddress();

// 라벨별 설정 조회
var uiConfigs = AddressableLabel.UI.GetConfigs();

// 타입별 설정 조회
var audioConfigs = AddressableConfigLookup.GetConfigsByType<AudioClip>();
```

## 🏗️ 고급 기능

### 1. 커스텀 로딩 시스템

```csharp
public class CustomLoader : MonoBehaviour
{
    [SerializeField] private AddressableId[] preloadAssets;
    [SerializeField] private Slider progressBar;
    
    private async void Start()
    {
        await PreloadAssets();
    }
    
    private async UniTask PreloadAssets()
    {
        int totalAssets = preloadAssets.Length;
        int loadedAssets = 0;
        
        foreach (var assetId in preloadAssets)
        {
            try
            {
                await AddressableHelper.LoadAssetAsync<UnityEngine.Object>(assetId);
                loadedAssets++;
                
                float progress = (float)loadedAssets / totalAssets;
                progressBar.value = progress;
                
                await UniTask.Yield(); // 프레임 양보
            }
            catch (System.Exception e)
            {
                Debug.LogError($"에셋 로드 실패: {assetId} - {e.Message}");
            }
        }
    }
}
```

### 2. 동적 에셋 관리

```csharp
public class DynamicAssetManager : MonoBehaviour
{
    private readonly Dictionary<string, UnityEngine.Object> assetCache = new();
    
    public async UniTask<T> GetAssetAsync<T>(AddressableId assetId) where T : UnityEngine.Object
    {
        string address = assetId.GetAddress();
        
        // 캐시 확인
        if (assetCache.TryGetValue(address, out var cachedAsset))
        {
            return cachedAsset as T;
        }
        
        // 새로 로드
        var asset = await AddressableHelper.LoadAssetAsync<T>(assetId);
        if (asset != null)
        {
            assetCache[address] = asset;
        }
        
        return asset;
    }
    
    public void ClearCache()
    {
        foreach (var kvp in assetCache)
        {
            AddressableManager.Instance.ReleaseAsset(kvp.Key);
        }
        assetCache.Clear();
    }
}
```

### 3. 씬 전환 시스템

```csharp
public class SceneTransitionManager : MonoBehaviour
{
    public async UniTask LoadSceneAsync(AddressableId sceneId, bool showLoadingScreen = true)
    {
        if (showLoadingScreen)
        {
            // 로딩 화면 표시
            var loadingScreen = await AddressableHelper.InstantiateAsync(AddressableId.UI_LoadingScreen);
        }
        
        try
        {
            // 현재 씬 언로드 (필요한 경우)
            // ...
            
            // 새 씬 로드
            var sceneInstance = await AddressableManager.Instance.LoadSceneAsync(
                sceneId.GetAddress(),
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
            
            Debug.Log($"씬 로드 완료: {sceneInstance.Scene.name}");
        }
        finally
        {
            if (showLoadingScreen)
            {
                // 로딩 화면 제거
                // ...
            }
        }
    }
}
```

## 🔧 에디터 도구

### AddressableIdGenerator

Addressable 그룹의 에셋들을 분석하여 AddressableId.cs 파일을 자동 생성합니다.

**사용법:**
1. `Unity2D Framework > Tools > Generate Addressable IDs` 메뉴 선택
2. 포함할 에셋 타입 선택
3. "AddressableId.cs 생성" 버튼 클릭

**생성되는 내용:**
- `AddressableId` 열거형
- `AddressableLabel` 열거형  
- `AddressableConfig` 클��스
- `AddressableConfigLookup` 정적 클래스

## 📊 성능 최적화

### 1. 메모리 관리

```csharp
// 정기적인 메모리 정리
private void Update()
{
    if (Time.frameCount % 300 == 0) // 5초마다 (60fps 기준)
    {
        AddressableHelper.OptimizeMemory();
    }
}

// 씬 전환 시 정리
private void OnDestroy()
{
    AddressableManager.Instance.ReleaseAllAssets();
}
```

### 2. 배치 로딩

```csharp
// 라벨별 배치 로딩
public async UniTask PreloadGameplayAssets()
{
    var gameplayAssets = AddressableLabel.Gameplay.GetConfigs();
    var loadTasks = new List<UniTask>();
    
    foreach (var config in gameplayAssets)
    {
        var task = AddressableManager.Instance.LoadAssetAsync<UnityEngine.Object>(config.address);
        loadTasks.Add(task.AsUniTask());
    }
    
    await UniTask.WhenAll(loadTasks);
}
```

### 3. 조건부 로딩

```csharp
// 플랫폼별 조건부 로딩
public async UniTask LoadPlatformSpecificAssets()
{
    if (Application.isMobilePlatform)
    {
        await AddressableHelper.LoadAssetsByLabelAsync<Texture2D>(AddressableLabel.Mobile);
    }
    else
    {
        await AddressableHelper.LoadAssetsByLabelAsync<Texture2D>(AddressableLabel.Desktop);
    }
}
```

## 🐛 디버깅 및 모니터링

### 1. 통계 정보 확인

```csharp
[ContextMenu("Show Statistics")]
private void ShowStatistics()
{
    // AddressableManager 통계
    Debug.Log(AddressableManager.Instance.GetStatistics());
    
    // 라벨별 통계
    AddressableHelper.PrintLabelStatistics();
    
    // 모든 설정 정보
    AddressableHelper.PrintAllConfigs();
    
    // 로드된 에셋 목록
    AddressableManager.Instance.PrintLoadedAssets();
}
```

### 2. 에셋 정보 확인

```csharp
// 특정 에셋 정보
string info = AddressableHelper.GetAssetInfo(AddressableId.Player_Character);
Debug.Log(info);

// 에셋 유효성 검사
bool isValid = AddressableHelper.IsValidAddress(AddressableId.UI_MainMenu);
if (!isValid)
{
    Debug.LogError("유효하지 않은 에셋 주소");
}
```

### 3. 런타임 모니터링

```csharp
public class AddressableMonitor : MonoBehaviour
{
    [SerializeField] private Text statisticsText;
    
    private void Update()
    {
        if (statisticsText != null && Time.frameCount % 60 == 0) // 1초마다 업데이트
        {
            statisticsText.text = AddressableManager.Instance.GetStatistics();
        }
    }
}
```

## ⚠️ 주의사항

1. **Resources 폴더 사용 금지**: 모든 에셋은 Addressable로 관리
2. **메모리 해제**: 사용하지 않는 에셋은 반드시 해제
3. **에셋 주소 변경**: AddressableId 재생성 필요
4. **빌드 전 확인**: Addressable 그룹 설정 검증
5. **플랫폼별 최적화**: 타겟 플랫폼에 맞는 설정 사용

## 🔗 관련 문서

- [Unity Addressables 공식 문서](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [UniTask 문서](https://github.com/Cysharp/UniTask)
- [Unity2D Framework 메인 문서](../../README.md)

## 📝 변경 로그

### v1.0.0
- 초기 Addressable 시스템 구현
- AddressableManager, AddressableHelper, AddressableId 추가
- 에디터 도구 및 사용 예제 포함
- 자동 ID 생성 기능 구현

---

이 시스템을 통해 Unity 2D 게임에서 효율적이고 안전한 에셋 관리를 구현할 수 있습니다. 추가 질문이나 개선 사항이 있다면 이슈를 등록해 주세요.