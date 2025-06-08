using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Unity2DFramework.Core.Assets
{
    /// <summary>
    /// Addressable 시스템을 위한 헬퍼 클래스
    /// 자주 사용되는 Addressable 작업들을 간편하게 수행할 수 있는 유틸리티 메서드 제공
    /// </summary>
    public static class AddressableHelper
    {
        /// <summary>
        /// AddressableId를 사용하여 에셋을 로드합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="addressableId">Addressable ID</param>
        /// <returns>로드된 에셋</returns>
        public static async UniTask<T> LoadAssetAsync<T>(AddressableId addressableId) where T : UnityEngine.Object
        {
            string address = addressableId.GetAddress();
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"AddressableHelper: 유효하지 않은 주소 - {addressableId}");
                return null;
            }
            
            return await AddressableManager.Instance.LoadAssetAsync<T>(address);
        }
        
        /// <summary>
        /// AddressableId를 사용하여 에셋을 로드하고 콜백을 실행합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="addressableId">Addressable ID</param>
        /// <param name="onComplete">로드 완료 콜백</param>
        /// <param name="onFailed">로드 실패 콜백</param>
        public static void LoadAssetAsync<T>(AddressableId addressableId, Action<T> onComplete, Action<string> onFailed = null) where T : UnityEngine.Object
        {
            string address = addressableId.GetAddress();
            if (string.IsNullOrEmpty(address))
            {
                onFailed?.Invoke($"유효하지 않은 주소 - {addressableId}");
                return;
            }
            
            AddressableManager.Instance.LoadAssetAsync<T>(address, onComplete, onFailed);
        }
        
        /// <summary>
        /// AddressableId를 사용하여 프리팹을 인스턴스화합니다.
        /// </summary>
        /// <param name="addressableId">Addressable ID</param>
        /// <param name="parent">부모 Transform</param>
        /// <param name="instantiateInWorldSpace">월드 공간에서 인스턴스화 여부</param>
        /// <returns>인스턴스화된 GameObject</returns>
        public static async UniTask<GameObject> InstantiateAsync(AddressableId addressableId, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            string address = addressableId.GetAddress();
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"AddressableHelper: 유효하지 않은 주소 - {addressableId}");
                return null;
            }
            
            return await AddressableManager.Instance.InstantiateAsync(address, parent, instantiateInWorldSpace);
        }
        
        /// <summary>
        /// 여러 에셋을 병렬로 로드합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="addressableIds">Addressable ID 배열</param>
        /// <returns>로드된 에셋 배열</returns>
        public static async UniTask<T[]> LoadAssetsAsync<T>(params AddressableId[] addressableIds) where T : UnityEngine.Object
        {
            var tasks = new UniTask<T>[addressableIds.Length];
            
            for (int i = 0; i < addressableIds.Length; i++)
            {
                tasks[i] = LoadAssetAsync<T>(addressableIds[i]);
            }
            
            return await UniTask.WhenAll(tasks);
        }
        
        /// <summary>
        /// 특정 라벨의 모든 에셋을 로드합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <param name="label">라벨</param>
        /// <returns>로드된 에셋 리스트</returns>
        public static async UniTask<List<T>> LoadAssetsByLabelAsync<T>(AddressableLabel label) where T : UnityEngine.Object
        {
            var configs = label.GetConfigs();
            var results = new List<T>();
            
            foreach (var config in configs)
            {
                if (config.assetType == typeof(T) || config.assetType.IsSubclassOf(typeof(T)))
                {
                    try
                    {
                        var asset = await AddressableManager.Instance.LoadAssetAsync<T>(config.address);
                        if (asset != null)
                        {
                            results.Add(asset);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"AddressableHelper: 라벨 에셋 로드 실패 - {config.address}\n{e.Message}");
                    }
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 특정 라벨의 모든 에셋을 미리 다운로드합니다.
        /// </summary>
        /// <param name="label">라벨</param>
        /// <param name="onProgress">진행률 콜백 (0.0 ~ 1.0)</param>
        /// <returns>다운로드 성공 여부</returns>
        public static async UniTask<bool> PreloadAssetsByLabelAsync(AddressableLabel label, Action<float> onProgress = null)
        {
            var configs = label.GetConfigs();
            int totalAssets = configs.Count;
            int completedAssets = 0;
            bool allSuccessful = true;
            
            foreach (var config in configs)
            {
                try
                {
                    bool success = await AddressableManager.Instance.DownloadDependenciesAsync(config.address);
                    if (!success)
                    {
                        allSuccessful = false;
                        Debug.LogWarning($"AddressableHelper: 에셋 다운로드 실패 - {config.address}");
                    }
                }
                catch (Exception e)
                {
                    allSuccessful = false;
                    Debug.LogError($"AddressableHelper: 에셋 다운로드 예외 - {config.address}\n{e.Message}");
                }
                
                completedAssets++;
                onProgress?.Invoke((float)completedAssets / totalAssets);
            }
            
            return allSuccessful;
        }
        
        /// <summary>
        /// 특정 라벨의 총 다운로드 크기를 계산합니다.
        /// </summary>
        /// <param name="label">라벨</param>
        /// <returns>총 다운로드 크기 (바이트)</returns>
        public static async UniTask<long> GetDownloadSizeByLabelAsync(AddressableLabel label)
        {
            var configs = label.GetConfigs();
            long totalSize = 0;
            
            foreach (var config in configs)
            {
                try
                {
                    long size = await AddressableManager.Instance.GetDownloadSizeAsync(config.address);
                    totalSize += size;
                }
                catch (Exception e)
                {
                    Debug.LogError($"AddressableHelper: 다운로드 크기 확인 실패 - {config.address}\n{e.Message}");
                }
            }
            
            return totalSize;
        }
        
        /// <summary>
        /// 필수 에셋들을 미리 로드합니다.
        /// </summary>
        /// <param name="onProgress">진행률 콜백</param>
        /// <returns>로드 성공 여부</returns>
        public static async UniTask<bool> PreloadEssentialAssetsAsync(Action<float> onProgress = null)
        {
            var essentialConfigs = AddressableConfigLookup.GetConfigs("Essential");
            int totalAssets = essentialConfigs.Count;
            int completedAssets = 0;
            bool allSuccessful = true;
            
            foreach (var config in essentialConfigs)
            {
                try
                {
                    bool success = await AddressableManager.Instance.DownloadDependenciesAsync(config.address);
                    if (!success)
                    {
                        allSuccessful = false;
                        Debug.LogWarning($"AddressableHelper: 필수 에셋 다운로드 실패 - {config.address}");
                    }
                }
                catch (Exception e)
                {
                    allSuccessful = false;
                    Debug.LogError($"AddressableHelper: 필수 에셋 다운로드 예외 - {config.address}\n{e.Message}");
                }
                
                completedAssets++;
                onProgress?.Invoke((float)completedAssets / totalAssets);
            }
            
            return allSuccessful;
        }
        
        /// <summary>
        /// 에셋 주소의 유효성을 검사합니다.
        /// </summary>
        /// <param name="addressableId">Addressable ID</param>
        /// <returns>유효성 여부</returns>
        public static bool IsValidAddress(AddressableId addressableId)
        {
            var config = addressableId.GetConfig();
            return config != null && !string.IsNullOrEmpty(config.address);
        }
        
        /// <summary>
        /// 에셋이 이미 로드되었는지 확인합니다.
        /// </summary>
        /// <param name="addressableId">Addressable ID</param>
        /// <returns>로드 여부</returns>
        public static bool IsAssetLoaded(AddressableId addressableId)
        {
            string address = addressableId.GetAddress();
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }
            
            // AddressableManager의 내부 상태를 확인하는 로직이 필요하다면
            // AddressableManager에 public 메서드를 추가해야 합니다.
            return false; // 임시 구현
        }
        
        /// <summary>
        /// 특정 타입의 모든 에셋을 로드합니다.
        /// </summary>
        /// <typeparam name="T">로드할 에셋 타입</typeparam>
        /// <returns>로드된 에셋 리스트</returns>
        public static async UniTask<List<T>> LoadAllAssetsByTypeAsync<T>() where T : UnityEngine.Object
        {
            var configs = AddressableConfigLookup.GetConfigsByType<T>();
            var results = new List<T>();
            
            foreach (var config in configs)
            {
                try
                {
                    var asset = await AddressableManager.Instance.LoadAssetAsync<T>(config.address);
                    if (asset != null)
                    {
                        results.Add(asset);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"AddressableHelper: 타입별 에셋 로드 실패 - {config.address}\n{e.Message}");
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 에셋 정보를 문자열로 반환합니다. (디버깅용)
        /// </summary>
        /// <param name="addressableId">Addressable ID</param>
        /// <returns>에셋 정보 문자열</returns>
        public static string GetAssetInfo(AddressableId addressableId)
        {
            var config = addressableId.GetConfig();
            if (config == null)
            {
                return $"AddressableId: {addressableId} - 유효하지 않은 설정";
            }
            
            return $"AddressableId: {addressableId}\n" +
                   $"주소: {config.address}\n" +
                   $"경로: {config.assetPath}\n" +
                   $"타입: {config.assetType.Name}\n" +
                   $"라벨: [{string.Join(", ", config.labels)}]";
        }
        
        /// <summary>
        /// 모든 Addressable 설정 정보를 출력합니다. (디버깅용)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void PrintAllConfigs()
        {
            Debug.Log("=== AddressableHelper 전체 설정 정보 ===");
            
            var configs = AddressableConfigLookup.Configs;
            foreach (var config in configs)
            {
                Debug.Log($"ID: {config.id}, 주소: {config.address}, 타입: {config.assetType.Name}, 라벨: [{string.Join(", ", config.labels)}]");
            }
            
            Debug.Log("=========================================");
        }
        
        /// <summary>
        /// 라벨별 에셋 개수를 출력합니다. (디버깅용)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void PrintLabelStatistics()
        {
            Debug.Log("=== AddressableHelper 라벨별 통계 ===");
            
            var labels = AddressableConfigLookup.Labels;
            foreach (var label in labels)
            {
                var configs = AddressableConfigLookup.GetConfigs(label);
                Debug.Log($"라벨 '{label}': {configs.Count}개 에셋");
            }
            
            Debug.Log("====================================");
        }
        
        /// <summary>
        /// 메모리 사용량을 최적화합니다.
        /// </summary>
        public static void OptimizeMemory()
        {
            // 사용하지 않는 에셋 언로드
            Resources.UnloadUnusedAssets();
            
            // 가비지 컬렉션 실행
            System.GC.Collect();
            
            Debug.Log("AddressableHelper: 메모리 최적화 완료");
        }
        
        /// <summary>
        /// 에셋 캐시를 정리합니다.
        /// </summary>
        public static void ClearCache()
        {
            // Addressables 캐시 정리
            Addressables.CleanBundleCache();
            
            Debug.Log("AddressableHelper: 캐시 정리 완료");
        }
    }
}