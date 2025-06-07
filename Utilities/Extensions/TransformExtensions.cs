using UnityEngine;

/// <summary>
/// Transform 컴포넌트를 위한 확장 메서드 모음
/// </summary>
public static class TransformExtensions
{
    /// <summary>
    /// 트랜스폼의 x 위치를 설정합니다.
    /// </summary>
    public static void SetPositionX(this Transform transform, float x)
    {
        Vector3 position = transform.position;
        position.x = x;
        transform.position = position;
    }
    
    /// <summary>
    /// 트랜스폼의 y 위치를 설정합니다.
    /// </summary>
    public static void SetPositionY(this Transform transform, float y)
    {
        Vector3 position = transform.position;
        position.y = y;
        transform.position = position;
    }
    
    /// <summary>
    /// 트랜스폼의 z 위치를 설정합니다.
    /// </summary>
    public static void SetPositionZ(this Transform transform, float z)
    {
        Vector3 position = transform.position;
        position.z = z;
        transform.position = position;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 x 위치를 설정합니다.
    /// </summary>
    public static void SetLocalPositionX(this Transform transform, float x)
    {
        Vector3 localPosition = transform.localPosition;
        localPosition.x = x;
        transform.localPosition = localPosition;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 y 위치를 설정합니다.
    /// </summary>
    public static void SetLocalPositionY(this Transform transform, float y)
    {
        Vector3 localPosition = transform.localPosition;
        localPosition.y = y;
        transform.localPosition = localPosition;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 z 위치를 설정합니다.
    /// </summary>
    public static void SetLocalPositionZ(this Transform transform, float z)
    {
        Vector3 localPosition = transform.localPosition;
        localPosition.z = z;
        transform.localPosition = localPosition;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 스케일 x를 설정합니다.
    /// </summary>
    public static void SetLocalScaleX(this Transform transform, float x)
    {
        Vector3 localScale = transform.localScale;
        localScale.x = x;
        transform.localScale = localScale;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 스케일 y를 설정합니다.
    /// </summary>
    public static void SetLocalScaleY(this Transform transform, float y)
    {
        Vector3 localScale = transform.localScale;
        localScale.y = y;
        transform.localScale = localScale;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 스케일 z를 설정합니다.
    /// </summary>
    public static void SetLocalScaleZ(this Transform transform, float z)
    {
        Vector3 localScale = transform.localScale;
        localScale.z = z;
        transform.localScale = localScale;
    }
    
    /// <summary>
    /// 트랜스폼의 모든 자식 오브젝트를 제거합니다.
    /// </summary>
    public static void DestroyChildren(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }
    
    /// <summary>
    /// 트랜스폼의 모든 자식 오브젝트를 즉시 제거합니다.
    /// </summary>
    public static void DestroyChildrenImmediate(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// 트랜스폼의 월드 위치를 리셋합니다.
    /// </summary>
    public static void ResetPosition(this Transform transform)
    {
        transform.position = Vector3.zero;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 위치를 리셋합니다.
    /// </summary>
    public static void ResetLocalPosition(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 회전을 리셋합니다.
    /// </summary>
    public static void ResetLocalRotation(this Transform transform)
    {
        transform.localRotation = Quaternion.identity;
    }
    
    /// <summary>
    /// 트랜스폼의 로컬 스케일을 리셋합니다.
    /// </summary>
    public static void ResetLocalScale(this Transform transform)
    {
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// 트랜스폼의 모든 로컬 변환을 리셋합니다.
    /// </summary>
    public static void ResetLocal(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// 다른 트랜스폼을 향해 회전합니다. (2D용)
    /// </summary>
    public static void LookAt2D(this Transform transform, Transform target)
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    /// <summary>
    /// 특정 방향을 향해 회전합니다. (2D용)
    /// </summary>
    public static void LookAt2D(this Transform transform, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    /// <summary>
    /// 다른 트랜스폼을 향해 부드럽게 회전합니다. (2D용)
    /// </summary>
    public static void SmoothLookAt2D(this Transform transform, Transform target, float speed)
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
    }
    
    /// <summary>
    /// 모든 자식을 트랜스폼에 복사합니다.
    /// </summary>
    public static void CopyChildrenFrom(this Transform transform, Transform source)
    {
        // 기존 자식 제거
        transform.DestroyChildren();
        
        // 소스 자식 복제
        foreach (Transform child in source)
        {
            Transform copy = Object.Instantiate(child, transform);
            copy.localPosition = child.localPosition;
            copy.localRotation = child.localRotation;
            copy.localScale = child.localScale;
            copy.name = child.name;
        }
    }
}