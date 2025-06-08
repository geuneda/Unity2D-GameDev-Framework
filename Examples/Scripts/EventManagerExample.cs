using UnityEngine;
using Unity2DFramework.Core.Managers;
using System.Collections;

namespace Unity2DFramework.Examples
{
    /// <summary>
    /// EventManager 사용 예제 클래스
    /// 이벤트 기반 통신으로 Find 사용 없이 컴포넌트 간 상호작용을 보여줍니다.
    /// </summary>
    public class EventManagerExample : MonoBehaviour
    {
        [Header("예제 설정")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float autoTestInterval = 3f;
        
        private bool isAutoTesting = false;
        
        private void Start()
        {
            InitializeEventSystem();
            StartCoroutine(AutoTestRoutine());
        }
        
        /// <summary>
        /// 이벤트 시스템 초기화 및 구독
        /// </summary>
        private void InitializeEventSystem()
        {
            Debug.Log("=== EventManager 예제 시작 ===");
            
            // 다양한 이벤트 타입 구독
            EventManager.Subscribe(GameEventType.PlayerSpawn, OnPlayerSpawn);
            EventManager.Subscribe(GameEventType.PlayerDeath, OnPlayerDeath);
            EventManager.Subscribe(GameEventType.DamageDealt, OnDamageDealt);
            EventManager.Subscribe(GameEventType.ItemPickup, OnItemPickup);
            EventManager.Subscribe(GameEventType.GameStart, OnGameStart);
            EventManager.Subscribe(GameEventType.PlaySound, OnPlaySound);
            
            Debug.Log("모든 이벤트 구독 완료");
        }
        
        /// <summary>
        /// 자동 테스트 루틴
        /// </summary>
        private IEnumerator AutoTestRoutine()
        {
            yield return new WaitForSeconds(1f);
            
            while (true)
            {
                if (isAutoTesting)
                {
                    TestRandomEvent();
                }
                yield return new WaitForSeconds(autoTestInterval);
            }
        }
        
        /// <summary>
        /// 랜덤 이벤트 테스트
        /// </summary>
        private void TestRandomEvent()
        {
            var eventTypes = System.Enum.GetValues(typeof(GameEventType));
            var randomEventType = (GameEventType)eventTypes.GetValue(Random.Range(0, eventTypes.Length));
            
            switch (randomEventType)
            {
                case GameEventType.PlayerSpawn:
                    TestPlayerSpawn();
                    break;
                case GameEventType.DamageDealt:
                    TestDamageEvent();
                    break;
                case GameEventType.ItemPickup:
                    TestItemPickup();
                    break;
                case GameEventType.PlaySound:
                    TestSoundEvent();
                    break;
                default:
                    EventManager.Dispatch(randomEventType);
                    break;
            }
        }
        
        /// <summary>
        /// 플레이어 스폰 이벤트 테스트
        /// </summary>
        private void TestPlayerSpawn()
        {
            var playerData = new PlayerSpawnData
            {
                position = new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0),
                playerName = $"Player_{Random.Range(1, 100)}",
                level = Random.Range(1, 10)
            };
            
            EventManager.Dispatch(GameEventType.PlayerSpawn, playerData);
        }
        
        /// <summary>
        /// 데미지 이벤트 테스트
        /// </summary>
        private void TestDamageEvent()
        {
            var damageData = new DamageData
            {
                amount = Random.Range(10, 50),
                source = gameObject,
                position = transform.position,
                damageType = (DamageType)Random.Range(0, 3)
            };
            
            EventManager.Dispatch(GameEventType.DamageDealt, damageData);
        }
        
        /// <summary>
        /// 아이템 픽업 이벤트 테스트
        /// </summary>
        private void TestItemPickup()
        {
            var itemData = new ItemData
            {
                itemId = Random.Range(1, 10),
                itemName = $"Item_{Random.Range(1, 100)}",
                quantity = Random.Range(1, 5),
                rarity = (ItemRarity)Random.Range(0, 4)
            };
            
            EventManager.Dispatch(GameEventType.ItemPickup, itemData);
        }
        
        /// <summary>
        /// 사운드 이벤트 테스트
        /// </summary>
        private void TestSoundEvent()
        {
            string[] soundNames = { "Jump", "Attack", "Pickup", "Death", "Victory" };
            string randomSound = soundNames[Random.Range(0, soundNames.Length)];
            
            EventManager.Dispatch(GameEventType.PlaySound, randomSound);
        }
        
        // ===== 이벤트 리스너들 =====
        
