using UnityEngine;
using System.Collections.Generic;

namespace Unity2DFramework.Utilities.Extensions
{
    /// <summary>
    /// Unity 오브젝트들을 위한 확장 메서드 모음
    /// 자주 사용되는 기능들을 간편하게 사용할 수 있도록 제공
    /// </summary>
    public static class UnityExtensions
    {
        #region GameObject Extensions
        
        /// <summary>
        /// 안전한 컴포넌트 가져오기 (없으면 추가)
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
        /// 안전한 컴포넌트 접근 (로그 포함)
        /// </summary>
        public static bool TryGetComponentSafe<T>(this GameObject gameObject, out T component) where T : Component
        {
            component = gameObject.GetComponent<T>();
            if (component == null)
            {
                Debug.LogWarning($"[SafeAccess] {gameObject.name}에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 자식 오브젝트에서 컴포넌트 찾기 (재귀적)
        /// </summary>
        public static T FindComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false) where T : Component
        {
            T component = gameObject.GetComponentInChildren<T>(includeInactive);
            if (component == null)
            {
                Debug.LogWarning($"[FindComponent] {gameObject.name}의 자식에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            }
            return component;
        }
        
        /// <summary>
        /// 레이어 설정 (자식 포함)
        /// </summary>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
        
        /// <summary>
        /// 태그로 자식 찾기
        /// </summary>
        public static GameObject FindChildWithTag(this GameObject gameObject, string tag)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.CompareTag(tag))
                {
                    return child.gameObject;
                }
                
                GameObject found = child.gameObject.FindChildWithTag(tag);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }
        
        #endregion
        
        #region Transform Extensions
        
        /// <summary>
        /// 모든 자식 제거
        /// </summary>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(transform.GetChild(i).gameObject);
                }
                else
                {
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }
        
        /// <summary>
        /// 위치 리셋
        /// </summary>
        public static void ResetTransform(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// 로컬 위치만 설정
        /// </summary>
        public static void SetLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            Vector3 localPos = transform.localPosition;
            if (x.HasValue) localPos.x = x.Value;
            if (y.HasValue) localPos.y = y.Value;
            if (z.HasValue) localPos.z = z.Value;
            transform.localPosition = localPos;
        }
        
        /// <summary>
        /// 월드 위치만 설정
        /// </summary>
        public static void SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            Vector3 pos = transform.position;
            if (x.HasValue) pos.x = x.Value;
            if (y.HasValue) pos.y = y.Value;
            if (z.HasValue) pos.z = z.Value;
            transform.position = pos;
        }
        
        /// <summary>
        /// 이름으로 자식 찾기 (재귀적)
        /// </summary>
        public static Transform FindDeepChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                
                Transform result = child.FindDeepChild(name);
                if (result != null)
                    return result;
            }
            return null;
        }
        
        #endregion
        
        #region Vector Extensions
        
        /// <summary>
        /// Vector2를 Vector3로 변환 (Z값 지정 가능)
        /// </summary>
        public static Vector3 ToVector3(this Vector2 vector, float z = 0f)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        /// <summary>
        /// Vector3를 Vector2로 변환
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        
        /// <summary>
        /// 특정 축만 설정
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }
        
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }
        
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        public static Vector2 WithX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }
        
        public static Vector2 WithY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, y);
        }
        
        #endregion
        
        #region Color Extensions
        
        /// <summary>
        /// 알파값만 변경
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
        /// <summary>
        /// RGB 값만 변경
        /// </summary>
        public static Color WithRGB(this Color color, float r, float g, float b)
        {
            return new Color(r, g, b, color.a);
        }
        
        #endregion
        
        #region Collection Extensions
        
        /// <summary>
        /// 리스트에서 랜덤 요소 가져오기
        /// </summary>
        public static T GetRandomElement<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            
            return list[Random.Range(0, list.Count)];
        }
        
        /// <summary>
        /// 배열에서 랜덤 요소 가져오기
        /// </summary>
        public static T GetRandomElement<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                return default(T);
            
            return array[Random.Range(0, array.Length)];
        }
        
        /// <summary>
        /// 리스트 셔플
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
        
        #endregion
    }
}