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
                   .SetCapacity(maxTweenersCapacity, maxSequencesCapacity);\n            \n            Debug.Log(\"[TweenManager] DOTween 초기화 완료\");\n        }\n        \n        #region UI 트위닝 메서드들\n        \n        /// <summary>\n        /// UI 요소 페이드 인\n        /// </summary>\n        public Tween FadeIn(CanvasGroup canvasGroup, float duration = -1f, Ease ease = Ease.Unset)\n        {\n            if (duration < 0) duration = defaultDuration;\n            if (ease == Ease.Unset) ease = defaultEase;\n            \n            canvasGroup.alpha = 0f;\n            canvasGroup.gameObject.SetActive(true);\n            \n            return canvasGroup.DOFade(1f, duration).SetEase(ease);\n        }\n        \n        /// <summary>\n        /// UI 요소 페이드 아웃\n        /// </summary>\n        public Tween FadeOut(CanvasGroup canvasGroup, float duration = -1f, Ease ease = Ease.Unset, bool deactivateOnComplete = true)\n        {\n            if (duration < 0) duration = defaultDuration;\n            if (ease == Ease.Unset) ease = defaultEase;\n            \n            Tween tween = canvasGroup.DOFade(0f, duration).SetEase(ease);\n            \n            if (deactivateOnComplete)\n            {\n                tween.OnComplete(() => canvasGroup.gameObject.SetActive(false));\n            }\n            \n            return tween;\n        }\n        \n        /// <summary>\n        /// UI 요소 스케일 애니메이션 (팝업 효과)\n        /// </summary>\n        public Tween PopupScale(Transform transform, float duration = -1f, Ease ease = Ease.Unset)\n        {\n            if (duration < 0) duration = defaultDuration;\n            if (ease == Ease.Unset) ease = Ease.OutBack;\n            \n            transform.localScale = Vector3.zero;\n            return transform.DOScale(Vector3.one, duration).SetEase(ease);\n        }\n        \n        /// <summary>\n        /// UI 요소 슬라이드 인 (위에서 아래로)\n        /// </summary>\n        public Tween SlideInFromTop(RectTransform rectTransform, float duration = -1f, Ease ease = Ease.Unset)\n        {\n            if (duration < 0) duration = defaultDuration;\n            if (ease == Ease.Unset) ease = defaultEase;\n            \n            Vector2 originalPos = rectTransform.anchoredPosition;\n            Vector2 startPos = originalPos + Vector2.up * Screen.height;\n            \n            rectTransform.anchoredPosition = startPos;\n            return rectTransform.DOAnchorPos(originalPos, duration).SetEase(ease);\n        }\n        \n        /// <summary>\n        /// UI 요소 슬라이드 아웃 (아래에서 위로)\n        /// </summary>\n        public Tween SlideOutToTop(RectTransform rectTransform, float duration = -1f, Ease ease = Ease.Unset)\n        {\n            if (duration < 0) duration = defaultDuration;\n            if (ease == Ease.Unset) ease = defaultEase;\n            \n            Vector2 targetPos = rectTransform.anchoredPosition + Vector2.up * Screen.height;\n            return rectTransform.DOAnchorPos(targetPos, duration).SetEase(ease);\n        }\n        \n        #endregion\n        \n        #region 게임플레이 트위닝 메서드들\n        \n        /// <summary>\n        /// 오브젝트 바운스 효과\n        /// </summary>\n        public Tween Bounce(Transform transform, float bounceHeight = 1f, float duration = -1f)\n        {\n            if (duration < 0) duration = defaultDuration;\n            \n            Vector3 originalPos = transform.position;\n            Vector3 bouncePos = originalPos + Vector3.up * bounceHeight;\n            \n            Sequence sequence = DOTween.Sequence();\n            sequence.Append(transform.DOMove(bouncePos, duration * 0.5f).SetEase(Ease.OutQuad));\n            sequence.Append(transform.DOMove(originalPos, duration * 0.5f).SetEase(Ease.InQuad));\n            \n            return sequence;\n        }\n        \n        /// <summary>\n        /// 오브젝트 흔들기 효과\n        /// </summary>\n        public Tween Shake(Transform transform, float strength = 1f, float duration = -1f, int vibrato = 10)\n        {\n            if (duration < 0) duration = defaultDuration;\n            \n            return transform.DOShakePosition(duration, strength, vibrato);\n        }\n        \n        /// <summary>\n        /// 오브젝트 펄스 효과 (크기 변화)\n        /// </summary>\n        public Tween Pulse(Transform transform, float scaleMultiplier = 1.2f, float duration = -1f)\n        {\n            if (duration < 0) duration = defaultDuration;\n            \n            Vector3 originalScale = transform.localScale;\n            Vector3 pulseScale = originalScale * scaleMultiplier;\n            \n            Sequence sequence = DOTween.Sequence();\n            sequence.Append(transform.DOScale(pulseScale, duration * 0.5f).SetEase(Ease.OutQuad));\n            sequence.Append(transform.DOScale(originalScale, duration * 0.5f).SetEase(Ease.InQuad));\n            sequence.SetLoops(-1, LoopType.Restart);\n            \n            return sequence;\n        }\n        \n        /// <summary>\n        /// 오브젝트 회전 애니메이션\n        /// </summary>\n        public Tween RotateLoop(Transform transform, Vector3 rotation, float duration = 1f)\n        {\n            return transform.DORotate(rotation, duration, RotateMode.FastBeyond360)\n                          .SetLoops(-1, LoopType.Incremental)\n                          .SetEase(Ease.Linear);\n        }\n        \n        #endregion\n        \n        #region 트윈 관리 메서드들\n        \n        /// <summary>\n        /// 이름을 가진 트윈 등록\n        /// </summary>\n        public void RegisterTween(string name, Tween tween)\n        {\n            if (activeTweens.ContainsKey(name))\n            {\n                activeTweens[name].Kill();\n            }\n            \n            activeTweens[name] = tween;\n            \n            // 트윈 완료 시 딕셔너리에서 제거\n            tween.OnComplete(() => activeTweens.Remove(name));\n        }\n        \n        /// <summary>\n        /// 이름으로 트윈 중지\n        /// </summary>\n        public void KillTween(string name)\n        {\n            if (activeTweens.ContainsKey(name))\n            {\n                activeTweens[name].Kill();\n                activeTweens.Remove(name);\n            }\n        }\n        \n        /// <summary>\n        /// 모든 트윈 중지\n        /// </summary>\n        public void KillAllTweens()\n        {\n            DOTween.KillAll();\n            activeTweens.Clear();\n        }\n        \n        /// <summary>\n        /// 특정 타겟의 모든 트윈 중지\n        /// </summary>\n        public void KillTweensOfTarget(object target)\n        {\n            DOTween.Kill(target);\n        }\n        \n        /// <summary>\n        /// 트윈 일시정지/재개\n        /// </summary>\n        public void PauseAllTweens()\n        {\n            DOTween.PauseAll();\n        }\n        \n        public void ResumeAllTweens()\n        {\n            DOTween.PlayAll();\n        }\n        \n        #endregion\n        \n        #region 유틸리티 메서드들\n        \n        /// <summary>\n        /// 지연 실행\n        /// </summary>\n        public Tween DelayedCall(float delay, System.Action callback)\n        {\n            return DOVirtual.DelayedCall(delay, callback);\n        }\n        \n        /// <summary>\n        /// 값 보간\n        /// </summary>\n        public Tween InterpolateValue(float from, float to, float duration, System.Action<float> onUpdate)\n        {\n            return DOVirtual.Float(from, to, duration, onUpdate);\n        }\n        \n        /// <summary>\n        /// 색상 보간\n        /// </summary>\n        public Tween InterpolateColor(Color from, Color to, float duration, System.Action<Color> onUpdate)\n        {\n            return DOVirtual.Color(from, to, duration, onUpdate);\n        }\n        \n        #endregion\n        \n        private void OnDestroy()\n        {\n            KillAllTweens();\n        }\n        \n        private void OnApplicationPause(bool pauseStatus)\n        {\n            if (pauseStatus)\n            {\n                PauseAllTweens();\n            }\n            else\n            {\n                ResumeAllTweens();\n            }\n        }\n    }\n}"