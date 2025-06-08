using UnityEngine;
using UnityEngine.UI;
using Unity2DFramework.Core.Assets;
using Unity2DFramework.Core.Managers;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Unity2DFramework.Examples
{
    /// <summary>
    /// Addressable 시스템 사용 예제
    /// AddressableManager와 AddressableHelper의 다양한 기능을 시연합니다.
    /// </summary>
    public class AddressableExample : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Button loadAssetButton;
        [SerializeField] private Button instantiatePrefabButton;
        [SerializeField] private Button loadSceneButton;
        [SerializeField] private Button preloadAssetsButton;
        [SerializeField] private Button releaseAssetsButton;
        [SerializeField] private Button showStatisticsButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Transform spawnParent;
        
        [Header("테스트 설정")]
        [SerializeField] private AddressableId testAssetId = AddressableId.UI_MainMenu;
        [SerializeField] private AddressableId testPrefabId = AddressableId.Player_Character;
        [SerializeField] private AddressableId testSceneId = AddressableId.Scene_GameLevel1;
        [SerializeField] private AddressableLabel testLabel = AddressableLabel.UI;
        
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private AudioClip loadedAudioClip;
        
        private void Start()
        {
            InitializeUI();
            UpdateStatus("Addressable 시스템 준비 완료");
        }
        
        /// <summary>
        /// UI 버튼 이벤트 초기화
        /// </summary>
        private void InitializeUI()
        {
            if (loadAssetButton != null)
                loadAssetButton.onClick.AddListener(() => LoadAssetExample().Forget());
                
            if (instantiatePrefabButton != null)
                instantiatePrefabButton.onClick.AddListener(() => InstantiatePrefabExample().Forget());
                
            if (loadSceneButton != null)
                loadSceneButton.onClick.AddListener(() => LoadSceneExample().Forget());
                
            if (preloadAssetsButton != null)
                preloadAssetsButton.onClick.AddListener(() => PreloadAssetsExample().Forget());
                
            if (releaseAssetsButton != null)
                releaseAssetsButton.onClick.AddListener(ReleaseAssetsExample);
                
            if (showStatisticsButton != null)
                showStatisticsButton.onClick.AddListener(ShowStatisticsExample);
                
            if (progressSlider != null)
                progressSlider.value = 0f;
        }
        
        /// <summary>
        /// 에셋 로드 예제
        /// </summary>
        private async UniTaskVoid LoadAssetExample()
        {
            UpdateStatus("에셋 로딩 중...");
            
            try
            {
                // 방법 1: AddressableHelper 사용
                var audioClip = await AddressableHelper.LoadAssetAsync<AudioClip>(AddressableId.Audio_BGM_MainTheme);
                if (audioClip != null)
                {
                    loadedAudioClip = audioClip;
                    UpdateStatus($"오디오 클립 로드 성공: {audioClip.name}");
                    
                    // 오디오 재생 (AudioSource가 있다면)
                    var audioSource = GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.clip = audioClip;
                        audioSource.Play();
                    }
                }
                else
                {
                    UpdateStatus("오디오 클립 로드 실패");
                }
                
                // 방법 2: AddressableManager 직접 사용
                var texture = await AddressableManager.Instance.LoadAssetAsync<Texture2D>("UI/MainMenuBackground");
                if (texture != null)
                {
                    UpdateStatus($"텍스처 로드 성공: {texture.name}");
                }
                
                // 방법 3: 콜백 방식
                AddressableHelper.LoadAssetAsync<Material>(
                    AddressableId.UI_MainMenu,
                    material => Debug.Log($"머티리얼 로드 성공: {material.name}"),
                    error => Debug.LogError($"머티리얼 로드 실패: {error}")
                );
            }
            catch (System.Exception e)
            {
                UpdateStatus($"에셋 로드 오류: {e.Message}");
                Debug.LogError($"AddressableExample: 에셋 로드 오류 - {e}");
            }
        }
        
        /// <summary>
        /// 프리팹 인스턴스화 예제
        /// </summary>
        private async UniTaskVoid InstantiatePrefabExample()
        {
            UpdateStatus("프리팹 인스턴스화 중...");
            
            try
            {
                // AddressableHelper를 사용한 인스턴스화
                var spawnedObject = await AddressableHelper.InstantiateAsync(
                    testPrefabId, 
                    spawnParent != null ? spawnParent : transform
                );
                
                if (spawnedObject != null)
                {
                    spawnedObjects.Add(spawnedObject);
                    
                    // 랜덤 위치에 배치
                    Vector3 randomPosition = new Vector3(
                        Random.Range(-5f, 5f),
                        Random.Range(-3f, 3f),
                        0f
                    );
                    spawnedObject.transform.localPosition = randomPosition;
                    
                    UpdateStatus($"프리팹 인스턴스화 성공: {spawnedObject.name} (총 {spawnedObjects.Count}개)");
                }
                else
                {
                    UpdateStatus("프리팹 인스턴스화 실패");
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"프리팹 인스턴스화 오류: {e.Message}");
                Debug.LogError($"AddressableExample: 프리팹 인스턴스화 오류 - {e}");
            }
        }
        
        /// <summary>
        /// 씬 로드 예제
        /// </summary>
        private async UniTaskVoid LoadSceneExample()
        {
            UpdateStatus("씬 로딩 중...");
            
            try
            {
                var sceneInstance = await AddressableManager.Instance.LoadSceneAsync(
                    testSceneId.GetAddress(),
                    UnityEngine.SceneManagement.LoadSceneMode.Additive
                );
                
                if (sceneInstance.Scene.IsValid())
                {
                    UpdateStatus($"씬 로드 성공: {sceneInstance.Scene.name}");
                }
                else
                {
                    UpdateStatus("씬 로드 실패");
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"씬 로드 오류: {e.Message}");
                Debug.LogError($"AddressableExample: 씬 로드 오류 - {e}");
            }
        }
        
        /// <summary>
        /// 에셋 미리 로드 예제
        /// </summary>
        private async UniTaskVoid PreloadAssetsExample()
        {
            UpdateStatus("에셋 미리 로드 중...");
            
            try
            {
                // 진행률 콜백과 함께 라벨별 에셋 ���리 로드
                bool success = await AddressableHelper.PreloadAssetsByLabelAsync(
                    testLabel,
                    progress =>
                    {
                        if (progressSlider != null)
                        {
                            progressSlider.value = progress;
                        }
                        UpdateStatus($"미리 로드 진행률: {(progress * 100):F1}%");
                    }
                );
                
                if (success)
                {
                    UpdateStatus("모든 에셋 미리 로드 완료");
                }
                else
                {
                    UpdateStatus("일부 에셋 미리 로드 실패");
                }
                
                // 필수 에셋 미리 로드
                bool essentialSuccess = await AddressableHelper.PreloadEssentialAssetsAsync(
                    progress => Debug.Log($"필수 에셋 로드 진행률: {(progress * 100):F1}%")
                );
                
                if (essentialSuccess)
                {
                    Debug.Log("필수 에셋 미리 로드 완료");
                }
            }
            catch (System.Exception e)
            {
                UpdateStatus($"미리 로드 오류: {e.Message}");
                Debug.LogError($"AddressableExample: 미리 로드 오류 - {e}");
            }
        }
        
        /// <summary>
        /// 에셋 해제 예제
        /// </summary>
        private void ReleaseAssetsExample()
        {
            try
            {
                // 인스턴스화된 오브젝트들 해제
                foreach (var obj in spawnedObjects)
                {
                    if (obj != null)
                    {
                        AddressableManager.Instance.ReleaseInstance(obj);
                    }
                }
                spawnedObjects.Clear();
                
                // 특정 에셋 해제
                if (loadedAudioClip != null)
                {
                    AddressableManager.Instance.ReleaseAsset(AddressableId.Audio_BGM_MainTheme.GetAddress());
                    loadedAudioClip = null;
                }
                
                // 메모리 최적화
                AddressableHelper.OptimizeMemory();
                
                UpdateStatus("에셋 해제 완료");
            }
            catch (System.Exception e)
            {
                UpdateStatus($"에셋 해제 오류: {e.Message}");
                Debug.LogError($"AddressableExample: 에셋 해제 오류 - {e}");
            }
        }
        
        /// <summary>
        /// 통계 정보 표시 예제
        /// </summary>
        private void ShowStatisticsExample()
        {
            try
            {
                // AddressableManager 통계
                string statistics = AddressableManager.Instance.GetStatistics();
                Debug.Log(statistics);
                
                // 에셋 정보 출력
                string assetInfo = AddressableHelper.GetAssetInfo(testAssetId);
                Debug.Log(assetInfo);
                
                // 라벨별 통계 출력
                AddressableHelper.PrintLabelStatistics();
                
                // 모든 설정 정보 출력
                AddressableHelper.PrintAllConfigs();
                
                UpdateStatus("통계 정보를 콘솔에 출력했습니다.");
            }
            catch (System.Exception e)
            {
                UpdateStatus($"통계 정보 오류: {e.Message}");
                Debug.LogError($"AddressableExample: 통계 정보 오류 - {e}");
            }
        }
        
        /// <summary>
        /// 고급 사용 예제들
        /// </summary>
        [ContextMenu("고급 예제 실행")]
        private async void AdvancedExamples()
        {
            // 1. 여러 에셋 병렬 로드
            var assets = await AddressableHelper.LoadAssetsAsync<AudioClip>(
                AddressableId.Audio_BGM_MainTheme,
                AddressableId.Audio_BGM_Battle,
                AddressableId.Audio_SFX_Jump
            );
            Debug.Log($"병렬 로드 완료: {assets.Length}개 에셋");
            
            // 2. 타입별 모든 에셋 로드
            var allAudioClips = await AddressableHelper.LoadAllAssetsByTypeAsync<AudioClip>();
            Debug.Log($"모든 오디오 클립 로드: {allAudioClips.Count}개");
            
            // 3. 다운로드 크기 확인
            long downloadSize = await AddressableHelper.GetDownloadSizeByLabelAsync(AddressableLabel.Audio);
            Debug.Log($"오디오 라벨 다운로드 크기: {downloadSize} bytes");
            
            // 4. 에셋 유효성 검사
            bool isValid = AddressableHelper.IsValidAddress(testAssetId);
            Debug.Log($"에셋 주소 유효성: {isValid}");
        }
        
        /// <summary>
        /// 키보드 입력으로 테스트 기능 실행
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                LoadAssetExample().Forget();
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                InstantiatePrefabExample().Forget();
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                LoadSceneExample().Forget();
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                PreloadAssetsExample().Forget();
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                ReleaseAssetsExample();
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                ShowStatisticsExample();
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                AdvancedExamples();
            }
        }
        
        /// <summary>
        /// 상태 텍스트 업데이트
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }
            
            Debug.Log($"AddressableExample: {message}");
        }
        
        /// <summary>
        /// 컴포넌트 해제 시 정리 작업
        /// </summary>
        private void OnDestroy()
        {
            // 생성된 오브젝트들 정리
            foreach (var obj in spawnedObjects)
            {
                if (obj != null)
                {
                    AddressableManager.Instance.ReleaseInstance(obj);
                }
            }
            spawnedObjects.Clear();
        }
        
        /// <summary>
        /// 에디터에서 정보 표시
        /// </summary>
        private void OnValidate()
        {
            #if UNITY_EDITOR
            // 에디터에서 선택된 에셋 정보 표시
            if (Application.isPlaying)
            {
                return;
            }
            
            var config = testAssetId.GetConfig();
            if (config != null)
            {
                Debug.Log($"선택된 에셋: {config.address} ({config.assetType.Name})");
            }
            #endif
        }
    }
}