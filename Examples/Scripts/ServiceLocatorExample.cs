using UnityEngine;
using Unity2DFramework.Core.Managers;

namespace Unity2DFramework.Examples
{
    /// <summary>
    /// ServiceLocator 사용 예제 클래스
    /// 게임 초기화 시 서비스들을 등록하고 관리하는 방법을 보여줍니다.
    /// </summary>
    public class ServiceLocatorExample : MonoBehaviour
    {
        [Header("예제 서비스들")]
        [SerializeField] private ExampleAudioService audioService;
        [SerializeField] private ExampleUIService uiService;
        [SerializeField] private ExamplePlayerService playerService;
        
        private void Awake()
        {
            // 게임 시작 시 모든 서비스를 ServiceLocator에 등록
            RegisterServices();
        }
        
        private void Start()
        {
            // 서비스 등록 후 테스트
            TestServiceLocator();
        }
        
        /// <summary>
        /// 모든 서비스를 ServiceLocator에 등록
        /// </summary>
        private void RegisterServices()
        {
            Debug.Log("=== ServiceLocator 예제: 서비스 등록 시작 ===");
            
            // 각 서비스를 타입별로 등록
            ServiceLocator.Instance.RegisterService<ExampleAudioService>(audioService);
            ServiceLocator.Instance.RegisterService<ExampleUIService>(uiService);
            ServiceLocator.Instance.RegisterService<ExamplePlayerService>(playerService);
            
            Debug.Log("=== 모든 서비스 등록 완료 ===");
        }
        
        /// <summary>
        /// ServiceLocator 기능 테스트
        /// </summary>
        private void TestServiceLocator()
        {
            Debug.Log("=== ServiceLocator 기능 테스트 시작 ===");
            
            // 1. 서비스 존재 확인 테스트
            TestServiceExistence();
            
            // 2. 서비스 가져오기 테스트
            TestServiceRetrieval();
            
            // 3. 자동 탐지 기능 테스트
            TestAutoDiscovery();
            
            // 4. 등록된 서비스 목록 출력
            ServiceLocator.Instance.PrintRegisteredServices();
            
            Debug.Log("=== ServiceLocator 기능 테스트 완료 ===");
        }
        
        /// <summary>
        /// 서비스 존재 확인 테스트
        /// </summary>
        private void TestServiceExistence()
        {
            Debug.Log("--- 서비스 존재 확인 테스트 ---");
            
            bool hasAudio = ServiceLocator.Instance.HasService<ExampleAudioService>();
            bool hasUI = ServiceLocator.Instance.HasService<ExampleUIService>();
            bool hasPlayer = ServiceLocator.Instance.HasService<ExamplePlayerService>();
            bool hasNonExistent = ServiceLocator.Instance.HasService<Rigidbody2D>();
            
            Debug.Log($"AudioService 존재: {hasAudio}");
            Debug.Log($"UIService 존재: {hasUI}");
            Debug.Log($"PlayerService 존재: {hasPlayer}");
            Debug.Log($"존재하지 않는 서비스: {hasNonExistent}");
        }
        
        /// <summary>
        /// 서비스 가져오기 테스트
        /// </summary>
        private void TestServiceRetrieval()
        {
            Debug.Log("--- 서비스 가져오기 테스트 ---");
            
            var audio = ServiceLocator.Instance.GetService<ExampleAudioService>();
            var ui = ServiceLocator.Instance.GetService<ExampleUIService>();
            var player = ServiceLocator.Instance.GetService<ExamplePlayerService>();
            
            // 가져온 서비스들 사용 테스트
            audio?.PlaySound("TestSound");
            ui?.ShowMessage("ServiceLocator 테스트 메시지");
            player?.MovePlayer(Vector2.right);
        }
        
        /// <summary>
        /// 자동 탐지 기능 테스트
        /// </summary>
        private void TestAutoDiscovery()
        {
            Debug.Log("--- 자동 탐지 기능 테스트 ---");
            
            // 등록되지 않은 컴포넌트를 자동으로 찾아서 등록하는 테스트
            // (실제로는 씬에 해당 컴포넌트가 있어야 함)
            var camera = ServiceLocator.Instance.GetOrFindService<Camera>();
            if (camera != null)
            {
                Debug.Log($"카메라 자동 탐지 성공: {camera.name}");
            }
        }
        
        /// <summary>
        /// 키 입력으로 ServiceLocator 기능 테스트
        /// </summary>
        private void Update()
        {
            // F1: 등록된 서비스 목록 출력
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ServiceLocator.Instance.PrintRegisteredServices();
            }
            
            // F2: 서비스 사용 테스트
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestServiceUsage();
            }
            
            // F3: 서비스 해제 테스트
            if (Input.GetKeyDown(KeyCode.F3))
            {
                TestServiceUnregistration();
            }
        }
        
        /// <summary>
        /// 서비스 사용 테스트
        /// </summary>
        private void TestServiceUsage()
        {
            Debug.Log("--- 서비스 사용 테스트 (F2) ---");
            
            var audioService = ServiceLocator.Instance.GetService<ExampleAudioService>();
            var uiService = ServiceLocator.Instance.GetService<ExampleUIService>();
            
            audioService?.PlaySound("ButtonClick");
            uiService?.ShowMessage($"현재 시간: {System.DateTime.Now:HH:mm:ss}");
        }
        
        /// <summary>
        /// 서비스 해제 테스트
        /// </summary>
        private void TestServiceUnregistration()
        {
            Debug.Log("--- 서비스 해제 테스트 (F3) ---");
            
            // AudioService 해제
            ServiceLocator.Instance.UnregisterService<ExampleAudioService>();
            
            // 해제 후 다시 가져오기 시도
            var audioService = ServiceLocator.Instance.GetService<ExampleAudioService>();
            if (audioService == null)
            {
                Debug.Log("AudioService가 성공적으로 해제되었습니다.");
            }
        }
        
        private void OnDestroy()
        {
            Debug.Log("ServiceLocatorExample 종료");
        }
    }
    
    /// <summary>
    /// 예제용 오디오 서비스
    /// </summary>
    public class ExampleAudioService : MonoBehaviour
    {
        public void PlaySound(string soundName)
        {
            Debug.Log($"🔊 오디오 재생: {soundName}");
        }
        
        public void StopSound()
        {
            Debug.Log("🔇 오디오 정지");
        }
    }
    
    /// <summary>
    /// 예제용 UI 서비스
    /// </summary>
    public class ExampleUIService : MonoBehaviour
    {
        public void ShowMessage(string message)
        {
            Debug.Log($"💬 UI 메시지: {message}");
        }
        
        public void HideMessage()
        {
            Debug.Log("❌ UI 메시지 숨김");
        }
    }
    
    /// <summary>
    /// 예제용 플레이어 서비스
    /// </summary>
    public class ExamplePlayerService : MonoBehaviour
    {
        public void MovePlayer(Vector2 direction)
        {
            Debug.Log($"🏃 플레이어 이동: {direction}");
        }
        
        public void StopPlayer()
        {
            Debug.Log("⏹️ 플레이어 정지");
        }
    }
}