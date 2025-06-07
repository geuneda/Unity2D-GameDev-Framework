using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 씬 전환을 관리하는 클래스
/// 씬 로딩 및 전환 효과를 처리합니다.
/// </summary>
public class SceneController : MonoBehaviour
{
    #region 싱글톤
    private static SceneController _instance;
    
    public static SceneController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SceneController");
                _instance = go.AddComponent<SceneController>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion
    
    [SerializeField] private GameObject _loadingScreenPrefab;
    [SerializeField] private float _minLoadingTime = 0.5f;
    [SerializeField] private bool _useAsyncLoading = true;
    [SerializeField] private bool _showDebugInfo = false;
    
    // 씬 전환 이벤트
    public event Action<string> OnBeforeSceneLoad;
    public event Action<string> OnAfterSceneLoad;
    
    // 씬 간에 유지되는 데이터
    private Dictionary<string, object> _persistentData = new Dictionary<string, object>();
    
    // 로딩 진행 상황 처리
    private bool _isLoading = false;
    private GameObject _loadingScreen;
    private ILoadingScreen _loadingScreenController;
    
    // 현재 씬 정보
    private string _currentScene;
    public string CurrentScene => _currentScene;
    
    // 씬 히스토리
    private Stack<string> _sceneHistory = new Stack<string>();
    
    private bool _isInitialized = false;
    
    /// <summary>
    /// 씬 컨트롤러를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;
        
        _currentScene = SceneManager.GetActiveScene().name;
        _sceneHistory.Push(_currentScene);
        
        if (_showDebugInfo)
        {
            Debug.Log($"SceneController가 초기화되었습니다. 현재 씬: {_currentScene}");
        }
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// 씬을 로드합니다.
    /// </summary>
    public async Task LoadScene(string sceneName, bool addToHistory = true)
    {
        if (_isLoading)
        {
            Debug.LogWarning("이미 씬을 로드하는 중입니다.");
            return;
        }
        
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("씬 이름이 비어있습니다.");
            return;
        }
        
        _isLoading = true;
        OnBeforeSceneLoad?.Invoke(sceneName);
        
        // 로딩 화면 표시
        ShowLoadingScreen();
        
        // 비동기 로딩을 위한 지연
        await Task.Delay(100);
        
        // 로딩 시작 시간
        float startTime = Time.time;
        
        // 씬 로드 (비동기 또는 동기)
        if (_useAsyncLoading)
        {
            await LoadSceneAsync(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
        
        // 최소 로딩 시간 보장
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < _minLoadingTime)
        {
            await Task.Delay(Mathf.RoundToInt((_minLoadingTime - elapsedTime) * 1000));
        }
        
        // 로딩 화면 숨김
        HideLoadingScreen();
        
        // 현재 씬 업데이트
        _currentScene = sceneName;
        
        if (addToHistory)
        {
            _sceneHistory.Push(sceneName);
        }
        
        _isLoading = false;
        OnAfterSceneLoad?.Invoke(sceneName);
        
        if (_showDebugInfo)
        {
            Debug.Log($"씬 로드 완료: {sceneName}");
        }
    }
    
    /// <summary>
    /// 비동기 씬 로딩을 처리합니다.
    /// </summary>
    private async Task LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        while (asyncLoad.progress < 0.9f)
        {
            // 로딩 진행 상황 업데이트
            if (_loadingScreenController != null)
            {
                _loadingScreenController.UpdateProgress(asyncLoad.progress);
            }
            
            await Task.Yield();
        }
        
        // 로딩 완료 표시
        if (_loadingScreenController != null)
        {
            _loadingScreenController.UpdateProgress(1f);
        }
        
        await Task.Delay(200);
        asyncLoad.allowSceneActivation = true;
        
        // 씬이 활성화될 때까지 대기
        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }
    
    /// <summary>
    /// 이전 씬으로 돌아갑니다.
    /// </summary>
    public async Task LoadPreviousScene()
    {
        if (_sceneHistory.Count <= 1)
        {
            Debug.LogWarning("이전 씬 정보가 없습니다.");
            return;
        }
        
        // 현재 씬은 제거
        _sceneHistory.Pop();
        
        // 이전 씬 로드
        string previousScene = _sceneHistory.Peek();
        await LoadScene(previousScene, false);
    }
    
    /// <summary>
    /// 로딩 화면을 표시합니다.
    /// </summary>
    private void ShowLoadingScreen()
    {
        if (_loadingScreenPrefab != null)
        {
            _loadingScreen = Instantiate(_loadingScreenPrefab);
            DontDestroyOnLoad(_loadingScreen);
            
            _loadingScreenController = _loadingScreen.GetComponent<ILoadingScreen>();
            if (_loadingScreenController != null)
            {
                _loadingScreenController.Show();
            }
        }
    }
    
    /// <summary>
    /// 로딩 화면을 숨깁니다.
    /// </summary>
    private void HideLoadingScreen()
    {
        if (_loadingScreen != null)
        {
            if (_loadingScreenController != null)
            {
                _loadingScreenController.Hide();
            }
            
            Destroy(_loadingScreen);
            _loadingScreen = null;
            _loadingScreenController = null;
        }
    }
    
    /// <summary>
    /// 씬 간에 유지할 데이터를 저장합니다.
    /// </summary>
    public void SetPersistentData(string key, object data)
    {
        if (_persistentData.ContainsKey(key))
        {
            _persistentData[key] = data;
        }
        else
        {
            _persistentData.Add(key, data);
        }
    }
    
    /// <summary>
    /// 저장된 유지 데이터를 가져옵니다.
    /// </summary>
    public T GetPersistentData<T>(string key, T defaultValue = default)
    {
        if (_persistentData.TryGetValue(key, out object value) && value is T typedValue)
        {
            return typedValue;
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// 유지 데이터를 삭제합니다.
    /// </summary>
    public void ClearPersistentData(string key = null)
    {
        if (key == null)
        {
            _persistentData.Clear();
        }
        else if (_persistentData.ContainsKey(key))
        {
            _persistentData.Remove(key);
        }
    }
}

/// <summary>
/// 로딩 화면 구현을 위한 인터페이스
/// </summary>
public interface ILoadingScreen
{
    void Show();
    void Hide();
    void UpdateProgress(float progress);
}