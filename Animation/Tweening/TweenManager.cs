using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

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
        public Tween FadeIn(CanvasGroup canvasGroup, float duration = -1f, Ease ease = Ease.Unset)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);
            
            return canvasGroup.DOFade(1f, duration).SetEase(ease);
        }
        
        /// <summary>
        /// UI 요소 페이드 아웃
        /// </summary>
        public Tween FadeOut(CanvasGroup canvasGroup, float duration = -1f, Ease ease = Ease.Unset, bool deactivateOnComplete = true)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            Tween tween = canvasGroup.DOFade(0f, duration).SetEase(ease);
            
            if (deactivateOnComplete)
            {
                tween.OnComplete(() => canvasGroup.gameObject.SetActive(false));
            }
            
            return tween;
        }
        
        /// <summary>
        /// UI 요소 스케일 애니메이션 (팝업 효과)
        /// </summary>
        public Tween PopupScale(Transform transform, float duration = -1f, Ease ease = Ease.Unset)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = Ease.OutBack;
            
            transform.localScale = Vector3.zero;
            return transform.DOScale(Vector3.one, duration).SetEase(ease);
        }
        
        /// <summary>
        /// UI 요소 슬라이드 인 (위에서 아래로)
        /// </summary>
        public Tween SlideInFromTop(RectTransform rectTransform, float duration = -1f, Ease ease = Ease.Unset)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            Vector2 originalPos = rectTransform.anchoredPosition;
            Vector2 startPos = originalPos + Vector2.up * Screen.height;
            
            rectTransform.anchoredPosition = startPos;
            return rectTransform.DOAnchorPos(originalPos, duration).SetEase(ease);
        }
        
        /// <summary>
        /// UI 요소 슬라이드 아웃 (아래에서 위로)
        /// </summary>
        public Tween SlideOutToTop(RectTransform rectTransform, float duration = -1f, Ease ease = Ease.Unset)
        {
            if (duration < 0) duration = defaultDuration;
            if (ease == Ease.Unset) ease = defaultEase;
            
            Vector2 targetPos = rectTransform.anchoredPosition + Vector2.up * Screen.height;
            return rectTransform.DOAnchorPos(targetPos, duration).SetEase(ease);
        }
        
        #endregion
        
        #region 게임플레이 트위닝 메서드들
        
        /// <summary>
        /// 오브젝트 바운스 효과
        /// </summary>
        public Tween Bounce(Transform transform, float bounceHeight = 1f, float duration = -1f)
        {
            if (duration < 0) duration = defaultDuration;
            
            Vector3 originalPos = transform.position;
            Vector3 bouncePos = originalPos + Vector3.up * bounceHeight;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(bouncePos, duration * 0.5f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOMove(originalPos, duration * 0.5f).SetEase(Ease.InQuad));
            
            return sequence;
        }
        
        /// <summary>
        /// 오브젝트 흔들기 효과
        /// </summary>
        public Tween Shake(Transform transform, float strength = 1f, float duration = -1f, int vibrato = 10)
        {
            if (duration < 0) duration = defaultDuration;
            
            return transform.DOShakePosition(duration, strength, vibrato);
        }
        
        /// <summary>
        /// 오브젝트 펄스 효과 (크기 변화)
        /// </summary>
        public Tween Pulse(Transform transform, float scaleMultiplier = 1.2f, float duration = -1f)
        {
            if (duration < 0) duration = defaultDuration;
            
            Vector3 originalScale = transform.localScale;
            Vector3 pulseScale = originalScale * scaleMultiplier;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(pulseScale, duration * 0.5f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(originalScale, duration * 0.5f).SetEase(Ease.InQuad));
            sequence.SetLoops(-1, LoopType.Restart);
            
            return sequence;
        }
        
        /// <summary>
        /// 오브젝트 회전 애니메이션
        /// </summary>
        public Tween RotateLoop(Transform transform, Vector3 rotation, float duration = 1f)
        {
            return transform.DORotate(rotation, duration, RotateMode.FastBeyond360)
                          .SetLoops(-1, LoopType.Incremental)
                          .SetEase(Ease.Linear);
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
        /// 이름으로 트윈 중지
        /// </summary>
        public void KillTween(string name)
        {
            if (activeTweens.ContainsKey(name))
            {
                activeTweens[name].Kill();
                activeTweens.Remove(name);
            }
        }
        
        /// <summary>
        /// 모든 트윈 중지
        /// </summary>
        public void KillAllTweens()
        {
            DOTween.KillAll();
            activeTweens.Clear();
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
        public Tween DelayedCall(float delay, System.Action callback)
        {
            return DOVirtual.DelayedCall(delay, callback);
        }
        
        /// <summary>
        /// 값 보간
        /// </summary>
        public Tween InterpolateValue(float from, float to, float duration, System.Action<float> onUpdate)
        {
            return DOVirtual.Float(from, to, duration, onUpdate);
        }
        
        /// <summary>
        /// 색상 보간
        /// </summary>
        public Tween InterpolateColor(Color from, Color to, float duration, System.Action<Color> onUpdate)
        {
            return DOVirtual.Color(from, to, duration, onUpdate);
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