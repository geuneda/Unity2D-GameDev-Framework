using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 새로운 Unity Input System을 활용한 입력 관리 클래스
/// 입력 액션과 콜백을 관리합니다.
/// </summary>
public class InputManager : MonoBehaviour
{
    #region 싱글톤
    private static InputManager _instance;
    
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("InputManager");
                _instance = go.AddComponent<InputManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField] private InputActionAsset _inputActions;
    private Dictionary<string, InputActionReference> _actionReferences = new Dictionary<string, InputActionReference>();
    
    private bool _isInitialized = false;
    private bool _inputEnabled = true;

    public bool InputEnabled
    {
        get => _inputEnabled;
        set
        {
            _inputEnabled = value;
            if (_inputEnabled)
            {
                EnableAllInput();
            }
            else
            {
                DisableAllInput();
            }
        }
    }

    /// <summary>
    /// 입력 시스템을 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;
        
        if (_inputActions == null)
        {
            Debug.LogError("InputActionAsset이 설정되지 않았습니다. Inspector에서 설정해주세요.");
            return;
        }
        
        // 모든 액션을 순회하며 참조 캐싱
        foreach (var actionMap in _inputActions.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                string actionId = $"{actionMap.name}/{action.name}";
                _actionReferences[actionId] = InputActionReference.Create(action);
            }
        }
        
        // 액션들 활성화
        _inputActions.Enable();
        _isInitialized = true;
        
        Debug.Log("InputManager가 초기화되었습니다.");
    }

    /// <summary>
    /// 특정 액션에 콜백을 등록합니다.
    /// </summary>
    public void RegisterAction(string actionMapName, string actionName, System.Action<InputAction.CallbackContext> callback)
    {
        string actionId = $"{actionMapName}/{actionName}";
        
        if (!_actionReferences.TryGetValue(actionId, out var actionRef))
        {
            Debug.LogError($"'{actionId}' 액션을 찾을 수 없습니다.");
            return;
        }
        
        InputAction action = actionRef.action;
        action.performed += callback;
        action.canceled += callback;
    }
    
    /// <summary>
    /// 특정 액션의 콜백 등록을 해제합니다.
    /// </summary>
    public void UnregisterAction(string actionMapName, string actionName, System.Action<InputAction.CallbackContext> callback)
    {
        string actionId = $"{actionMapName}/{actionName}";
        
        if (!_actionReferences.TryGetValue(actionId, out var actionRef))
        {
            Debug.LogError($"'{actionId}' 액션을 찾을 수 없습니다.");
            return;
        }
        
        InputAction action = actionRef.action;
        action.performed -= callback;
        action.canceled -= callback;
    }
    
    /// <summary>
    /// 특정 액션맵을 활성화합니다.
    /// </summary>
    public void EnableActionMap(string actionMapName)
    {
        var actionMap = _inputActions.FindActionMap(actionMapName);
        if (actionMap != null)
        {
            actionMap.Enable();
        }
        else
        {
            Debug.LogError($"'{actionMapName}' 액션맵을 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 특정 액션맵을 비활성화합니다.
    /// </summary>
    public void DisableActionMap(string actionMapName)
    {
        var actionMap = _inputActions.FindActionMap(actionMapName);
        if (actionMap != null)
        {
            actionMap.Disable();
        }
        else
        {
            Debug.LogError($"'{actionMapName}' 액션맵을 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 모든 입력을 활성화합니다.
    /// </summary>
    public void EnableAllInput()
    {
        _inputActions.Enable();
    }
    
    /// <summary>
    /// 모든 입력을 비활성화합니다.
    /// </summary>
    public void DisableAllInput()
    {
        _inputActions.Disable();
    }
    
    /// <summary>
    /// 특정 액션의 바인딩을 런타임에 변경합니다.
    /// </summary>
    public void RebindAction(string actionMapName, string actionName, int bindingIndex)
    {
        string actionId = $"{actionMapName}/{actionName}";
        
        if (!_actionReferences.TryGetValue(actionId, out var actionRef))
        {
            Debug.LogError($"'{actionId}' 액션을 찾을 수 없습니다.");
            return;
        }
        
        InputAction action = actionRef.action;
        
        // 현재 바인딩 비활성화
        action.Disable();
        
        // 리바인딩 작업 시작
        var rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => {
                action.Enable();
                operation.Dispose();
            })
            .Start();
    }
    
    private void OnDestroy()
    {
        // 입력 액션 자원 해제
        if (_isInitialized)
        {
            _inputActions.Disable();
        }
    }
}