using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Unity2DFramework.Core.Input
{
    /// <summary>
    /// 새로운 Unity Input System을 기반으로 한 입력 관리자
    /// 멀티플랫폼 입력을 통합 관리하고 런타임 키 리매핑 지원
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        
        [Header("입력 액션 에셋")]
        [SerializeField] private InputActionAsset inputActionAsset;
        
        // 입력 액션 맵들
        private InputActionMap playerActionMap;
        private InputActionMap uiActionMap;
        
        // 입력 액션들 (캐싱)
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;
        private InputAction interactAction;
        private InputAction pauseAction;
        private InputAction nextAction;
        private InputAction previousAction;
        private InputAction sprintAction;
        
        // 입력 이벤트들
        public System.Action<Vector2> OnMove;
        public System.Action OnJump;
        public System.Action OnJumpCanceled;
        public System.Action OnAttack;
        public System.Action OnInteract;
        public System.Action OnPause;
        public System.Action OnNext;
        public System.Action OnPrevious;
        public System.Action<bool> OnSprint;
        
        // 입력 상태
        private Vector2 currentMoveInput;
        private bool isInputEnabled = true;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 입력 매니저 초기화
        /// </summary>
        public void Initialize()
        {
            if (inputActionAsset == null)
            {
                // 리소스에서 입력 액션 에셋을 로드
                inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
                
                if (inputActionAsset == null)
                {
                    Debug.LogError("[InputManager] InputActionAsset을 찾을 수 없습니다!");
                    return;
                }
            }
            
            // 액션 맵 가져오기
            playerActionMap = inputActionAsset.FindActionMap("Player");
            uiActionMap = inputActionAsset.FindActionMap("UI");
            
            if (playerActionMap == null)
            {
                Debug.LogError("[InputManager] Player 액션 맵을 찾을 수 없습니다!");
                return;
            }
            
            // 액션들 캐싱
            CacheInputActions();
            
            // 콜백 등록
            RegisterInputCallbacks();
            
            // 플레이어 액션 맵 활성화
            EnablePlayerInput();
            
            Debug.Log("[InputManager] 입력 매니저 초기화 완료");
        }
        
        /// <summary>
        /// 입력 액션들을 캐싱
        /// </summary>
        private void CacheInputActions()
        {
            if (playerActionMap != null)
            {
                moveAction = playerActionMap.FindAction("Move");
                jumpAction = playerActionMap.FindAction("Jump");
                attackAction = playerActionMap.FindAction("Attack");
                interactAction = playerActionMap.FindAction("Interact");
                pauseAction = playerActionMap.FindAction("Pause", false);
                nextAction = playerActionMap.FindAction("Next");
                previousAction = playerActionMap.FindAction("Previous");
                sprintAction = playerActionMap.FindAction("Sprint");
                
                if (pauseAction == null && uiActionMap != null)
                {
                    pauseAction = uiActionMap.FindAction("Pause", false);
                }
            }
        }
        
        /// <summary>
        /// 입력 콜백들을 등록
        /// </summary>
        private void RegisterInputCallbacks()
        {
            // Move 액션
            if (moveAction != null)
            {
                moveAction.performed += OnMovePerformed;
                moveAction.canceled += OnMoveCanceled;
            }
            
            // Jump 액션
            if (jumpAction != null)
            {
                jumpAction.performed += OnJumpPerformed;
                jumpAction.canceled += OnJumpCanceledPerformed;
            }
            
            // Attack 액션
            if (attackAction != null)
            {
                attackAction.performed += OnAttackPerformed;
            }
            
            // Interact 액션
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }
            
            // Pause 액션
            if (pauseAction != null)
            {
                pauseAction.performed += OnPausePerformed;
            }
            
            // Next 액션
            if (nextAction != null)
            {
                nextAction.performed += OnNextPerformed;
            }
            
            // Previous 액션
            if (previousAction != null)
            {
                previousAction.performed += OnPreviousPerformed;
            }
            
            // Sprint 액션
            if (sprintAction != null)
            {
                sprintAction.performed += OnSprintPerformed;
                sprintAction.canceled += OnSprintCanceled;
            }
        }
        
        /// <summary>
        /// 입력 콜백들을 해제
        /// </summary>
        private void UnregisterInputCallbacks()
        {
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }
            
            if (jumpAction != null)
            {
                jumpAction.performed -= OnJumpPerformed;
                jumpAction.canceled -= OnJumpCanceledPerformed;
            }
            
            if (attackAction != null)
            {
                attackAction.performed -= OnAttackPerformed;
            }
            
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
            }
            
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePerformed;
            }
            
            if (nextAction != null)
            {
                nextAction.performed -= OnNextPerformed;
            }
            
            if (previousAction != null)
            {
                previousAction.performed -= OnPreviousPerformed;
            }
            
            if (sprintAction != null)
            {
                sprintAction.performed -= OnSprintPerformed;
                sprintAction.canceled -= OnSprintCanceled;
            }
        }
        
        // 입력 콜백 메서드들
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            
            currentMoveInput = context.ReadValue<Vector2>();
            OnMove?.Invoke(currentMoveInput);
        }
        
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            currentMoveInput = Vector2.zero;
            OnMove?.Invoke(currentMoveInput);
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnJump?.Invoke();
        }
        
        private void OnJumpCanceledPerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnJumpCanceled?.Invoke();
        }
        
        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnAttack?.Invoke();
        }
        
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnInteract?.Invoke();
        }
        
        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            OnPause?.Invoke();
        }
        
        private void OnNextPerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnNext?.Invoke();
        }
        
        private void OnPreviousPerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnPrevious?.Invoke();
        }
        
        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnSprint?.Invoke(true);
        }
        
        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            if (!isInputEnabled) return;
            OnSprint?.Invoke(false);
        }
        
        /// <summary>
        /// 플레이어 입력 활성화
        /// </summary>
        public void EnablePlayerInput()
        {
            playerActionMap?.Enable();
            uiActionMap?.Disable();
            isInputEnabled = true;
        }
        
        /// <summary>
        /// UI 입력 활성화
        /// </summary>
        public void EnableUIInput()
        {
            playerActionMap?.Disable();
            uiActionMap?.Enable();
            isInputEnabled = false;
        }
        
        /// <summary>
        /// 모든 입력 비활성화
        /// </summary>
        public void DisableAllInput()
        {
            playerActionMap?.Disable();
            uiActionMap?.Disable();
            isInputEnabled = false;
        }
        
        /// <summary>
        /// 현재 이동 입력값 반환
        /// </summary>
        public Vector2 GetMoveInput() => currentMoveInput;
        
        /// <summary>
        /// 특정 액션이 눌려있는지 확인
        /// </summary>
        public bool IsActionPressed(string actionName)
        {
            var action = playerActionMap?.FindAction(actionName);
            return action?.IsPressed() ?? false;
        }
        
        private void OnDestroy()
        {
            UnregisterInputCallbacks();
        }
        
        private void OnDisable()
        {
            DisableAllInput();
        }
    }
}