using UnityEngine;
using Unity2DFramework.Gameplay.Player;
using Unity2DFramework.Core.Managers;
using Unity2DFramework.Data.Save;

namespace Unity2DFramework.Examples
{
    /// <summary>
    /// PlayerController 사용 예제
    /// 실제 게임에서 어떻게 활용하는지 보여주는 예제 클래스
    /// </summary>
    public class ExamplePlayerController : MonoBehaviour
    {
        [Header("플레이어 설정")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private int playerHealth = 100;
        [SerializeField] private int maxHealth = 100;
        
        [Header("오디오 클립")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip landSound;
        [SerializeField] private AudioClip hurtSound;
        
        // 플레이어 상태
        private bool isAlive = true;
        private float lastGroundedTime;
        
        private void Start()
        {
            // 컴포넌트 참조 확인
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
            
            // 저장된 데이터에서 체력 로드
            if (SaveSystem.Instance != null)
            {
                playerHealth = SaveSystem.Instance.GetIntValue("PlayerHealth", maxHealth);
            }
            
            // 입력 이벤트 구독
            SubscribeToInputEvents();
        }
        
        /// <summary>
        /// 입력 이벤트 구독
        /// </summary>
        private void SubscribeToInputEvents()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnJump += OnJumpInput;
                InputManager.Instance.OnAttack += OnAttackInput;
                InputManager.Instance.OnInteract += OnInteractInput;
            }
        }
        
        private void Update()
        {
            // 플레이어 상태 체크
            CheckPlayerState();
            
            // 착지 사운드 체크
            CheckLandingSound();
        }
        
        /// <summary>
        /// 플레이어 상태 확인
        /// </summary>
        private void CheckPlayerState()
        {
            if (!isAlive) return;
            
            // 낙사 체크
            if (transform.position.y < -10f)
            {
                TakeDamage(maxHealth); // 즉사
            }
        }
        
        /// <summary>
        /// 착지 사운드 체크
        /// </summary>
        private void CheckLandingSound()
        {
            if (playerController == null) return;
            
            // 이전에 공중에 있었다가 지금 착지했을 때
            if (!playerController.IsGrounded)
            {
                lastGroundedTime = Time.time;
            }
            else if (Time.time - lastGroundedTime > 0.1f && playerController.Velocity.y <= 0)
            {
                // 착지 사운드 재생
                if (landSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(landSound, 0.7f);
                }
                lastGroundedTime = Time.time;
            }
        }
        
        /// <summary>
        /// 점프 입력 처리
        /// </summary>
        private void OnJumpInput()
        {
            if (!isAlive) return;
            
            // 점프 사운드 재생
            if (jumpSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(jumpSound, 0.8f);
            }
        }
        
        /// <summary>
        /// 공격 입력 처리
        /// </summary>
        private void OnAttackInput()
        {
            if (!isAlive) return;
            
            Debug.Log("플레이어 공격!");
            // 공격 로직 구현
        }
        
        /// <summary>
        /// 상호작용 입력 처리
        /// </summary>
        private void OnInteractInput()
        {
            if (!isAlive) return;
            
            Debug.Log("상호작용 시도");
            // 상호작용 로직 구현
        }
        
        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!isAlive) return;
            
            playerHealth -= damage;
            playerHealth = Mathf.Max(0, playerHealth);
            
            // 피해 사운드 재생
            if (hurtSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(hurtSound);
            }
            
            // 체력 저장
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SetIntValue("PlayerHealth", playerHealth);
            }
            
            // 사망 체크
            if (playerHealth <= 0)
            {
                Die();
            }
            
            Debug.Log($"플레이어 체력: {playerHealth}/{maxHealth}");
        }
        
        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(int amount)
        {
            if (!isAlive) return;
            
            playerHealth += amount;
            playerHealth = Mathf.Min(maxHealth, playerHealth);
            
            // 체력 저장
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SetIntValue("PlayerHealth", playerHealth);
            }
            
            Debug.Log($"플레이어 체력 회복: {playerHealth}/{maxHealth}");
        }
        
        /// <summary>
        /// 플레이어 사망
        /// </summary>
        private void Die()
        {
            isAlive = false;
            
            Debug.Log("플레이어 사망!");
            
            // 입력 비활성화
            if (InputManager.Instance != null)
            {
                InputManager.Instance.DisableAllInput();
            }
            
            // 게임 오버 처리
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame();
            }
        }
        
        /// <summary>
        /// 플레이어 부활
        /// </summary>
        public void Respawn(Vector3 respawnPosition)
        {
            transform.position = respawnPosition;
            playerHealth = maxHealth;
            isAlive = true;
            
            // 입력 재활성화
            if (InputManager.Instance != null)
            {
                InputManager.Instance.EnablePlayerInput();
            }
            
            Debug.Log("플레이어 부활!");
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnJump -= OnJumpInput;
                InputManager.Instance.OnAttack -= OnAttackInput;
                InputManager.Instance.OnInteract -= OnInteractInput;
            }
        }
        
        // 프로퍼티
        public int Health => playerHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => isAlive;
        public float HealthPercentage => (float)playerHealth / maxHealth;
    }
}