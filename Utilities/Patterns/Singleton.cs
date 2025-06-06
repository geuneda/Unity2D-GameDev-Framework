using UnityEngine;

namespace Unity2DFramework.Utilities.Patterns
{
    /// <summary>
    /// 제네릭 싱글톤 패턴 베이스 클래스
    /// MonoBehaviour를 상속받는 클래스에서 사용 가능
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object lockObject = new object();
        private static bool applicationIsQuitting = false;
        
        /// <summary>
        /// 싱글톤 인스턴스에 접근하는 프로퍼티
        /// </summary>
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] '{typeof(T)}' 인스턴스가 이미 파괴되었습니다. " +
                                   "애플리케이션 종료 중에는 접근할 수 없습니다.");
                    return null;
                }
                
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        // 씬에서 기존 인스턴스 찾기
                        instance = FindObjectOfType<T>();
                        
                        if (instance == null)
                        {
                            // 새로운 게임오브젝트 생성
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name} (Singleton)";
                            
                            // 씬 전환 시에도 유지
                            DontDestroyOnLoad(singletonObject);
                            
                            Debug.Log($"[Singleton] '{typeof(T)}' 인스턴스가 자동으로 생성되었습니다.");
                        }
                    }
                    
                    return instance;
                }
            }
        }
        
        /// <summary>
        /// 싱글톤이 초기화되었는지 확인
        /// </summary>
        public static bool IsInitialized => instance != null;
        
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] '{typeof(T)}' 인스턴스가 이미 존재합니다. 중복 인스턴스를 제거합니다.");
                Destroy(gameObject);
            }
        }
        
        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
        
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
    
    /// <summary>
    /// 영구 싱글톤 패턴 (씬 전환 시에도 유지)
    /// </summary>
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                base.Awake();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
    
    /// <summary>
    /// 일반 C# 클래스용 싱글톤 패턴
    /// </summary>
    public abstract class SingletonClass<T> where T : class, new()
    {
        private static T instance;
        private static readonly object lockObject = new object();
        
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }
        
        protected SingletonClass() { }
    }
}