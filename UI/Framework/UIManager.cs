using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;

/// <summary>
/// UI 관리 시스템
/// 화면에 표시되는 모든 UI 요소를 관리합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region 싱글톤
    private static UIManager _instance;
    
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UIManager");
                _instance = go.AddComponent<UIManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion
    
    [System.Serializable]
    public class UILayer
    {
        public string name;
        public Transform container;
    }
    
    [Header("UI Containers")]
    [SerializeField] private UILayer[] _uiLayers;
    
    [Header("UI Prefabs")]
    [SerializeField] private GameObject _overlayLoaderPrefab;
    [SerializeField] private GameObject _messagePrefab;
    
    [Header("UI Settings")]
    [SerializeField] private bool _fadeInOnCreate = true;
    [SerializeField] private float _defaultFadeDuration = 0.3f;
    [SerializeField] private int _messageStackLimit = 5;
    
    // UI 캐싱 및 관리
    private Dictionary<string, UIPanel> _activePanels = new Dictionary<string, UIPanel>();
    private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
    private Dictionary<string, Transform> _layerMap = new Dictionary<string, Transform>();
    
    private Stack<UIPanel> _panelHistory = new Stack<UIPanel>();
    private Queue<MessageData> _messageQueue = new Queue<MessageData>();
    
    private Canvas _mainCanvas;
    private GameObject _overlayLoader;
    private bool _isShowingMessage = false;
    
    private bool _isInitialized = false;
    
    // 메시지 데이터 구조체
    private struct MessageData
    {
        public string message;
        public string title;
        public MessageType type;
        public float duration;
        public System.Action callback;
    }
    
    /// <summary>
    /// UI 관리자를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;
        
        // 메인 캔버스 생성
        GameObject canvasObj = new GameObject("UI Canvas");
        canvasObj.transform.SetParent(transform);
        
        _mainCanvas = canvasObj.AddComponent<Canvas>();
        _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // 캔버스 스케일러 설정
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // 레이어 설정
        if (_uiLayers == null || _uiLayers.Length == 0)
        {
            // 기본 레이어 생성
            _uiLayers = new UILayer[]
            {
                new UILayer { name = "Background", container = CreateLayerContainer("Background", 0) },
                new UILayer { name = "Main", container = CreateLayerContainer("Main", 100) },
                new UILayer { name = "Popup", container = CreateLayerContainer("Popup", 200) },
                new UILayer { name = "Modal", container = CreateLayerContainer("Modal", 300) },
                new UILayer { name = "Overlay", container = CreateLayerContainer("Overlay", 400) }
            };
        }
        else
        {
            // 지정된 레이어 컨테이너 생성
            for (int i = 0; i < _uiLayers.Length; i++)
            {
                if (_uiLayers[i].container == null)
                {
                    _uiLayers[i].container = CreateLayerContainer(_uiLayers[i].name, i * 100);
                }
            }
        }
        
        // 레이어 맵 초기화
        foreach (var layer in _uiLayers)
        {
            _layerMap[layer.name] = layer.container;
        }
        
        // 로더 프리팹 설정
        if (_overlayLoaderPrefab == null)
        {
            Debug.LogWarning("오버레이 로더 프리팹이 설정되지 않았습니다.");
        }
        
        _isInitialized = true;
        Debug.Log("UIManager가 초기화되었습니다.");
    }
    
    /// <summary>
    /// UI 레이어 컨테이너를 생성합니다.
    /// </summary>
    private Transform CreateLayerContainer(string name, int sortingOrder)
    {
        GameObject containerObj = new GameObject(name);
        containerObj.transform.SetParent(_mainCanvas.transform, false);
        
        RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        Canvas canvas = containerObj.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        
        containerObj.AddComponent<GraphicRaycaster>();
        
        return containerObj.transform;
    }
    
    /// <summary>
    /// UI 패널을 표시합니다.
    /// </summary>
    public async Task<T> ShowPanel<T>(string panelPrefabPath, string layerName = "Main", bool addToHistory = true) where T : UIPanel
    {
        if (!_isInitialized)
        {
            Debug.LogError("UIManager가 초기화되지 않았습니다.");
            return null;
        }
        
        // 이미 활성화된 패널인지 확인
        if (_activePanels.TryGetValue(panelPrefabPath, out UIPanel existingPanel) && existingPanel != null)
        {
            return existingPanel as T;
        }
        
        // 레이어 확인
        if (!_layerMap.TryGetValue(layerName, out Transform layerContainer))
        {
            Debug.LogError($"UI 레이어를 찾을 수 없습니다: {layerName}");
            return null;
        }
        
        // 프리팹 로드
        GameObject panelPrefab;
        if (!_prefabCache.TryGetValue(panelPrefabPath, out panelPrefab))
        {
            panelPrefab = Resources.Load<GameObject>(panelPrefabPath);
            
            if (panelPrefab == null)
            {
                Debug.LogError($"UI 프리팹을 찾을 수 없습니다: {panelPrefabPath}");
                return null;
            }
            
            _prefabCache[panelPrefabPath] = panelPrefab;
        }
        
        // 패널 생성
        GameObject panelObj = Instantiate(panelPrefab, layerContainer);
        T panel = panelObj.GetComponent<T>();
        
        if (panel == null)
        {
            Debug.LogError($"생성된 UI에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            Destroy(panelObj);
            return null;
        }
        
        // 패널 초기화 및 등록
        panel.PanelId = panelPrefabPath;
        _activePanels[panelPrefabPath] = panel;
        
        if (addToHistory)
        {
            _panelHistory.Push(panel);
        }
        
        // 패널 표시
        if (_fadeInOnCreate && panel.CanFade)
        {
            panel.gameObject.SetActive(true);
            await panel.FadeIn(_defaultFadeDuration);
        }
        else
        {
            panel.gameObject.SetActive(true);
            panel.OnPanelShown();
        }
        
        return panel;
    }
    
    /// <summary>
    /// UI 패널을 닫습니다.
    /// </summary>
    public async Task ClosePanel(UIPanel panel, bool fadeOut = true)
    {
        if (panel == null) return;
        
        if (fadeOut && panel.CanFade)
        {
            await panel.FadeOut(_defaultFadeDuration);
        }
        
        panel.OnPanelHidden();
        
        // 활성 패널 목록에서 제거
        if (_activePanels.ContainsKey(panel.PanelId))
        {
            _activePanels.Remove(panel.PanelId);
        }
        
        // 히스토리에서 제거
        if (_panelHistory.Count > 0 && _panelHistory.Peek() == panel)
        {
            _panelHistory.Pop();
        }
        
        Destroy(panel.gameObject);
    }
    
    /// <summary>
    /// UI 패널을 패널 ID로 닫습니다.
    /// </summary>
    public async Task ClosePanel(string panelId, bool fadeOut = true)
    {
        if (_activePanels.TryGetValue(panelId, out UIPanel panel))
        {
            await ClosePanel(panel, fadeOut);
        }
    }
    
    /// <summary>
    /// 현재 표시된 최상위 패널을 닫습니다.
    /// </summary>
    public async Task CloseTopPanel()
    {
        if (_panelHistory.Count > 0)
        {
            UIPanel topPanel = _panelHistory.Peek();
            await ClosePanel(topPanel);
        }
    }
    
    /// <summary>
    /// 모든 UI 패널을 닫습니다.
    /// </summary>
    public async Task CloseAllPanels(bool fadeOut = true)
    {
        List<UIPanel> panelsToClose = new List<UIPanel>(_activePanels.Values);
        
        foreach (var panel in panelsToClose)
        {
            await ClosePanel(panel, fadeOut);
        }
        
        _panelHistory.Clear();
    }
    
    /// <summary>
    /// 로딩 오버레이를 표시합니다.
    /// </summary>
    public void ShowLoader()
    {
        if (_overlayLoader == null && _overlayLoaderPrefab != null)
        {
            Transform overlayLayer = _layerMap["Overlay"];
            _overlayLoader = Instantiate(_overlayLoaderPrefab, overlayLayer);
        }
        
        if (_overlayLoader != null)
        {
            _overlayLoader.SetActive(true);
        }
    }
    
    /// <summary>
    /// 로딩 오버레이를 숨깁니다.
    /// </summary>
    public void HideLoader()
    {
        if (_overlayLoader != null)
        {
            _overlayLoader.SetActive(false);
        }
    }
    
    /// <summary>
    /// 메시지를 표시합니다.
    /// </summary>
    public void ShowMessage(string message, MessageType type = MessageType.Info, float duration = 2.0f, string title = "", System.Action callback = null)
    {
        MessageData data = new MessageData
        {
            message = message,
            title = title,
            type = type,
            duration = duration,
            callback = callback
        };
        
        _messageQueue.Enqueue(data);
        
        if (!_isShowingMessage)
        {
            ProcessMessageQueue();
        }
    }
    
    /// <summary>
    /// 메시지 큐를 처리합니다.
    /// </summary>
    private async void ProcessMessageQueue()
    {
        if (_messageQueue.Count == 0)
        {
            _isShowingMessage = false;
            return;
        }
        
        _isShowingMessage = true;
        
        // 메시지 큐가 너무 많이 쌓이면 오래된 메시지 제거
        while (_messageQueue.Count > _messageStackLimit)
        {
            _messageQueue.Dequeue();
        }
        
        MessageData data = _messageQueue.Dequeue();
        
        if (_messagePrefab == null)
        {
            Debug.LogError("메시지 프리팹이 설정되지 않았습니다.");
            _isShowingMessage = false;
            return;
        }
        
        Transform overlayLayer = _layerMap["Overlay"];
        GameObject messageObj = Instantiate(_messagePrefab, overlayLayer);
        UIMessage messageComponent = messageObj.GetComponent<UIMessage>();
        
        if (messageComponent != null)
        {
            messageComponent.SetMessage(data.message, data.type, data.title);
            messageComponent.Show();
            
            // 지정된 시간 후 메시지 숨김
            await Task.Delay(Mathf.RoundToInt(data.duration * 1000));
            
            messageComponent.Hide();
            
            // 콜백 실행
            data.callback?.Invoke();
            
            // 숨김 애니메이션 완료 대기
            await Task.Delay(300);
            
            Destroy(messageObj);
            
            // 다음 메시지 처리
            ProcessMessageQueue();
        }
        else
        {
            Debug.LogError("UIMessage 컴포넌트를 찾을 수 없습니다.");
            Destroy(messageObj);
            _isShowingMessage = false;
        }
    }
    
    /// <summary>
    /// 활성화된 패널을 가져옵니다.
    /// </summary>
    public T GetPanel<T>(string panelId) where T : UIPanel
    {
        if (_activePanels.TryGetValue(panelId, out UIPanel panel))
        {
            return panel as T;
        }
        
        return null;
    }
    
    /// <summary>
    /// 지정된 레이어의 모든 패널을 닫습니다.
    /// </summary>
    public async Task CloseAllPanelsInLayer(string layerName, bool fadeOut = true)
    {
        if (!_layerMap.TryGetValue(layerName, out Transform layerContainer))
        {
            Debug.LogError($"UI 레이어를 찾을 수 없습니다: {layerName}");
            return;
        }
        
        List<UIPanel> panelsToClose = new List<UIPanel>();
        
        foreach (var kvp in _activePanels)
        {
            if (kvp.Value.transform.parent == layerContainer)
            {
                panelsToClose.Add(kvp.Value);
            }
        }
        
        foreach (var panel in panelsToClose)
        {
            await ClosePanel(panel, fadeOut);
        }
    }
    
    private void OnDestroy()
    {
        // 필요한 정리 작업 수행
        _activePanels.Clear();
        _prefabCache.Clear();
        _panelHistory.Clear();
        _messageQueue.Clear();
    }
}

/// <summary>
/// 메시지 유형
/// </summary>
public enum MessageType
{
    Info,
    Success,
    Warning,
    Error
}