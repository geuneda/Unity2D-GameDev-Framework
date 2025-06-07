using UnityEngine;

/// <summary>
/// 컴포넌트 안전 접근을 위한 확장 메서드 모음
/// </summary>
public static class SafeComponentAccess
{
    /// <summary>
    /// 안전하게 컴포넌트를 가져오며, 없으면 추가합니다.
    /// </summary>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }
    
    /// <summary>
    /// 안전하게 컴포넌트를 가져오며, 없으면 null을 반환합니다.
    /// </summary>
    public static T GetSafeComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
        {
            Debug.LogWarning($"GameObject is null when trying to GetSafeComponent<{typeof(T).Name}>");
            return null;
        }
        
        T component = gameObject.GetComponent<T>();
        return component;
    }
    
    /// <summary>
    /// 안전하게 컴포넌트를 가져오려고 시도합니다.
    /// </summary>
    public static bool TryGetComponent<T>(this GameObject gameObject, out T component) where T : Component
    {
        component = null;
        
        if (gameObject == null)
        {
            Debug.LogWarning($"GameObject is null when trying to TryGetComponent<{typeof(T).Name}>");
            return false;
        }
        
        component = gameObject.GetComponent<T>();
        if (component == null)
        {
            Debug.LogWarning($"{gameObject.name}에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 부모 오브젝트에서 컴포넌트를 안전하게 가져옵니다.
    /// </summary>
    public static T GetComponentInParentSafe<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
        {
            Debug.LogWarning($"GameObject is null when trying to GetComponentInParentSafe<{typeof(T).Name}>");
            return null;
        }
        
        T component = gameObject.GetComponentInParent<T>();
        return component;
    }
    
    /// <summary>
    /// 자식 오브젝트에서 컴포넌트를 안전하게 가져옵니다.
    /// </summary>
    public static T GetComponentInChildrenSafe<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
        {
            Debug.LogWarning($"GameObject is null when trying to GetComponentInChildrenSafe<{typeof(T).Name}>");
            return null;
        }
        
        T component = gameObject.GetComponentInChildren<T>();
        return component;
    }
    
    /// <summary>
    /// 부모 오브젝트에서 컴포넌트를 안전하게 가져옵니다.
    /// </summary>
    public static T GetComponentInParentSafe<T>(this Component component) where T : Component
    {
        if (component == null)
        {
            Debug.LogWarning($"Component is null when trying to GetComponentInParentSafe<{typeof(T).Name}>");
            return null;
        }
        
        T targetComponent = component.GetComponentInParent<T>();
        return targetComponent;
    }
    
    /// <summary>
    /// 자식 오브젝트에서 컴포넌트를 안전하게 가져옵니다.
    /// </summary>
    public static T GetComponentInChildrenSafe<T>(this Component component) where T : Component
    {
        if (component == null)
        {
            Debug.LogWarning($"Component is null when trying to GetComponentInChildrenSafe<{typeof(T).Name}>");
            return null;
        }
        
        T targetComponent = component.GetComponentInChildren<T>();
        return targetComponent;
    }
    
    /// <summary>
    /// 안전하게 컴포넌트를 제거합니다.
    /// </summary>
    public static void RemoveComponentIfExists<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null) return;
        
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            Object.Destroy(component);
        }
    }
    
    /// <summary>
    /// 모든 컴포넌트를 안전하게 제거합니다.
    /// </summary>
    public static void RemoveAllComponentsOfType<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null) return;
        
        T[] components = gameObject.GetComponents<T>();
        foreach (T component in components)
        {
            Object.Destroy(component);
        }
    }
    
    /// <summary>
    /// null이 아닌 경우에만 컴포넌트를 활성화/비활성화합니다.
    /// </summary>
    public static void SetEnabledSafe<T>(this GameObject gameObject, bool enabled) where T : Behaviour
    {
        if (gameObject == null) return;
        
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            component.enabled = enabled;
        }
    }
}