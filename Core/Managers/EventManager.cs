using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity2DFramework.Core.Managers
{
    public delegate void EventListener(object args);

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
    /// 성능 최적화를 위한 캐시 배열 사용 및 메모리 할당 최소화
    /// Find 사용 없이 이벤트 기반 통신으로 컴포넌트 간 결합도 최소화
    /// </summary>
    public static class EventManager
    {
        /// <summary>
        /// 이벤트 리스너 엔트리 클래스
        /// 리스너 리스트와 캐시 배열을 함께 관리하여 성능 최적화
        /// </summary>
        private class EventListenerEntry
        {
            /// <summary>
            /// 이벤트 리스너 리스트
            /// </summary>
            public readonly List<EventListener> Listeners = new List<EventListener>();

            /// <summary>
            /// 캐시된 이벤트 리스너 배열 (성능 최적화용)
            /// 매번 ToArray() 호출을 방지하여 GC 부담 감소
            /// </summary>
            public EventListener[] CachedListeners;

            /// <summary>
            /// 캐시가 유효한지 여부
            /// </summary>
            public bool IsCacheValid;

            /// <summary>
            /// 캐시 배열을 업데이트합니다.
            /// </summary>
            public void UpdateCache()
            {
                if (Listeners.Count == 0)
                {
                    CachedListeners = null;
                }
                else
                {
                    CachedListeners = Listeners.ToArray();
                }
                IsCacheValid = true;
            }

            /// <summary>
            /// 캐시를 무효화합니다.
            /// </summary>
            public void InvalidateCache()
            {
                IsCacheValid = false;
            }

            /// <summary>
            /// 유효한 캐시 배열을 반환합니다.
            /// </summary>
            public EventListener[] GetValidCache()
            {
                if (!IsCacheValid)
                {
                    UpdateCache();
                }
                return CachedListeners;
            }
        }

        // 이벤트 리스너를 저장하는 Dictionary (캐시 최적화 버전)
        private static readonly Dictionary<GameEventType, EventListenerEntry> eventListenerDic = 
            new Dictionary<GameEventType, EventListenerEntry>();

        // 통계 정보 (디버깅용)
        private static int totalSubscriptions = 0;
        private static int totalDispatches = 0;

        /// <summary>
        /// 이벤트 리스너 등록
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
            entry.InvalidateCache();
            totalSubscriptions++;

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 이벤트 구독됨 - {eventType}, 총 리스너 수: {entry.Listeners.Count}");
            #endif
        }

        /// <summary>
        /// 이벤트 리스너 해제
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
                entry.InvalidateCache();
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 이벤트 구독 해제됨 - {eventType}, 남은 리스너 수: {entry.Listeners.Count}");
            #endif
        }

        /// <summary>
        /// 특정 이벤트 타입의 모든 리스너를 해제
        /// </summary>
        /// <param name="eventType">해제할 이벤트 타입</param>
        public static void UnsubscribeAll(GameEventType eventType)
        {
            if (eventListenerDic.ContainsKey(eventType))
            {
                eventListenerDic.Remove(eventType);
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"EventManager: 모든 리스너 해제됨 - {eventType}");
                #endif
            }
        }

        /// <summary>
        /// 모든 이벤트 리스너 해제 (주로 씬 전환 시 사용)
        /// </summary>
        public static void UnsubscribeAll()
        {
            int totalListeners = eventListenerDic.Count;
            eventListenerDic.Clear();
            totalSubscriptions = 0;
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"EventManager: 모든 이벤트 리스너 해제됨 (총 {totalListeners}개 타입)");
            #endif
        }

        /// <summary>
        /// 이벤트 발생 알림
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

            var listeners = entry.GetValidCache();
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

        /// <summary>
        /// 특정 이벤트 타입에 구독된 리스너 수 반환
        /// </summary>
        /// <param name="eventType">확인할 이벤트 타입</param>
        /// <returns>구독된 리스너 수</returns>
        public static int GetListenerCount(GameEventType eventType)
        {
            return eventListenerDic.TryGetValue(eventType, out EventListenerEntry entry) 
                ? entry.Listeners.Count : 0;
        }

        /// <summary>
        /// 전체 구독 통계 정보 반환 (디버깅용)
        /// </summary>
        /// <returns>구독 통계 정보</returns>
        public static string GetStatistics()
        {
            int totalTypes = eventListenerDic.Count;
            int totalListeners = 0;
            
            foreach (var entry in eventListenerDic.Values)
            {
                totalListeners += entry.Listeners.Count;
            }

            return $"EventManager 통계:\n" +
                   $"- 활성 이벤트 타입: {totalTypes}\n" +
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
            foreach (var kvp in eventListenerDic)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value.Listeners.Count}개 리스너");
            }
            Debug.Log("===============================");
        }
    }
}