using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity2DFramework.Core.Managers
{
    public delegate void EventListener(object args);
    public delegate void EventListener<T>(T args) where T : class;

    /// <summary>
    /// 이벤트 데이터 래퍼 클래스 - 박싱 방지용
    /// </summary>
    /// <typeparam name="T">래핑할 데이터 타입</typeparam>
    public class EventData<T>
    {
        public T Value { get; set; }
        
        public EventData(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// 게임 이벤트 타입 정의
    /// 게임에서 발생할 수 있는 모든 이벤트를 체계적으로 분류
    /// </summary>
    public enum GameEventType
    {
        // 플레이어 관련 이벤트
        PlayerSpawn,
        PlayerDeath,
        PlayerLevelUp,
        PlayerHealthChanged,
        PlayerManaChanged,
        
        // 게임플레이 이벤트
        GameStart,
        GamePause,
        GameEnd,
        WaveStart,
        WaveComplete,
        
        // UI 이벤트
        UIMenuOpen,
        UIMenuClose,
        UIInventoryUpdate,
        UIDialogShow,
        UIDialogHide,
        
        // 아이템/인벤토리 이벤트
        ItemPickup,
        ItemDrop,
        ItemUse,
        InventoryUpdate,
        
        // 전투 이벤트
        BattleStart,
        BattleEnd,
        EnemySpawn,
        EnemyDeath,
        SpellCast,
        DamageDealt,
        
        // 오디오 이벤트
        PlaySound,
        PlayMusic,
        StopMusic,
        
        // 시스템 이벤트
        SceneLoad,
        SceneUnload,
        SaveGame,
        LoadGame
    }

    /// <summary>
    /// 게임 이벤트 시스템 관리자 (Publisher-Subscriber 패턴 구현)
    /// 박싱/언박싱 최적화 버전 - 성능과 타입 안정성 모두 확보
    /// </summary>
    public static class EventManager
    {
        /// <summary>
        /// 레거시 이벤트 리스너 엔트리 클래스 (박싱/언박싱 발생)
        /// </summary>
        private class EventListenerEntry
        {
            /// <summary>
            /// 이벤트 리스너 리스트
            /// </summary>
            public readonly List<EventListener> Listeners = new List<EventListener>();

            /// <summary>
            /// 캐시된 이벤트 리스너 배열
            /// </summary>
            public EventListener[] Cached;

            /// <summary>
            /// 캐시 배열을 업데이트합니다.
            /// </summary>
            public void UpdateCache()
            {
                Cached = Listeners.ToArray();
            }
        }

        /// <summary>
        /// 제네릭 이벤트 리스너 엔트리 클래스 (박싱/언박싱 방지)
        /// </summary>
        private class GenericEventListenerEntry<T> where T : class
        {
            public readonly List<EventListener<T>> Listeners = new List<EventListener<T>>();
            public EventListener<T>[] Cached;

            public void UpdateCache()
            {
                Cached = Listeners.ToArray();
            }
        }

        // 레거시 이벤트 리스너를 저장하는 Dictionary (박싱/언박싱 발생)
        private static readonly Dictionary<GameEventType, EventListenerEntry> eventListenerDic = 
            new Dictionary<GameEventType, EventListenerEntry>();

        // 제네릭 이벤트 리스너를 위한 Dictionary (박싱/언박싱 방지)
        private static readonly Dictionary<(GameEventType, Type), object> genericEventListenerDic = 
            new Dictionary<(GameEventType, Type), object>();

        // 통계 정보 (디버깅용)
        private static int totalSubscriptions = 0;
        private static int totalDispatches = 0;

        #region Legacy Object-based Methods (박싱/언박싱 발생)

        /// <summary>
        /// 이벤트 리스너 등록 (레거시 - 박싱/언박싱 발생)
        /// </summary>
        /// <param name="eventType">구독할 이벤트 타입</param>
        /// <param name="listener">콜백 함수</param>
        public static void Subscribe(GameEventType eventType, EventListener listener)
        {
            if (listener == null)
            {
                Debug.LogWarning($"EventManager: null 리스너를 구독하려고 시도했습니다. EventType: {eventType}");
                return;
            }

            if (!eventListenerDic.TryGetValue(eventType, out EventListenerEntry entry))
            {
                entry = new EventListenerEntry();
                eventListenerDic[eventType] = entry;
            }

            // 중복 구독 방지
            if (entry.Listeners.Contains(listener))
            {
                Debug.LogWarning($"EventManager: 이미 구독된 리스너입니다. EventType: {eventType}");
                return;
            }

            entry.Listeners.Add(listener);
            entry.UpdateCache();
            totalSubscriptions++;

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 이벤트 구독됨 - {eventType}, 총 리스너 수: {entry.Listeners.Count}");
            #endif
        }

        /// <summary>
        /// 이벤트 리스너 해제 (레거시 - 박싱/언박싱 발생)
        /// </summary>
        /// <param name="eventType">구독 해제할 이벤트 타입</param>
        /// <param name="listener">제거할 콜백 함수</param>
        public static void Unsubscribe(GameEventType eventType, EventListener listener)
        {
            if (listener == null)
            {
                Debug.LogWarning($"EventManager: null 리스너를 구독 해제하려고 시도했습니다. EventType: {eventType}");
                return;
            }

            if (!eventListenerDic.TryGetValue(eventType, out EventListenerEntry entry))
            {
                Debug.LogWarning($"EventManager: 구독되지 않은 이벤트 타입입니다. EventType: {eventType}");
                return;
            }

            bool removed = entry.Listeners.Remove(listener);
            if (!removed)
            {
                Debug.LogWarning($"EventManager: 구독되지 않은 리스너를 해제하려고 시도했습니다. EventType: {eventType}");
                return;
            }

            if (entry.Listeners.Count == 0)
            {
                eventListenerDic.Remove(eventType);
            }
            else
            {
                entry.UpdateCache();
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 이벤트 구독 해제됨 - {eventType}, 남은 리스너 수: {entry.Listeners.Count}");
            #endif
        }

        /// <summary>
        /// 이벤트 발생 알림 (레거시 - 박싱/언박싱 발생)
        /// </summary>
        /// <param name="eventType">발생시킬 이벤트 타입</param>
        /// <param name="eventArgs">전달할 데이터 객체</param>
        public static void Dispatch(GameEventType eventType, object eventArgs = null)
        {
            if (!eventListenerDic.TryGetValue(eventType, out EventListenerEntry entry))
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"EventManager: 구독자가 없는 이벤트 발생 - {eventType}");
                #endif
                return;
            }

            var listeners = entry.Cached;
            if (listeners == null || listeners.Length == 0)
            {
                return;
            }

            totalDispatches++;

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 이벤트 발생 - {eventType}, 알림 대상: {listeners.Length}개");
            #endif

            // 예외 발생 시에도 다른 리스너들이 실행될 수 있도록 각각 try-catch 처리
            for (int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    listeners[i]?.Invoke(eventArgs);
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventManager: 이벤트 처리 중 예외 발생\n" +
                                  $"EventType: {eventType}\n" +
                                  $"Listener Index: {i}\n" +
                                  $"Exception: {e}");
                }
            }
        }

        #endregion

        #region Generic Methods (박싱/언박싱 방지)

        /// <summary>
        /// 제네릭 이벤트 리스너 등록 (박싱/언박싱 방지)
        /// </summary>
        /// <typeparam name="T">이벤트 데이터 타입 (클래스만 가능)</typeparam>
        /// <param name="eventType">구독할 이벤트 타입</param>
        /// <param name="listener">콜백 함수</param>
        public static void Subscribe<T>(GameEventType eventType, EventListener<T> listener) where T : class
        {
            if (listener == null)
            {
                Debug.LogWarning($"EventManager: null 제네릭 리스너를 구독하려고 시도했습니다. EventType: {eventType}, Type: {typeof(T)}");
                return;
            }

            var key = (eventType, typeof(T));
            
            if (!genericEventListenerDic.TryGetValue(key, out object entryObj))
            {
                entryObj = new GenericEventListenerEntry<T>();
                genericEventListenerDic[key] = entryObj;
            }

            var entry = (GenericEventListenerEntry<T>)entryObj;
            
            // 중복 구독 방지
            if (entry.Listeners.Contains(listener))
            {
                Debug.LogWarning($"EventManager: 이미 구독된 제네릭 리스너입니다. EventType: {eventType}, Type: {typeof(T)}");
                return;
            }

            entry.Listeners.Add(listener);
            entry.UpdateCache();
            totalSubscriptions++;

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 제네릭 이벤트 구독됨 - {eventType}<{typeof(T).Name}>, 총 리스너 수: {entry.Listeners.Count}");
            #endif
        }

        /// <summary>
        /// 제네릭 이벤트 리스너 해제 (박싱/언박싱 방지)
        /// </summary>
        /// <typeparam name="T">이벤트 데이터 타입 (클래스만 가능)</typeparam>
        /// <param name="eventType">구독 해제할 이벤트 타입</param>
        /// <param name="listener">제거할 콜백 함수</param>
        public static void Unsubscribe<T>(GameEventType eventType, EventListener<T> listener) where T : class
        {
            if (listener == null)
            {
                Debug.LogWarning($"EventManager: null 제네릭 리스너를 구독 해제하려고 시도했습니다. EventType: {eventType}, Type: {typeof(T)}");
                return;
            }

            var key = (eventType, typeof(T));
            
            if (!genericEventListenerDic.TryGetValue(key, out object entryObj))
            {
                Debug.LogWarning($"EventManager: 구독되지 않은 제네릭 이벤트 타입입니다. EventType: {eventType}, Type: {typeof(T)}");
                return;
            }

            var entry = (GenericEventListenerEntry<T>)entryObj;
            
            bool removed = entry.Listeners.Remove(listener);
            if (!removed)
            {
                Debug.LogWarning($"EventManager: 구독되지 않은 제네릭 리스너를 해제하려고 시도했습니다. EventType: {eventType}, Type: {typeof(T)}");
                return;
            }

            if (entry.Listeners.Count == 0)
            {
                genericEventListenerDic.Remove(key);
            }
            else
            {
                entry.UpdateCache();
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 제네릭 이벤트 구독 해제됨 - {eventType}<{typeof(T).Name}>, 남은 리스너 수: {entry.Listeners.Count}");
            #endif
        }

        /// <summary>
        /// 제네릭 이벤트 발생 알림 (박싱/언박싱 방지)
        /// </summary>
        /// <typeparam name="T">이벤트 데이터 타입 (클래스만 가능)</typeparam>
        /// <param name="eventType">발생시킬 이벤트 타입</param>
        /// <param name="eventArgs">전달할 데이터 객체</param>
        public static void Dispatch<T>(GameEventType eventType, T eventArgs) where T : class
        {
            var key = (eventType, typeof(T));
            
            if (!genericEventListenerDic.TryGetValue(key, out object entryObj))
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"EventManager: 구독자가 없는 제네릭 이벤트 발생 - {eventType}<{typeof(T).Name}>");
                #endif
                return;
            }

            var entry = (GenericEventListenerEntry<T>)entryObj;
            var listeners = entry.Cached;

            if (listeners == null || listeners.Length == 0)
            {
                return;
            }

            totalDispatches++;

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 제네릭 이벤트 발생 - {eventType}<{typeof(T).Name}>, 알림 대상: {listeners.Length}개");
            #endif

            for (int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    listeners[i]?.Invoke(eventArgs);
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventManager: 제네릭 이벤트 처리 중 예외 발생\n" +
                                  $"EventType: {eventType}<{typeof(T).Name}>\n" +
                                  $"Listener Index: {i}\n" +
                                  $"Exception: {e}");
                }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Value Type을 안전하게 전달하기 위한 헬퍼 메서드
        /// </summary>
        /// <typeparam name="T">Value Type</typeparam>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="value">전달할 값</param>
        public static void DispatchValue<T>(GameEventType eventType, T value) where T : struct
        {
            var eventData = new EventData<T>(value);
            Dispatch(eventType, eventData);
        }

        /// <summary>
        /// 특정 이벤트 타입의 모든 리스너를 해제
        /// </summary>
        /// <param name="eventType">해제할 이벤트 타입</param>
        public static void UnsubscribeAll(GameEventType eventType)
        {
            // 레거시 이벤트 해제
            if (eventListenerDic.ContainsKey(eventType))
            {
                eventListenerDic.Remove(eventType);
            }

            // 제네릭 이벤트 해제
            var keysToRemove = new List<(GameEventType, Type)>();
            foreach (var key in genericEventListenerDic.Keys)
            {
                if (key.Item1 == eventType)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                genericEventListenerDic.Remove(key);
            }
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 모든 리스너 해제됨 - {eventType}");
            #endif
        }

        /// <summary>
        /// 모든 이벤트 리스너 해제 (주로 씬 전환 시 사용)
        /// </summary>
        public static void UnsubscribeAll()
        {
            int totalListeners = eventListenerDic.Count + genericEventListenerDic.Count;
            eventListenerDic.Clear();
            genericEventListenerDic.Clear();
            totalSubscriptions = 0;
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 모든 이벤트 리스너 해제됨 (총 {totalListeners}개 타입)");
            #endif
        }

        /// <summary>
        /// 특정 이벤트 타입에 구독된 리스너 수 반환
        /// </summary>
        /// <param name="eventType">확인할 이벤트 타입</param>
        /// <returns>구독된 리스너 수</returns>
        public static int GetListenerCount(GameEventType eventType)
        {
            int count = 0;
            
            // 레거시 이벤트 카운트
            if (eventListenerDic.TryGetValue(eventType, out EventListenerEntry entry))
            {
                count += entry.Listeners.Count;
            }

            // 제네릭 이벤트 카운트
            foreach (var kvp in genericEventListenerDic)
            {
                if (kvp.Key.Item1 == eventType)
                {
                    var entryObj = kvp.Value;
                    var listenersProp = entryObj.GetType().GetProperty("Listeners");
                    if (listenersProp?.GetValue(entryObj) is System.Collections.IList listeners)
                    {
                        count += listeners.Count;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 전체 구독 통계 정보 반환 (디버깅용)
        /// </summary>
        /// <returns>구독 통계 정보</returns>
        public static string GetStatistics()
        {
            int totalTypes = eventListenerDic.Count + genericEventListenerDic.Count;
            int totalListeners = 0;
            
            foreach (var entry in eventListenerDic.Values)
            {
                totalListeners += entry.Listeners.Count;
            }

            foreach (var entryObj in genericEventListenerDic.Values)
            {
                var listenersProp = entryObj.GetType().GetProperty("Listeners");
                if (listenersProp?.GetValue(entryObj) is System.Collections.IList listeners)
                {
                    totalListeners += listeners.Count;
                }
            }

            return $"EventManager 통계:\n" +
                   $"- 활성 이벤트 타입: {totalTypes} (레거시: {eventListenerDic.Count}, 제네릭: {genericEventListenerDic.Count})\n" +
                   $"- 총 리스너 수: {totalListeners}\n" +
                   $"- 총 구독 횟수: {totalSubscriptions}\n" +
                   $"- 총 이벤트 발생 횟수: {totalDispatches}";
        }

        /// <summary>
        /// 이벤트 타입별 구독 정보 출력 (디버깅용)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void PrintAllSubscriptions()
        {
            Debug.Log("=== EventManager 구독 현황 ===");
            
            Debug.Log("레거시 이벤트:");
            foreach (var kvp in eventListenerDic)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.Listeners.Count}개 리스너");
            }
            
            Debug.Log("제네릭 이벤트:");
            foreach (var kvp in genericEventListenerDic)
            {
                var entryObj = kvp.Value;
                var listenersProp = entryObj.GetType().GetProperty("Listeners");
                if (listenersProp?.GetValue(entryObj) is System.Collections.IList listeners)
                {
                    Debug.Log($"  {kvp.Key.Item1}<{kvp.Key.Item2.Name}>: {listeners.Count}개 리스너");
                }
            }
            
            Debug.Log("===============================");
        }

        #endregion
    }
}