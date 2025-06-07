using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임의 전체적인 흐름을 관리하는 싱글톤 매니저 클래스
/// 다른 매니저들의 생성 및 초기화를 담당합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 싱글톤
    private static GameManager _instance;
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameManager");
                _instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField] private bool _isGamePaused = false;
    
    // 다른 매니저들에 대한 참조
    private AudioManager _audioManager;
    private InputManager _inputManager;
    private UIManager _uiManager;
    private PoolManager _poolManager;
    private SceneController _sceneController;
    
    // 이벤트 시스템
    private Dictionary<string, System.Action> _eventDictionary;
    
    /// <summary>
    /// 게임이 일시정지 상태인지 여부
    /// </summary>
    public bool IsGamePaused
    {
        get => _isGamePaused;
        set
        {
            _isGamePaused = value;
            Time.timeScale = _isGamePaused ? 0f : 1f;
            
            // 게임 일시정지 이벤트 발생
            TriggerEvent(_isGamePaused ? "OnGamePaused" : "OnGameResumed");
        }
    }

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 이벤트 시스템 초기화
        _eventDictionary = new Dictionary<string, System.Action>();
        
        InitializeManagers();
    }

    /// <summary>
    /// 모든 매니저들을 초기화합니다.
    /// </summary>
    private void InitializeManagers()
    {
        // 각 매니저 인스턴스 생성 및 초기화
        _audioManager = AudioManager.Instance;
        _inputManager = InputManager.Instance;
        _uiManager = UIManager.Instance;
        _poolManager = PoolManager.Instance;
        _sceneController = SceneController.Instance;
        
        // 각 매니저 초기화
        _audioManager.Initialize();
        _inputManager.Initialize();
        _uiManager.Initialize();
        _poolManager.Initialize();
        _sceneController.Initialize();
    }
    
    /// <summary>
    /// 이벤트를 구독합니다.
    /// </summary>
    public void SubscribeEvent(string eventName, System.Action listener)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            _eventDictionary[eventName] += listener;
        }
        else
        {
            _eventDictionary.Add(eventName, listener);
        }
    }
    
    /// <summary>
    /// 이벤트 구독을 취소합니다.
    /// </summary>
    public void UnsubscribeEvent(string eventName, System.Action listener)
    {
        if (_eventDictionary.ContainsKey(eventName))
        {
            _eventDictionary[eventName] -= listener;
            
            // 이벤트에 더 이상 리스너가 없으면 제거
            if (_eventDictionary[eventName] == null)
            {
                _eventDictionary.Remove(eventName);
            }
        }
    }
    
    /// <summary>
    /// 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerEvent(string eventName)
    {
        if (_eventDictionary.TryGetValue(eventName, out System.Action callback))
        {
            callback?.Invoke();
        }
    }
    
    /// <summary>
    /// 게임을 일시정지합니다.
    /// </summary>
    public void PauseGame()
    {
        IsGamePaused = true;
    }
    
    /// <summary>
    /// 게임을 재개합니다.
    /// </summary>
    public void ResumeGame()
    {
        IsGamePaused = false;
    }
    
    /// <summary>
    /// 게임을 종료합니다.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}