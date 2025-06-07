using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Unity2DFramework.Animation.Tweening
{
    /// <summary>
    /// DOTween을 활용한 트위닝 애니메이션 관리자
    /// 자주 사용되는 트위닝 패턴들을 미리 정의하여 편리하게 사용
    /// </summary>
    public class TweenManager : MonoBehaviour
    {
        public static TweenManager Instance { get; private set; }
        
        [Header("기본 설정")]
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private int maxTweenersCapacity = 200;
        [SerializeField] private int maxSequencesCapacity = 50;
        
        // 트위닝 프리셋
        [Header("트위닝 프리셋")]
        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private Ease defaultEase = Ease.OutQuart;
        
                // 활성 트윈들을 추적하기 위한 딕셔너리
    private Dictionary<string, Tween> activeTweens = new Dictionary<string, Tween>();
    
    // UI 요소별 특화 트윈 (ID 기반)
    private Dictionary<string, Tween> uiTweens = new Dictionary<string, Tween>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (initializeOnAwake)
                {
                    InitializeDOTween();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// DOTween 초기화
        /// </summary>
        private void InitializeDOTween()
        {
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly)
                   .SetCapacity(maxTweenersCapacity, maxSequencesCapacity);
            
            Debug.Log("[TweenManager] DOTween 초기화 완료");
        }
        
        #region UI 트위닝 메서드들
        
        /// <summary>
        /// UI 요소 페이드 인
        /// </summary>
        public Tween FadeIn(CanvasGroup canvasGroup, float duration = -1f, Ease ease = Ease.Unset, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);
            
            var tween = canvasGroup.DOFade(1f, duration).SetEase(ease);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterUITween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// UI 요소 페이드 아웃
        /// </summary>
        public Tween FadeOut(CanvasGroup canvasGroup, float duration = -1f, Ease ease = Ease.Unset, bool deactivateOnComplete = true, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            Tween tween = canvasGroup.DOFade(0f, duration).SetEase(ease);
            
            if (deactivateOnComplete)
            {
                tween.OnComplete(() => canvasGroup.gameObject.SetActive(false));
            }
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterUITween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// UI 요소 스케일 애니메이션 (팝업 효과)
        /// </summary>
        public Tween PopupScale(Transform transform, float duration = -1f, Ease ease = Ease.Unset, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = Ease.OutBack;
            
            transform.localScale = Vector3.zero;
            var tween = transform.DOScale(Vector3.one, duration).SetEase(ease);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterUITween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// UI 요소 슬라이드 인 (위에서 아래로)
        /// </summary>
        public Tween SlideInFromTop(RectTransform rectTransform, float duration = -1f, Ease ease = Ease.Unset, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            Vector2 originalPos = rectTransform.anchoredPosition;
            Vector2 startPos = originalPos + Vector2.up * Screen.height;
            
            rectTransform.anchoredPosition = startPos;
            var tween = rectTransform.DOAnchorPos(originalPos, duration).SetEase(ease);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterUITween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// UI 요소 슬라이드 아웃 (아래에서 위로)
        /// </summary>
        public Tween SlideOutToTop(RectTransform rectTransform, float duration = -1f, Ease ease = Ease.Unset, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            Vector2 targetPos = rectTransform.anchoredPosition + Vector2.up * Screen.height;
            var tween = rectTransform.DOAnchorPos(targetPos, duration).SetEase(ease);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterUITween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// UI 요소 흔들기 효과
        /// </summary>
        public Tween ShakeUI(RectTransform rectTransform, float strength = 10f, float duration = -1f, int vibrato = 10, float randomness = 90f, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration * 0.5f;
            
            var tween = rectTransform.DOShakeAnchorPos(duration, strength, vibrato, randomness);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterUITween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// 레이아웃 요소 새로고침
        /// </summary>
        public void RefreshLayoutElements(RectTransform rectTransform)
        {
            if (rectTransform == null || !rectTransform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in rectTransform)
            {
                RefreshLayoutElements(child);
            }

            var layoutGroup = rectTransform.GetComponent<LayoutGroup>();
            var contentSizeFitter = rectTransform.GetComponent<ContentSizeFitter>();
            
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }
        
        #endregion
        
        #region 게임플레이 트위닝 메서드들
        
        /// <summary>
        /// 오브젝트 바운스 효과
        /// </summary>
        public Tween Bounce(Transform transform, float bounceHeight = 1f, float duration = -1f, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            
            Vector3 originalPos = transform.position;
            Vector3 bouncePos = originalPos + Vector3.up * bounceHeight;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(bouncePos, duration * 0.5f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOMove(originalPos, duration * 0.5f).SetEase(Ease.InQuad));
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterTween(tweenId, sequence);
            }
            
            return sequence;
        }
        
        /// <summary>
        /// 오브젝트 흔들기 효과
        /// </summary>
        public Tween Shake(Transform transform, float strength = 1f, float duration = -1f, int vibrato = 10, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            
            var tween = transform.DOShakePosition(duration, strength, vibrato);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterTween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// 오브젝트 펄스 효과 (크기 변화)
        /// </summary>
        public Tween Pulse(Transform transform, float scaleMultiplier = 1.2f, float duration = -1f, string tweenId = null)
        {
            if (duration < 0) duration = defaultDuration;
            
            Vector3 originalScale = transform.localScale;
            Vector3 pulseScale = originalScale * scaleMultiplier;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(pulseScale, duration * 0.5f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(originalScale, duration * 0.5f).SetEase(Ease.InQuad));
            sequence.SetLoops(-1, LoopType.Restart);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterTween(tweenId, sequence);
            }
            
            return sequence;
        }
        
        /// <summary>
        /// 오브젝트 회전 애니메이션
        /// </summary>
        public Tween RotateLoop(Transform transform, Vector3 rotation, float duration = 1f, string tweenId = null)
        {
            var tween = transform.DORotate(rotation, duration, RotateMode.FastBeyond360)
                          .SetLoops(-1, LoopType.Incremental)
                          .SetEase(Ease.Linear);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterTween(tweenId, tween);
            }
            
            return tween;
        }
        
        #endregion
        
        #region 트윈 관리 메서드들
        
        /// <summary>
        /// 이름을 가진 트윈 등록
        /// </summary>
        public void RegisterTween(string name, Tween tween)
        {
            if (activeTweens.ContainsKey(name))
            {
                activeTweens[name].Kill();
            }
            
            activeTweens[name] = tween;
            
            // 트윈 완료 시 딕셔너리에서 제거
            tween.OnComplete(() => activeTweens.Remove(name));
        }
        
        /// <summary>
        /// UI 전용 트윈 등록
        /// </summary>
        private void RegisterUITween(string id, Tween tween)
        {
            if (uiTweens.ContainsKey(id))
            {
                uiTweens[id].Kill();
            }
            
            uiTweens[id] = tween;
            
            // 트윈 완료 시 딕셔너리에서 제거
            tween.OnComplete(() => uiTweens.Remove(id));
        }
        
        /// <summary>
        /// 이름으로 트윈 중지
        /// </summary>
        public void KillTween(string name)
        {
            if (activeTweens.ContainsKey(name))
            {
                activeTweens[name].Kill();
                activeTweens.Remove(name);
            }
            
            if (uiTweens.ContainsKey(name))
            {
                uiTweens[name].Kill();
                uiTweens.Remove(name);
            }
        }
        
        /// <summary>
        /// UI 요소의 모든 트윈 중지
        /// </summary>
        public void KillUITweens()
        {
            foreach (var tween in uiTweens.Values)
            {
                tween.Kill();
            }
            uiTweens.Clear();
        }
        
        /// <summary>
        /// 모든 트윈 중지
        /// </summary>
        public void KillAllTweens()
        {
            DOTween.KillAll();
            activeTweens.Clear();
            uiTweens.Clear();
        }
        
        /// <summary>
        /// 특정 타겟의 모든 트윈 중지
        /// </summary>
        public void KillTweensOfTarget(object target)
        {
            DOTween.Kill(target);
        }
        
        /// <summary>
        /// 트윈 일시정지/재개
        /// </summary>
        public void PauseAllTweens()
        {
            DOTween.PauseAll();
        }
        
        public void ResumeAllTweens()
        {
            DOTween.PlayAll();
        }
        
        #endregion
        
        #region 유틸리티 메서드들
        
        /// <summary>
        /// 지연 실행
        /// </summary>
        public Tween DelayedCall(float delay, System.Action callback, string tweenId = null)
        {
            var tween = DOVirtual.DelayedCall(delay, callback);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterTween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// 값 보간
        /// </summary>
        public Tween InterpolateValue(float from, float to, float duration, System.Action<float> onUpdate, string tweenId = null)
        {
            var tween = DOVirtual.Float(from, to, duration, onUpdate);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterTween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// 색상 보간
        /// </summary>
        public Tween InterpolateColor(Color from, Color to, float duration, System.Action<Color> onUpdate, string tweenId = null)
        {
            var tween = DOVirtual.Color(from, to, duration, onUpdate);
            
            if (!string.IsNullOrEmpty(tweenId))
            {
                RegisterTween(tweenId, tween);
            }
            
            return tween;
        }
        
        /// <summary>
        /// UI 트윈 시퀀스 생성
        /// </summary>
        public Sequence CreateUISequence(string sequenceId = null)
        {
            var sequence = DOTween.Sequence();
            
            if (!string.IsNullOrEmpty(sequenceId))
            {
                RegisterUITween(sequenceId, sequence);
            }
            
            return sequence;
        }
        
        #endregion
        
        #region UIBase 통합 메서드들
        
        /// <summary>
        /// UI 요소 열기 애니메이션
        /// </summary>
        public void PlayOpenAnimation(GameObject uiObject, RectTransform rectTransform, Animator animator = null)
        {
            if (animator != null && animator.parameters.Length > 0)
            {
                // 애니메이터가 있는 경우 트리거 실행
                animator.SetTrigger("OpenUI");
            }
            else
            {
                // 애니메이터가 없는 경우 기본 애니메이션
                rectTransform.localScale = Vector3.zero;
                PopupScale(rectTransform, 0.3f, Ease.OutBack);
            }
            
            // 위치 및 크기 초기화
            rectTransform.localPosition = Vector3.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        
        /// <summary>
        /// UI 요소 닫기 애니메이션
        /// </summary>
        public void PlayCloseAnimation(GameObject uiObject, Animator animator = null, System.Action onComplete = null)
        {
            if (animator != null && animator.parameters.Length > 0)
            {
                // 애니메이터가 있는 경우 트리거 실행
                animator.SetTrigger("CloseUI");
                
                // 애니메이션 완료 후 콜백 실행
                float animationLength = GetAnimationLength(animator, "CloseUI");
                if (animationLength > 0)
                {
                    DelayedCall(animationLength, () => {
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    onComplete?.Invoke();
                }
            }
            else
            {
                // 애니메이터가 없는 경우 기본 애니메이션
                var rectTransform = uiObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    var tween = rectTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
                    if (onComplete != null)
                    {
                        tween.OnComplete(() => onComplete());
                    }
                }
                else
                {
                    onComplete?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// 애니메이션 길이 가져오기
        /// </summary>
        private float GetAnimationLength(Animator animator, string triggerName)
        {
            if (animator == null) return 0;
            
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                return clipInfo[0].clip.length;
            }
            
            return 0;
        }
        
        #endregion
        
        private void OnDestroy()
        {
            KillAllTweens();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseAllTweens();
            }
            else
            {
                ResumeAllTweens();
            }
        }
    }
}