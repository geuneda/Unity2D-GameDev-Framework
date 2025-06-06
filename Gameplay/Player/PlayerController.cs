using UnityEngine;
using Unity2DFramework.Core.Input;

namespace Unity2DFramework.Gameplay.Player
{
    /// <summary>
    /// 플레이어의 기본 동작을 제어하는 컨트롤러
    /// 이동, 점프, 공격 등의 핵심 기능을 담당
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        
        [Header("점프 설정")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        
        [Header("지면 체크")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayerMask;
        
        // 캐싱된 컴포넌트 참조
        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        
        // 애니메이션 해시 ID (성능 최적화)
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");
        private static readonly int VelocityYHash = Animator.StringToHash("VelocityY");
        
        // 플레이어 상태
        private Vector2 moveInput;
        private bool isGrounded;
        private bool isJumping;
        private float horizontalVelocity;
        
        private void Awake()
        {
            // 컴포넌트 참조 캐싱
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void Start()
        {
            // 입력 이벤트 구독
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMove += HandleMove;
                InputManager.Instance.OnJump += HandleJump;
                InputManager.Instance.OnJumpCanceled += HandleJumpCanceled;
            }
        }
        
        private void Update()
        {
            CheckGrounded();
            UpdateAnimations();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
            HandleJumpPhysics();
        }
        
        /// <summary>
        /// 이동 입력 처리
        /// </summary>
        private void HandleMove(Vector2 input)
        {
            moveInput = input;
        }
        
        /// <summary>
        /// 점프 입력 처리
        /// </summary>
        private void HandleJump()
        {
            if (isGrounded && !isJumping)
            {
                Jump();
            }
        }
        
        /// <summary>
        /// 점프 취소 처리 (가변 점프 높이)
        /// </summary>
        private void HandleJumpCanceled()
        {
            if (isJumping && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        }
        
        /// <summary>
        /// 이동 처리
        /// </summary>
        private void HandleMovement()
        {
            float targetVelocity = moveInput.x * moveSpeed;
            
            // 부드러운 가속/감속
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                horizontalVelocity = Mathf.MoveTowards(horizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                horizontalVelocity = Mathf.MoveTowards(horizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
            }
            
            rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);
            
            // 스프라이트 방향 전환
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                spriteRenderer.flipX = moveInput.x < 0;
            }
        }
        
        /// <summary>
        /// 점프 실행
        /// </summary>
        private void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
            
            // 점프 애니메이션 트리거
            if (animator != null)
            {
                animator.SetTrigger(JumpTriggerHash);
            }
        }
        
        /// <summary>
        /// 점프 물리 처리 (더 나은 점프 느낌)
        /// </summary>
        private void HandleJumpPhysics()
        {
            if (rb.velocity.y < 0)
            {
                // 떨어질 때 중력 증가
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
                isJumping = false;
            }
            else if (rb.velocity.y > 0 && !InputManager.Instance.IsActionPressed("Jump"))
            {
                // 점프 키를 놓았을 때 중력 증가 (낮은 점프)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
        
        /// <summary>
        /// 지면 체크
        /// </summary>
        private void CheckGrounded()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayerMask);
            
            // 착지 시 점프 상태 리셋
            if (!wasGrounded && isGrounded)
            {
                isJumping = false;
            }
        }
        
        /// <summary>
        /// 애니메이션 업데이트
        /// </summary>
        private void UpdateAnimations()
        {
            if (animator == null) return;
            
            // 달리기 애니메이션
            bool isRunning = Mathf.Abs(horizontalVelocity) > 0.1f && isGrounded;
            animator.SetBool(IsRunningHash, isRunning);
            
            // 지면 상태
            animator.SetBool(IsGroundedHash, isGrounded);
            
            // Y축 속도 (점프/낙하 애니메이션용)
            animator.SetFloat(VelocityYHash, rb.velocity.y);
        }
        
        /// <summary>
        /// 기즈모 그리기 (지면 체크 시각화)
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }
        }
        
        private void OnDestroy()
        {
            // 입력 이벤트 구독 해제
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMove -= HandleMove;
                InputManager.Instance.OnJump -= HandleJump;
                InputManager.Instance.OnJumpCanceled -= HandleJumpCanceled;
            }
        }
        
        // 프로퍼티
        public bool IsGrounded => isGrounded;
        public bool IsJumping => isJumping;
        public Vector2 Velocity => rb.velocity;
    }
}