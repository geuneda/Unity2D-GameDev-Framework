using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI 패널의 기본 클래스
/// 모든 UI 패널은 이 클래스를 상속받아야 합니다.
/// </summary>
public abstract class UIPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] protected bool _canFade = true;
    [SerializeField] protected bool _blockRaycasts = true;
    [SerializeField] protected CanvasGroup _canvasGroup;
    [SerializeField] protected GameObject _background;
    
    // 패널 ID
    public string PanelId { get; set; }
    
    // 페이드 기능 제공 여부
    public bool CanFade => _canFade && _canvasGroup != null;
    
    // 레이캐스트 블로킹 여부
    public bool BlockRaycasts
    {
        get => _blockRaycasts;
        set
        {
            _blockRaycasts = value;
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = value;
            }
        }
    }
    
    protected virtual void Awake()
    {
        // CanvasGroup이 없으면 추가
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null && _canFade)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // 초기 설정
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1.0f;
            _canvasGroup.blocksRaycasts = _blockRaycasts;
        }
    }
    
    protected virtual void OnEnable()
    {
        // 패널이 활성화될 때 호출됨
    }
    
    protected virtual void OnDisable()
    {
        // 패널이 비활성화될 때 호출됨
    }
    
    /// <summary>
    /// 패널이 표시되었을 때 호출됩니다.
    /// </summary>
    public virtual void OnPanelShown()
    {
        // 패널이 표시될 때 수행할 작업 구현
    }
    
    /// <summary>
    /// 패널이 숨겨졌을 때 호출됩니다.
    /// </summary>
    public virtual void OnPanelHidden()
    {
        // 패널이 숨겨질 때 수행할 작업 구현
    }
    
    /// <summary>
    /// 패널을 페이드인합니다.
    /// </summary>
    public virtual async Task FadeIn(float duration = 0.3f)
    {
        if (!CanFade) return;
        
        // 애니메이션 초기 설정
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        
        // 배경이 있으면 배경도 페이드인
        if (_background != null)
        {
            CanvasGroup bgCanvasGroup = _background.GetComponent<CanvasGroup>();
            if (bgCanvasGroup != null)
            {
                bgCanvasGroup.alpha = 0f;
                bgCanvasGroup.DOFade(1f, duration);
            }
        }
        
        // 페이드인 애니메이션
        await _canvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        
        _canvasGroup.blocksRaycasts = _blockRaycasts;
        OnPanelShown();
    }
    
    /// <summary>
    /// 패널을 페이드아웃합니다.
    /// </summary>
    public virtual async Task FadeOut(float duration = 0.3f)
    {
        if (!CanFade) return;
        
        // 레이캐스트 차단 해제
        _canvasGroup.blocksRaycasts = false;
        
        // 배경이 있으면 배경도 페이드아웃
        if (_background != null)
        {
            CanvasGroup bgCanvasGroup = _background.GetComponent<CanvasGroup>();
            if (bgCanvasGroup != null)
            {
                bgCanvasGroup.DOFade(0f, duration);
            }
        }
        
        // 페이드아웃 애니메이션
        await _canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad).AsyncWaitForCompletion();
    }
    
    /// <summary>
    /// 패널을 닫습니다.
    /// </summary>
    public virtual void ClosePanel()
    {
        UIManager.Instance.ClosePanel(this);
    }
    
    /// <summary>
    /// 패널에 애니메이션을 적용합니다.
    /// </summary>
    public virtual async Task PlayAnimation(PanelAnimation animationType, float duration = 0.3f)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        Vector2 originalPosition = rectTransform.anchoredPosition;
        Vector2 originalScale = rectTransform.localScale;
        
        switch (animationType)
        {
            case PanelAnimation.SlideFromRight:
                rectTransform.anchoredPosition = new Vector2(Screen.width, originalPosition.y);
                await rectTransform.DOAnchorPos(originalPosition, duration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.SlideFromLeft:
                rectTransform.anchoredPosition = new Vector2(-Screen.width, originalPosition.y);
                await rectTransform.DOAnchorPos(originalPosition, duration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.SlideFromTop:
                rectTransform.anchoredPosition = new Vector2(originalPosition.x, Screen.height);
                await rectTransform.DOAnchorPos(originalPosition, duration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.SlideFromBottom:
                rectTransform.anchoredPosition = new Vector2(originalPosition.x, -Screen.height);
                await rectTransform.DOAnchorPos(originalPosition, duration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.ScaleUp:
                rectTransform.localScale = Vector3.zero;
                await rectTransform.DOScale(originalScale, duration).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.ScaleDown:
                rectTransform.localScale = originalScale * 1.5f;
                await rectTransform.DOScale(originalScale, duration).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                break;
        }
    }
    
    /// <summary>
    /// 패널에서 닫기 애니메이션을 실행합니다.
    /// </summary>
    public virtual async Task PlayCloseAnimation(PanelAnimation animationType, float duration = 0.3f)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        Vector2 originalPosition = rectTransform.anchoredPosition;
        Vector2 originalScale = rectTransform.localScale;
        
        _canvasGroup.blocksRaycasts = false;
        
        switch (animationType)
        {
            case PanelAnimation.SlideFromRight:
                await rectTransform.DOAnchorPos(new Vector2(Screen.width, originalPosition.y), duration).SetEase(Ease.InQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.SlideFromLeft:
                await rectTransform.DOAnchorPos(new Vector2(-Screen.width, originalPosition.y), duration).SetEase(Ease.InQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.SlideFromTop:
                await rectTransform.DOAnchorPos(new Vector2(originalPosition.x, Screen.height), duration).SetEase(Ease.InQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.SlideFromBottom:
                await rectTransform.DOAnchorPos(new Vector2(originalPosition.x, -Screen.height), duration).SetEase(Ease.InQuad).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.ScaleUp:
                await rectTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).AsyncWaitForCompletion();
                break;
                
            case PanelAnimation.ScaleDown:
                await rectTransform.DOScale(originalScale * 1.5f, duration).SetEase(Ease.InBack).AsyncWaitForCompletion();
                break;
        }
    }
    
    /// <summary>
    /// UI 요소를 일시적으로 강조 표시합니다.
    /// </summary>
    protected async Task HighlightElement(RectTransform element, float duration = 0.3f)
    {
        if (element == null) return;
        
        Vector3 originalScale = element.localScale;
        Color originalColor = Color.white;
        
        Image image = element.GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
            
            // 밝게 빛나는 효과
            await DOTween.Sequence()
                .Append(image.DOColor(Color.white, duration * 0.5f))
                .Append(image.DOColor(originalColor, duration * 0.5f))
                .SetLoops(2)
                .AsyncWaitForCompletion();
        }
        else
        {
            // 크기 변화 효과
            await DOTween.Sequence()
                .Append(element.DOScale(originalScale * 1.2f, duration * 0.5f))
                .Append(element.DOScale(originalScale, duration * 0.5f))
                .SetLoops(2)
                .AsyncWaitForCompletion();
        }
    }
    
    /// <summary>
    /// 패널을 초기화합니다.
    /// </summary>
    public virtual void ResetPanel()
    {
        // 패널을 초기 상태로 재설정하는 로직 구현
    }
    
    protected virtual void OnDestroy()
    {
        // 메모리 누수 방지를 위한 정리 작업
        DOTween.Kill(transform);
        DOTween.Kill(_canvasGroup);
    }
}

/// <summary>
/// 패널 애니메이션 타입
/// </summary>
public enum PanelAnimation
{
    None,
    SlideFromRight,
    SlideFromLeft,
    SlideFromTop,
    SlideFromBottom,
    ScaleUp,
    ScaleDown
}