        private void OnPlayerSpawn(object args)
        {
            if (args is PlayerSpawnData spawnData)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"🎮 플레이어 스폰: {spawnData.playerName} (레벨 {spawnData.level}) " +
                             $"위치: {spawnData.position}");
                }
            }
        }
        
        private void OnPlayerDeath(object args)
        {
            if (enableDebugLogs)
            {
                Debug.Log("💀 플레이어 사망 이벤트 처리");
            }
            
            // 사망 시 추가 이벤트 발생 (이벤트 체이닝 예제)
            EventManager.Dispatch(GameEventType.PlaySound, "PlayerDeath");
            EventManager.Dispatch(GameEventType.GameEnd);
        }
        
        private void OnDamageDealt(object args)
        {
            if (args is DamageData damageData)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"⚔️ 데미지 처리: {damageData.amount} ({damageData.damageType})");
                }
                
                // 크리티컬 데미지 체크
                if (damageData.amount > 40)
                {
                    EventManager.Dispatch(GameEventType.PlaySound, "CriticalHit");
                }
            }
        }
        
        private void OnItemPickup(object args)
        {
            if (args is ItemData itemData)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"📦 아이템 획득: {itemData.itemName} x{itemData.quantity} " +
                             $"({itemData.rarity})");
                }
                
                // 레어 아이템 특별 효과
                if (itemData.rarity == ItemRarity.Legendary)
                {
                    EventManager.Dispatch(GameEventType.PlaySound, "LegendaryPickup");
                }
            }
        }
        
        private void OnGameStart(object args)
        {
            if (enableDebugLogs)
            {
                Debug.Log("🎯 게임 시작 이벤트 처리");
            }
        }
        
        private void OnPlaySound(object args)
        {
            if (args is string soundName)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"🔊 사운드 재생: {soundName}");
                }
            }
        }
        
        // ===== 키보드 입력 테스트 =====
        
        private void Update()
        {
            HandleKeyboardInput();
        }
        
        private void HandleKeyboardInput()
        {
            // F1: 플레이어 스폰
            if (Input.GetKeyDown(KeyCode.F1))
            {
                TestPlayerSpawn();
            }
            
            // F2: 데미지 이벤트
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TestDamageEvent();
            }
            
            // F3: 아이템 픽업
            if (Input.GetKeyDown(KeyCode.F3))
            {
                TestItemPickup();
            }
            
            // F4: 통계 정보 출력
            if (Input.GetKeyDown(KeyCode.F4))
            {
                Debug.Log(EventManager.GetStatistics());
                EventManager.PrintAllSubscriptions();
            }
            
            // F5: 자동 테스트 토글
            if (Input.GetKeyDown(KeyCode.F5))
            {
                isAutoTesting = !isAutoTesting;
                Debug.Log($"자동 테스트: {(isAutoTesting ? "활성화" : "비활성화")}");
            }
            
            // F6: 플레이어 사망 (이벤트 체이닝 테스트)
            if (Input.GetKeyDown(KeyCode.F6))
            {
                EventManager.Dispatch(GameEventType.PlayerDeath);
            }
        }
        
        private void OnDestroy()
        {
            // 반드시 모든 이벤트 구독 해제 (메모리 누수 방지)
            EventManager.Unsubscribe(GameEventType.PlayerSpawn, OnPlayerSpawn);
            EventManager.Unsubscribe(GameEventType.PlayerDeath, OnPlayerDeath);
            EventManager.Unsubscribe(GameEventType.DamageDealt, OnDamageDealt);
            EventManager.Unsubscribe(GameEventType.ItemPickup, OnItemPickup);
            EventManager.Unsubscribe(GameEventType.GameStart, OnGameStart);
            EventManager.Unsubscribe(GameEventType.PlaySound, OnPlaySound);
            
            Debug.Log("EventManagerExample: 모든 이벤트 구독 해제 완료");
        }
        
        // ===== 컨텍스트 메뉴 (Inspector에서 우클릭) =====
        
        [ContextMenu("게임 시작 이벤트 발생")]
        private void TriggerGameStart()
        {
            EventManager.Dispatch(GameEventType.GameStart);
        }
        
        [ContextMenu("모든 이벤트 통계 출력")]
        private void PrintEventStatistics()
        {
            Debug.Log(EventManager.GetStatistics());
            EventManager.PrintAllSubscriptions();
        }
        
        [ContextMenu("이벤트 시스템 초기화")]
        private void ResetEventSystem()
        {
            EventManager.UnsubscribeAll();
            InitializeEventSystem();
            Debug.Log("이벤트 시스템이 초기화되었습니다.");
        }
    }
    
    // ===== 이벤트 데이터 클래스들 =====
    
    /// <summary>
    /// 플레이어 스폰 데이터
    /// </summary>
    [System.Serializable]
    public class PlayerSpawnData
    {
        public Vector3 position;
        public string playerName;
        public int level;
    }
    
    /// <summary>
    /// 데미지 데이터
    /// </summary>
    [System.Serializable]
    public class DamageData
    {
        public int amount;
        public GameObject source;
        public Vector3 position;
        public DamageType damageType;
    }
    
    /// <summary>
    /// 아이템 데이터
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public int itemId;
        public string itemName;
        public int quantity;
        public ItemRarity rarity;
    }
    
    /// <summary>
    /// 데미지 타입 열거형
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magic,
        Fire,
        Ice,
        Lightning
    }
    
    /// <summary>
    /// 아이템 희귀도 열거형
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}