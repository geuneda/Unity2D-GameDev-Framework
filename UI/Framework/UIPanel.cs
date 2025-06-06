using UnityEngine;
using Unity2DFramework.Animation.Tweening;
using DG.Tweening;

namespace Unity2DFramework.UI.Framework
{
    /// <summary>
    /// UI 패널의 기본 클래스
    /// 표시/숨김 애니메이션과 기본 기능들을 제공
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [Header("패널 설정")]
        [SerializeField] protected string panelName;
        [SerializeField] protected bool startVisible = false;
        
        [Header("애니메이션 설정")]
        [SerializeField] protected float animationDuration = 0.3f;
        [SerializeField] protected Ease showEase = Ease.OutBack;
        [SerializeField] protected Ease hideEase = Ease.InBack;
        
        // 캐싱된 컴포넌트들
        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        
        // 상태
        protected bool isVisible;
        protected bool isAnimating;
        
        // 이벤트
        public System.Action OnShowComplete;
        public System.Action OnHideComplete;
        
        protected virtual void Awake()
        {
            // 컴포넌트 캐싱
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            
            // CanvasGroup이 없으면 추가
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        protected virtual void Start()
        {
            // 초기 상태 설정
            if (startVisible)
            {
                ShowImmediate();
            }
            else
            {
                HideImmediate();
            }
        }
        
        /// <summary>
        /// 패널 초기화
        /// </summary>
        public virtual void Initialize()
        {
            // 하위 클래스에서 구현
        }
        
        /// <summary>
        /// 패널 표시 (애니메이션 포함)
        /// </summary>
        public virtual void Show()
        {
            if (isVisible || isAnimating) return;
            
            gameObject.SetActive(true);
            isAnimating = true;
            
            // 표시 애니메이션 실행
            PlayShowAnimation();
        }
        
        /// <summary>
        /// 패널 숨김 (애니메이션 포함)
        /// </summary>
        public virtual void Hide()
        {
            if (!isVisible || isAnimating) return;
            
            isAnimating = true;
            
            // 숨김 애니메이션 실행
            PlayHideAnimation();
        }
        
        /// <summary>
        /// 즉시 표시 (애니메이션 없음)
        /// </summary>
        public virtual void ShowImmediate()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale = Vector3.one;
            
            isVisible = true;
            isAnimating = false;
            
            OnShowComplete?.Invoke();
        }
        
        /// <summary>
        /// 즉시 숨김 (애니메이션 없음)
        /// </summary>
        public virtual void HideImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            
            isVisible = false;
            isAnimating = false;
            
            OnHideComplete?.Invoke();
        }
        
        /// <summary>
        /// 표시 애니메이션 재생
        /// </summary>
        protected virtual void PlayShowAnimation()
        {
            // 초기 상태 설정
            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.zero;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            // 애니메이션 시퀀스
            Sequence showSequence = DOTween.Sequence();
            showSequence.Append(canvasGroup.DOFade(1f, animationDuration).SetEase(showEase));
            showSequence.Join(rectTransform.DOScale(Vector3.one, animationDuration).SetEase(showEase));
            showSequence.OnComplete(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                isVisible = true;
                isAnimating = false;
                OnShowComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 숨김 애니메이션 재생
        /// </summary>
        protected virtual void PlayHideAnimation()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            // 애니메이션 시퀀스
            Sequence hideSequence = DOTween.Sequence();
            hideSequence.Append(canvasGroup.DOFade(0f, animationDuration).SetEase(hideEase));
            hideSequence.Join(rectTransform.DOScale(Vector3.zero, animationDuration).SetEase(hideEase));
            hideSequence.OnComplete(() => {
                gameObject.SetActive(false);
                isVisible = false;
                isAnimating = false;
                OnHideComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 토글 (표시/숨김 전환)
        /// </summary>
        public void Toggle()
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        
        // 프로퍼티
        public string PanelName => panelName;
        public bool IsVisible => isVisible;
        public bool IsAnimating => isAnimating;
    }
}