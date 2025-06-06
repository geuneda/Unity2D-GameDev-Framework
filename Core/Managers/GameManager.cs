using UnityEngine;
using System.Collections;

namespace Unity2DFramework.Core.Managers
{
    /// <summary>
    /// 게임의 전반적인 상태와 흐름을 관리하는 핵심 매니저
    /// 싱글톤 패턴으로 구현되어 전역 접근 가능
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("게임 설정")]
        [SerializeField] private bool isPaused = false;
        [SerializeField] private float gameSpeed = 1f;
        
        // 게임 상태 이벤트
        public System.Action OnGameStart;
        public System.Action OnGamePause;
        public System.Action OnGameResume;
        public System.Action OnGameEnd;
        
        // 캐싱된 컴포넌트 참조
        private AudioManager audioManager;
        private InputManager inputManager;
        private SceneTransitionManager sceneManager;
        
        private void Awake()
        {
            // 싱글톤 패턴 구현
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 모든 매니저들을 초기화
        /// </summary>
        private void InitializeManagers()
        {
            // 매니저 참조 캐싱
            audioManager = AudioManager.Instance;
            inputManager = InputManager.Instance;
            sceneManager = SceneTransitionManager.Instance;
            
            // 각 매니저 초기화
            audioManager?.Initialize();
            inputManager?.Initialize();
            sceneManager?.Initialize();
            
            Debug.Log("[GameManager] 모든 매니저 초기화 완료");
        }
        
        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            isPaused = false;
            Time.timeScale = gameSpeed;
            OnGameStart?.Invoke();
            Debug.Log("[GameManager] 게임 시작");
        }
        
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            if (!isPaused)
            {
                isPaused = true;
                Time.timeScale = 0f;
                OnGamePause?.Invoke();
                Debug.Log("[GameManager] 게임 일시정지");
            }
        }
        
        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (isPaused)
            {
                isPaused = false;
                Time.timeScale = gameSpeed;
                OnGameResume?.Invoke();
                Debug.Log("[GameManager] 게임 재개");
            }
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        public void EndGame()
        {
            OnGameEnd?.Invoke();
            Debug.Log("[GameManager] 게임 종료");
        }
        
        /// <summary>
        /// 게임 속도 설정
        /// </summary>
        public void SetGameSpeed(float speed)
        {
            gameSpeed = Mathf.Clamp(speed, 0f, 3f);
            if (!isPaused)
            {
                Time.timeScale = gameSpeed;
            }
        }
        
        // 프로퍼티
        public bool IsPaused => isPaused;
        public float GameSpeed => gameSpeed;
    }
}