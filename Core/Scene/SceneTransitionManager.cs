using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

namespace Unity2DFramework.Core.Scene
{
    /// <summary>
    /// 씬 전환과 로딩을 관리하는 매니저
    /// 페이드 효과와 로딩 화면을 포함한 부드러운 씬 전환 제공
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }
        
        [Header("페이드 설정")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        [Header("로딩 화면")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private UnityEngine.UI.Slider loadingProgressBar;
        [SerializeField] private TMPro.TextMeshProUGUI loadingText;
        
        // 씬 전환 이벤트
        public System.Action<string> OnSceneLoadStart;
        public System.Action<string> OnSceneLoadComplete;
        public System.Action<float> OnLoadingProgress;
        
        // 현재 로딩 상태
        private bool isLoading = false;
        private string currentSceneName;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 씬 전환 매니저 초기화
        /// </summary>
        public void Initialize()
        {
            // 초기 상태 설정
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.gameObject.SetActive(false);
            }
            
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
            
            currentSceneName = SceneManager.GetActiveScene().name;
            
            Debug.Log("[SceneTransitionManager] 씬 전환 매니저 초기화 완료");
        }
        
        /// <summary>
        /// 씬을 비동기로 로드 (페이드 효과 포함)
        /// </summary>
        public void LoadScene(string sceneName, bool showLoadingScreen = true)
        {
            if (isLoading)
            {
                Debug.LogWarning("[SceneTransitionManager] 이미 씬 로딩 중입니다.");
                return;
            }
            
            StartCoroutine(LoadSceneCoroutine(sceneName, showLoadingScreen));
        }
        
        /// <summary>
        /// 씬 로딩 코루틴
        /// </summary>
        private IEnumerator LoadSceneCoroutine(string sceneName, bool showLoadingScreen)
        {
            isLoading = true;
            OnSceneLoadStart?.Invoke(sceneName);
            
            // 페이드 아웃
            yield return StartCoroutine(FadeOut());
            
            // 로딩 화면 표시
            if (showLoadingScreen && loadingScreen != null)
            {
                loadingScreen.SetActive(true);
                UpdateLoadingText("로딩 중...");
            }
            
            // 씬 비동기 로딩 시작
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // 로딩 진행률 업데이트
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                // 로딩 바 업데이트
                if (loadingProgressBar != null)
                {
                    loadingProgressBar.value = progress;
                }
                
                // 로딩 텍스트 업데이트
                UpdateLoadingText($"로딩 중... {(int)(progress * 100)}%");
                
                OnLoadingProgress?.Invoke(progress);
                
                // 로딩 완료 시 씬 활성화
                if (asyncLoad.progress >= 0.9f)
                {
                    UpdateLoadingText("완료!");
                    yield return new WaitForSeconds(0.5f); // 잠시 대기
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            // 로딩 화면 숨기기
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
            
            // 페이드 인
            yield return StartCoroutine(FadeIn());
            
            currentSceneName = sceneName;
            isLoading = false;
            OnSceneLoadComplete?.Invoke(sceneName);
            
            Debug.Log($"[SceneTransitionManager] 씬 '{sceneName}' 로딩 완료");
        }
        
        /// <summary>
        /// 페이드 아웃 효과
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (fadeCanvasGroup == null) yield break;
            
            fadeCanvasGroup.gameObject.SetActive(true);
            
            // DOTween을 사용한 부드러운 페이드
            yield return fadeCanvasGroup.DOFade(1f, fadeOutDuration).WaitForCompletion();
        }
        
        /// <summary>
        /// 페이드 인 효과
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;
            
            // DOTween을 사용한 부드러운 페이드
            yield return fadeCanvasGroup.DOFade(0f, fadeInDuration).WaitForCompletion();
            
            fadeCanvasGroup.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 로딩 텍스트 업데이트
        /// </summary>
        private void UpdateLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }
        
        /// <summary>
        /// 현재 씬 이름 반환
        /// </summary>
        public string GetCurrentSceneName() => currentSceneName;
        
        /// <summary>
        /// 로딩 중인지 확인
        /// </summary>
        public bool IsLoading => isLoading;
        
        /// <summary>
        /// 게임 재시작 (현재 씬 다시 로드)
        /// </summary>
        public void RestartCurrentScene()
        {
            LoadScene(currentSceneName);
        }
        
        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public void LoadMainMenu()
        {
            LoadScene("MainMenu");
        }
    }
}