using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity2DFramework.Core.Input
{
    /// <summary>
    /// 플레이어 입력을 처리하고 플레이어 컨트롤러에 전달하는 컴포넌트
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        // 이동 관련 이벤트
        public System.Action<Vector2> OnMoveInput;
        
        // 액션 이벤트
        public System.Action OnJumpInput;
        public System.Action OnJumpCanceledInput;
        public System.Action OnAttackInput;
        public System.Action OnInteractInput;
        public System.Action OnNextInput;
        public System.Action OnPreviousInput;
        public System.Action<bool> OnSprintInput;
        
        // 메뉴 이벤트
        public System.Action OnPauseInput;
        
        // 추가 상태 정보
        private Vector2 currentMoveInput;
        
        private InputManager inputManager;
        
        private void Awake()
        {
            // InputManager 인스턴스 확인
            if (InputManager.Instance == null)
            {
                Debug.LogError("[PlayerInputController] InputManager 인스턴스가 없습니다. 씬에 InputManager 객체가 있는지 확인하세요.");
            }
            else
            {
                inputManager = InputManager.Instance;
            }
        }
        
        private void OnEnable()
        {
            if (inputManager != null)
            {
                // 입력 이벤트에 핸들러 등록
                inputManager.OnMove += HandleMoveInput;
                inputManager.OnJump += HandleJumpInput;
                inputManager.OnJumpCanceled += HandleJumpCanceledInput;
                inputManager.OnAttack += HandleAttackInput;
                inputManager.OnInteract += HandleInteractInput;
                inputManager.OnPause += HandlePauseInput;
                inputManager.OnNext += HandleNextInput;
                inputManager.OnPrevious += HandlePreviousInput;
                inputManager.OnSprint += HandleSprintInput;
            }
        }
        
        private void OnDisable()
        {
            if (inputManager != null)
            {
                // 입력 이벤트에서 핸들러 제거
                inputManager.OnMove -= HandleMoveInput;
                inputManager.OnJump -= HandleJumpInput;
                inputManager.OnJumpCanceled -= HandleJumpCanceledInput;
                inputManager.OnAttack -= HandleAttackInput;
                inputManager.OnInteract -= HandleInteractInput;
                inputManager.OnPause -= HandlePauseInput;
                inputManager.OnNext -= HandleNextInput;
                inputManager.OnPrevious -= HandlePreviousInput;
                inputManager.OnSprint -= HandleSprintInput;
            }
        }
        
        #region 입력 핸들러
        /// <summary>
        /// 이동 입력 처리
        /// </summary>
        private void HandleMoveInput(Vector2 moveInput)
        {
            currentMoveInput = moveInput;
            OnMoveInput?.Invoke(moveInput);
        }
        
        /// <summary>
        /// 점프 입력 처리
        /// </summary>
        private void HandleJumpInput()
        {
            OnJumpInput?.Invoke();
        }
        
        /// <summary>
        /// 점프 취소 입력 처리
        /// </summary>
        private void HandleJumpCanceledInput()
        {
            OnJumpCanceledInput?.Invoke();
        }
        
        /// <summary>
        /// 공격 입력 처리
        /// </summary>
        private void HandleAttackInput()
        {
            OnAttackInput?.Invoke();
        }
        
        /// <summary>
        /// 상호작용 입력 처리
        /// </summary>
        private void HandleInteractInput()
        {
            OnInteractInput?.Invoke();
        }
        
        /// <summary>
        /// 일시정지 입력 처리
        /// </summary>
        private void HandlePauseInput()
        {
            OnPauseInput?.Invoke();
        }
        
        /// <summary>
        /// 다음 아이템/스킬 선택 입력 처리
        /// </summary>
        private void HandleNextInput()
        {
            OnNextInput?.Invoke();
        }
        
        /// <summary>
        /// 이전 아이템/스킬 선택 입력 처리
        /// </summary>
        private void HandlePreviousInput()
        {
            OnPreviousInput?.Invoke();
        }
        
        /// <summary>
        /// 달리기 입력 처리
        /// </summary>
        private void HandleSprintInput(bool isSprinting)
        {
            OnSprintInput?.Invoke(isSprinting);
        }
        #endregion
        
        /// <summary>
        /// 현재 이동 입력 벡터 반환
        /// </summary>
        public Vector2 GetMoveInput()
        {
            return currentMoveInput;
        }
        
        /// <summary>
        /// 입력을 활성화 또는 비활성화합니다.
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            if (inputManager != null)
            {
                if (enabled)
                {
                    inputManager.EnablePlayerInput();
                }
                else
                {
                    inputManager.DisableAllInput();
                }
            }
        }
        
        /// <summary>
        /// UI 모드로 전환합니다. (게임 입력 비활성화, UI 입력 활성화)
        /// </summary>
        public void EnableUIMode()
        {
            if (inputManager != null)
            {
                inputManager.EnableUIInput();
            }
        }
        
        /// <summary>
        /// 게임 모드로 전환합니다. (게임 입력 활성화, UI 입력 비활성화)
        /// </summary>
        public void EnableGameMode()
        {
            if (inputManager != null)
            {
                inputManager.EnablePlayerInput();
            }
        }
    }
}