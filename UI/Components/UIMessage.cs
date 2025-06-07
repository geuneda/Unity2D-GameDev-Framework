using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;

/// <summary>
/// 메시지 UI 컴포넌트
/// 화면에 알림 메시지를 표시합니다.
/// </summary>
public class UIMessage : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _background;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _messageText;
    
    [Header("Icons")]
    [SerializeField] private Sprite _infoIcon;
    [SerializeField] private Sprite _successIcon;
    [SerializeField] private Sprite _warningIcon;
    [SerializeField] private Sprite _errorIcon;
    
    [Header("Colors")]
    [SerializeField] private Color _infoColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color _successColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _warningColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color _errorColor = new Color(1f, 0.3f, 0.3f);
    
    [Header("Animation")]
    [SerializeField] private float _showDuration = 0.3f;
    [SerializeField] private float _hideDuration = 0.3f;
    
    private RectTransform _rectTransform;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // 초기 상태 설정
        _canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// 메시지를 설정합니다.
    /// </summary>
    public void SetMessage(string message, MessageType type = MessageType.Info, string title = "")
    {
        // 메시지 텍스트 설정
        _messageText.text = message;
        
        // 제목 설정 (비어있으면 기본값 사용)
        if (string.IsNullOrEmpty(title))
        {
            switch (type)
            {
                case MessageType.Info:
                    title = "알림";
                    break;
                case MessageType.Success:
                    title = "성공";
                    break;
                case MessageType.Warning:
                    title = "주의";
                    break;
                case MessageType.Error:
                    title = "오류";
                    break;
            }
        }
        
        _titleText.text = title;
        
        // 아이콘과 색상 설정
        switch (type)
        {
            case MessageType.Info:
                _iconImage.sprite = _infoIcon;
                _background.color = _infoColor;
                break;
                
            case MessageType.Success:
                _iconImage.sprite = _successIcon;
                _background.color = _successColor;
                break;
                
            case MessageType.Warning:
                _iconImage.sprite = _warningIcon;
                _background.color = _warningColor;
                break;
                
            case MessageType.Error:
                _iconImage.sprite = _errorIcon;
                _background.color = _errorColor;
                break;
        }
    }
    
    /// <summary>
    /// 메시지를 표시합니다.
    /// </summary>
    public async void Show()
    {
        // 애니메이션 설정
        _canvasGroup.alpha = 0f;
        _rectTransform.anchoredPosition = new Vector2(0, -100);
        
        // 페이드인 애니메이션
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_canvasGroup.DOFade(1f, _showDuration).SetEase(Ease.OutQuad));
        sequence.Join(_rectTransform.DOAnchorPosY(0, _showDuration).SetEase(Ease.OutBack));
        
        await sequence.AsyncWaitForCompletion();
    }
    
    /// <summary>
    /// 메시지를 숨깁니다.
    /// </summary>
    public async void Hide()
    {
        // 페이드아웃 애니메이션
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_canvasGroup.DOFade(0f, _hideDuration).SetEase(Ease.InQuad));
        sequence.Join(_rectTransform.DOAnchorPosY(100, _hideDuration).SetEase(Ease.InBack));
        
        await sequence.AsyncWaitForCompletion();
    }
    
    private void OnDestroy()
    {
        // 트윈 정리
        DOTween.Kill(_canvasGroup);
        DOTween.Kill(_rectTransform);
    }
}