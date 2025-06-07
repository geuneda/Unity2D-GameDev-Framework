using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

/// <summary>
/// DOTween을 위한 확장 메서드 모음
/// 트위닝 애니메이션을 쉽게 사용할 수 있도록 도와줍니다.
/// </summary>
public static class TweenExtensions
{
    /// <summary>
    /// 트윈의 완료를 비동기적으로 기다립니다.
    /// </summary>
    public static async Task AsyncWaitForCompletion(this Tween tween)
    {
        if (!tween.IsActive() || tween.IsComplete())
        {
            return;
        }
        
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        tween.OnComplete(() => tcs.SetResult(true));
        
        await tcs.Task;
    }
    
    /// <summary>
    /// Transform을 부드럽게 이동시킵니다.
    /// </summary>
    public static Tween MoveSmooth(this Transform transform, Vector3 targetPosition, float duration = 0.5f, Ease ease = Ease.OutQuad)
    {
        return transform.DOMove(targetPosition, duration).SetEase(ease);
    }
    
    /// <summary>
    /// Transform을 부드럽게 회전시킵니다.
    /// </summary>
    public static Tween RotateSmooth(this Transform transform, Vector3 targetRotation, float duration = 0.5f, Ease ease = Ease.OutQuad)
    {
        return transform.DORotate(targetRotation, duration).SetEase(ease);
    }
    
    /// <summary>
    /// Transform을 부드럽게 크기 조절합니다.
    /// </summary>
    public static Tween ScaleSmooth(this Transform transform, Vector3 targetScale, float duration = 0.5f, Ease ease = Ease.OutQuad)
    {
        return transform.DOScale(targetScale, duration).SetEase(ease);
    }
    
    /// <summary>
    /// 오브젝트를 점프시킵니다.
    /// </summary>
    public static Tween Jump(this Transform transform, Vector3 targetPosition, float jumpPower = 2f, int numJumps = 1, float duration = 1f, Ease ease = Ease.OutQuad)
    {
        return transform.DOJump(targetPosition, jumpPower, numJumps, duration).SetEase(ease);
    }
    
    /// <summary>
    /// 오브젝트를 탄력적으로 튕기는 효과를 줍니다.
    /// </summary>
    public static Tween PunchScale(this Transform transform, float strength = 0.2f, float duration = 0.5f, int vibrato = 10, float elasticity = 1f)
    {
        return transform.DOPunchScale(Vector3.one * strength, duration, vibrato, elasticity);
    }
    
    /// <summary>
    /// UI 요소가 튀어오르는 효과를 줍니다.
    /// </summary>
    public static Sequence BounceIn(this RectTransform rectTransform, float duration = 0.5f)
    {
        Vector3 originalScale = rectTransform.localScale;
        rectTransform.localScale = Vector3.zero;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOScale(originalScale * 1.1f, duration * 0.6f).SetEase(Ease.OutQuad));
        sequence.Append(rectTransform.DOScale(originalScale, duration * 0.4f).SetEase(Ease.InOutQuad));
        
        return sequence;
    }
    
    /// <summary>
    /// UI 요소가 사라지는 효과를 줍니다.
    /// </summary>
    public static Sequence BounceOut(this RectTransform rectTransform, float duration = 0.5f)
    {
        Vector3 originalScale = rectTransform.localScale;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOScale(originalScale * 1.1f, duration * 0.4f).SetEase(Ease.OutQuad));
        sequence.Append(rectTransform.DOScale(Vector3.zero, duration * 0.6f).SetEase(Ease.InQuad));
        
        return sequence;
    }
    
    /// <summary>
    /// 오브젝트가 흔들리는 효과를 줍니다.
    /// </summary>
    public static Tween Shake(this Transform transform, float duration = 0.5f, float strength = 1f, int vibrato = 10, float randomness = 90f)
    {
        return transform.DOShakePosition(duration, strength, vibrato, randomness);
    }
    
    /// <summary>
    /// 오브젝트가 페이드인하는 효과를 줍니다.
    /// </summary>
    public static Tween FadeIn(this CanvasGroup canvasGroup, float duration = 0.3f, Ease ease = Ease.OutQuad)
    {
        canvasGroup.alpha = 0f;
        return canvasGroup.DOFade(1f, duration).SetEase(ease);
    }
    
    /// <summary>
    /// 오브젝트가 페이드아웃하는 효과를 줍니다.
    /// </summary>
    public static Tween FadeOut(this CanvasGroup canvasGroup, float duration = 0.3f, Ease ease = Ease.InQuad)
    {
        return canvasGroup.DOFade(0f, duration).SetEase(ease);
    }
    
    /// <summary>
    /// 한 색상에서 다른 색상으로 변화하는 효과를 줍니다.
    /// </summary>
    public static Tween ColorTransition(this UnityEngine.UI.Graphic graphic, Color targetColor, float duration = 0.3f, Ease ease = Ease.InOutQuad)
    {
        return graphic.DOColor(targetColor, duration).SetEase(ease);
    }
    
    /// <summary>
    /// 애니메이션이 완료될 때까지 비동기적으로 기다립니다.
    /// </summary>
    public static async Task WaitForAnimationAsync(this GameObject gameObject, float duration)
    {
        await Task.Delay(Mathf.RoundToInt(duration * 1000));
    }
    
    /// <summary>
    /// 텍스트가 한 글자씩 나타나는 효과를 줍니다.
    /// </summary>
    public static Tween TypeWriter(this TMPro.TextMeshProUGUI text, string fullText, float duration = 1.0f)
    {
        text.text = "";
        return DOTween.To(() => 0, x => {
            int visibleCharacters = Mathf.FloorToInt(x);
            if (visibleCharacters <= fullText.Length)
            {
                text.text = fullText.Substring(0, visibleCharacters);
            }
        }, fullText.Length, duration);
    }
    
    /// <summary>
    /// 진동 효과를 생성합니다.
    /// </summary>
    public static Sequence Vibrate(this Transform transform, float duration = 0.3f, float strength = 5f)
    {
        Vector3 originalPosition = transform.localPosition;
        
        Sequence sequence = DOTween.Sequence();
        
        for (int i = 0; i < Mathf.FloorToInt(duration * 10); i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * strength;
            sequence.Append(transform.DOLocalMove(originalPosition + randomOffset, 0.03f));
        }
        
        sequence.Append(transform.DOLocalMove(originalPosition, 0.05f));
        
        return sequence;
    }
}