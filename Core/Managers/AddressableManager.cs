using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace Unity2DFramework.Core.Managers
{
    /// <summary>
    /// Addressable 에셋 관리자
    /// Unity Addressables를 활용한 효율적인 에셋 로딩 및 관리 시스템
    /// Resources 폴더 사용을 금지하고 모든 에셋을 Addressable로 관리
    /// </summary>
    public class AddressableManager : MonoBehaviour
    {
        private static AddressableManager instance;
        
        /// <summary>
        /// AddressableManager 싱글톤 인스턴스
        /// </summary>
        public static AddressableManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("AddressableManager");
                    instance = managerObject.AddComponent<AddressableManager>();
                    DontDestroyOnLoad(managerObject);
                }
                return instance;
            }
        }
        
        [Header("디버그 설정")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool trackLoadedAssets = true;
        
        // 로드된 에셋 추적을 위한 Dictionary
        private readonly Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();
        private readonly Dictionary<string, GameObject> instantiatedObjects = new Dictionary<string, GameObject>();
        
        // 통계 정보
        private int totalLoadRequests = 0;
        private int successfulLoads = 0;
        private int failedLoads = 0;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAddressables();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Addressables 시스템 초기화
        /// </summary>
        private async void InitializeAddressables()
        {
            try
            {
                // Addressables 초기화
                var initHandle = Addressables.InitializeAsync();
                await initHandle.ToUniTask();
                
                if (enableDebugLogs)
                {
                    Debug.Log("AddressableManager: Addressables 시스템 초기화 완료");
                }
                
                // 카탈로그 업데이트 확인
                await CheckForCatalogUpdates();
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableManager: 초기화 실패 - {e.Message}");
            }
        }
        
        /// <summary>
        /// 카탈로그 업데이트 확인 및 적용
        /// </summary>
        private async UniTask CheckForCatalogUpdates()
        {
            try
            {
                var checkHandle = Addressables.CheckForCatalogUpdates(false);
                var catalogsToUpdate = await checkHandle.ToUniTask();
                
                if (catalogsToUpdate.Count > 0)
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"AddressableManager: {catalogsToUpdate.Count}개의 카탈로그 업데이트 발견");
                    }
                    
                    var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
                    await updateHandle.ToUniTask();
                    
                    if (enableDebugLogs)
                    {
                        Debug.Log("AddressableManager: 카탈로그 업데이트 완료");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AddressableManager: 카탈로그 업데이트 확인 실패 - {e.Message}");
            }
        }
        
        /// <summary>
        /// 에셋을 비동기로 로드합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="address">에셋 주소</param>
        /// <returns>로드된 에셋</returns>
        public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("AddressableManager: 에셋 주소가 비어있습니다.");
                return null;
            }
            
            totalLoadRequests++;
            
            try
            {
                // 이미 로드된 에셋인지 확인
                if (trackLoadedAssets && loadedAssets.TryGetValue(address, out var existingHandle))
                {
                    if (existingHandle.IsValid() && existingHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (enableDebugLogs)
                        {
                            Debug.Log($"AddressableManager: 캐시된 에셋 반환 - {address}");
                        }
                        return existingHandle.Result as T;
                    }
                    else
                    {
                        // 유효하지 않은 핸들 제거
                        loadedAssets.Remove(address);
                    }
                }
                
                // 새로운 에셋 로드
                var handle = Addressables.LoadAssetAsync<T>(address);
                var result = await handle.ToUniTask();
                
                // 추적 활성화 시 핸들 저장
                if (trackLoadedAssets)
                {
                    loadedAssets[address] = handle;
                }
                
                successfulLoads++;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 에셋 로드 성공 - {address} ({typeof(T).Name})");
                }
                
                return result;
            }
            catch (Exception e)
            {
                failedLoads++;
                Debug.LogError($"AddressableManager: 에셋 로드 실패 - {address}\n{e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 에셋을 비동기로 로드하고 콜백을 실행합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="address">에셋 주소</param>
        /// <param name="onComplete">로드 완료 콜백</param>
        /// <param name="onFailed">로드 실패 콜백</param>
        public async void LoadAssetAsync<T>(string address, Action<T> onComplete, Action<string> onFailed = null) where T : UnityEngine.Object
        {
            try
            {
                var result = await LoadAssetAsync<T>(address);
                if (result != null)
                {
                    onComplete?.Invoke(result);
                }
                else
                {
                    onFailed?.Invoke($"에셋 로드 실패: {address}");
                }
            }
            catch (Exception e)
            {
                onFailed?.Invoke($"에셋 로드 예외: {address} - {e.Message}");
            }
        }
        
        /// <summary>
        /// 프리팹을 인스턴스화합니다.
        /// </summary>
        /// <param name="address">프리팹 주소</param>
        /// <param name="parent">부모 Transform</param>
        /// <param name="instantiateInWorldSpace">월드 공간에서 인스턴스화 여부</param>
        /// <returns>인스턴스화된 GameObject</returns>
        public async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            try
            {
                var handle = Addressables.InstantiateAsync(address, parent, instantiateInWorldSpace);
                var result = await handle.ToUniTask();
                
                // 인스턴스화된 오브젝트 추적
                if (trackLoadedAssets && result != null)
                {
                    string instanceKey = $"{address}_{result.GetInstanceID()}";
                    instantiatedObjects[instanceKey] = result;
                    loadedAssets[instanceKey] = handle;
                }
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 프리팹 인스턴스화 성공 - {address}");
                }
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableManager: 프리팹 인스턴스화 실패 - {address}\n{e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 씬을 비동기로 로드합니다.
        /// </summary>
        /// <param name="address">씬 주소</param>
        /// <param name="loadMode">로드 모드</param>
        /// <param name="activateOnLoad">로드 시 활성화 여부</param>
        /// <returns>씬 인스턴스</returns>
        public async UniTask<SceneInstance> LoadSceneAsync(string address, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true)
        {
            try
            {
                var handle = Addressables.LoadSceneAsync(address, loadMode, activateOnLoad);
                var result = await handle.ToUniTask();
                
                if (trackLoadedAssets)
                {
                    loadedAssets[address] = handle;
                }
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 씬 로드 성공 - {address}");
                }
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableManager: 씬 로드 실패 - {address}\n{e.Message}");
                return default;
            }
        }
        
        /// <summary>
        /// 씬을 언로드합니다.
        /// </summary>
        /// <param name="sceneInstance">언로드할 씬 인스턴스</param>
        public async UniTask<bool> UnloadSceneAsync(SceneInstance sceneInstance)
        {
            try
            {
                var handle = Addressables.UnloadSceneAsync(sceneInstance);
                var result = await handle.ToUniTask();
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 씬 언로드 성공 - {sceneInstance.Scene.name}");
                }
                
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableManager: 씬 언로드 실패 - {sceneInstance.Scene.name}\n{e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 특정 에셋을 해제합니다.
        /// </summary>
        /// <param name="address">해제할 에셋 주소</param>
        public void ReleaseAsset(string address)
        {
            if (loadedAssets.TryGetValue(address, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                loadedAssets.Remove(address);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 에셋 해제 - {address}");
                }
            }
        }
        
        /// <summary>
        /// 인스턴스화된 오브젝트를 해제합니다.
        /// </summary>
        /// <param name="gameObject">해제할 GameObject</param>
        public void ReleaseInstance(GameObject gameObject)
        {
            if (gameObject == null) return;
            
            string instanceKey = null;
            foreach (var kvp in instantiatedObjects)
            {
                if (kvp.Value == gameObject)
                {
                    instanceKey = kvp.Key;
                    break;
                }
            }
            
            if (!string.IsNullOrEmpty(instanceKey))
            {
                instantiatedObjects.Remove(instanceKey);
                if (loadedAssets.TryGetValue(instanceKey, out var handle))
                {
                    if (handle.IsValid())
                    {
                        Addressables.ReleaseInstance(gameObject);
                    }
                    loadedAssets.Remove(instanceKey);
                }
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 인스턴스 해제 - {gameObject.name}");
                }
            }
            else
            {
                // Addressable로 생성되지 않은 오브젝트는 일반 Destroy
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 모든 로드된 에셋을 해제합니다.
        /// </summary>
        public void ReleaseAllAssets()
        {
            foreach (var kvp in loadedAssets)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }
            
            loadedAssets.Clear();
            instantiatedObjects.Clear();
            
            if (enableDebugLogs)
            {
                Debug.Log("AddressableManager: 모든 에셋 해제 완료");
            }
        }
        
        /// <summary>
        /// 에셋 다운로드 크기를 확인합니다.
        /// </summary>
        /// <param name="address">확인할 에셋 주소</param>
        /// <returns>다운로드 크기 (바이트)</returns>
        public async UniTask<long> GetDownloadSizeAsync(string address)
        {
            try
            {
                var handle = Addressables.GetDownloadSizeAsync(address);
                var size = await handle.ToUniTask();
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 다운로드 크기 - {address}: {size} bytes");
                }
                
                return size;
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableManager: 다운로드 크기 확인 실패 - {address}\n{e.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// 에셋을 미리 다운로드합니다.
        /// </summary>
        /// <param name="address">다운로드할 에셋 주소</param>
        public async UniTask<bool> DownloadDependenciesAsync(string address)
        {
            try
            {
                var handle = Addressables.DownloadDependenciesAsync(address);
                await handle.ToUniTask();
                
                if (enableDebugLogs)
                {
                    Debug.Log($"AddressableManager: 의존성 다운로드 완료 - {address}");
                }
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AddressableManager: 의존성 다운로드 실패 - {address}\n{e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 로드 통계 정보를 반환합니다.
        /// </summary>
        /// <returns>통계 정보 문자열</returns>
        public string GetStatistics()
        {
            return $"AddressableManager 통계:\n" +
                   $"- 총 로드 요청: {totalLoadRequests}\n" +
                   $"- 성공한 로드: {successfulLoads}\n" +
                   $"- 실패한 로드: {failedLoads}\n" +
                   $"- 현재 로드된 에셋: {loadedAssets.Count}\n" +
                   $"- 인스턴스화된 오브젝트: {instantiatedObjects.Count}";
        }
        
        /// <summary>
        /// 로드된 에셋 목록을 출력합니다. (디버깅용)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public void PrintLoadedAssets()
        {
            Debug.Log("=== AddressableManager 로드된 에셋 목록 ===");
            foreach (var kvp in loadedAssets)
            {
                var status = kvp.Value.IsValid() ? kvp.Value.Status.ToString() : "Invalid";
                Debug.Log($"{kvp.Key}: {status}");
            }
            Debug.Log("==========================================");
        }
        
        private void OnDestroy()
        {
            ReleaseAllAssets();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 앱 일시정지 시 메모리 정리
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
    }
}