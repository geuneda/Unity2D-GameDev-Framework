using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력을 처리하는 컨트롤러 클래스
/// 캐릭터 이동, 점프, 공격 등의 입력을 감지하고 처리합니다.
/// </summary>
public class PlayerInputController : MonoBehaviour
{
    // 입력 이벤트에 대한 델리게이트 정의
    public delegate void MoveInputEvent(Vector2 moveInput);
    public delegate void JumpInputEvent();
    public delegate void AttackInputEvent();
    public delegate void InteractInputEvent();
    public delegate void PauseInputEvent();
    
    // 이벤트 선언
    public event MoveInputEvent OnMoveInput;
    public event JumpInputEvent OnJumpStarted;
    public event JumpInputEvent OnJumpCanceled;
    public event AttackInputEvent OnAttackPerformed;
    public event InteractInputEvent OnInteractPerformed;
    public event PauseInputEvent OnPausePerformed;
    
    [SerializeField] private InputActionAsset _inputActions;
    
    // 입력 액션 참조 캐싱을 위한 변수
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _interactAction;
    private InputAction _pauseAction;
    
    private bool _isInputEnabled = true;
    
    /// <summary>
    /// 입력 활성화 여부를 설정합니다.
    /// </summary>
    public bool IsInputEnabled
    {
        get => _isInputEnabled;
        set
        {
            _isInputEnabled = value;
            if (_isInputEnabled)
            {
                EnableAllInput();
            }
            else
            {
                DisableAllInput();
            }
        }
    }
    
    private void Awake()
    {
        if (_inputActions == null)
        {
            Debug.LogError("PlayerInputController: InputActionAsset이 설정되지 않았습니다.");
            return;
        }
        
        // 필요한 액션들 참조 캐싱
        _moveAction = _inputActions.FindAction("Player/Move");
        _jumpAction = _inputActions.FindAction("Player/Jump");
        _attackAction = _inputActions.FindAction("Player/Attack");
        _interactAction = _inputActions.FindAction("Player/Interact");
        _pauseAction = _inputActions.FindAction("UI/Pause");
        
        // 액션들에 콜백 등록
        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;
        
        _jumpAction.performed += OnJump;
        _jumpAction.canceled += OnJumpCanceled;
        
        _attackAction.performed += OnAttack;
        _interactAction.performed += OnInteract;
        _pauseAction.performed += OnPause;
    }
    
    private void OnEnable()
    {
        // 모든 입력 활성화
        EnableAllInput();
    }
    
    private void OnDisable()
    {
        // 모든 입력 비활성화
        DisableAllInput();
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
    /// 게임 플레이 관련 입력만 활성화합니다.
    /// </summary>
    public void EnableGameplayInput()
    {
        var playerMap = _inputActions.FindActionMap("Player");
        if (playerMap != null)
        {
            playerMap.Enable();
        }
    }
    
    /// <summary>
    /// UI 관련 입력만 활성화합니다.
    /// </summary>
    public void EnableUIInput()
    {
        var uiMap = _inputActions.FindActionMap("UI");
        if (uiMap != null)
        {
            uiMap.Enable();
        }
    }
    
    // 입력 콜백 메서드들
    private void OnMove(InputAction.CallbackContext context)
    {
        if (!_isInputEnabled) return;
        
        Vector2 input = context.ReadValue<Vector2>();
        OnMoveInput?.Invoke(input);
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        if (!_isInputEnabled || !context.performed) return;
        
        OnJumpStarted?.Invoke();
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        if (!_isInputEnabled || !context.canceled) return;
        
        OnJumpCanceled?.Invoke();
    }
    
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!_isInputEnabled || !context.performed) return;
        
        OnAttackPerformed?.Invoke();
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!_isInputEnabled || !context.performed) return;
        
        OnInteractPerformed?.Invoke();
    }
    
    private void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        OnPausePerformed?.Invoke();
    }
    
    private void OnDestroy()
    {
        // 콜백 제거
        if (_moveAction != null)
        {
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
        }
        
        if (_jumpAction != null)
        {
            _jumpAction.performed -= OnJump;
            _jumpAction.canceled -= OnJumpCanceled;
        }
        
        if (_attackAction != null) _attackAction.performed -= OnAttack;
        if (_interactAction != null) _interactAction.performed -= OnInteract;
        if (_pauseAction != null) _pauseAction.performed -= OnPause;
    }
}