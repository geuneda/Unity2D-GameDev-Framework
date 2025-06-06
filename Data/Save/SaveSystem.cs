using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Unity2DFramework.Data.Save
{
    /// <summary>
    /// 게임 데이터 저장/로드 시스템
    /// JSON 기반으로 안전하고 확장 가능한 세이브 시스템 제공
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        
        [Header("저장 설정")]
        [SerializeField] private string saveFileName = "gamedata.json";
        [SerializeField] private bool encryptData = true;
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 60f; // 60초마다 자동 저장
        
        private string savePath;
        private GameData currentGameData;
        private float autoSaveTimer;
        
        // 저장/로드 이벤트
        public System.Action<GameData> OnDataSaved;
        public System.Action<GameData> OnDataLoaded;
        public System.Action OnSaveError;
        public System.Action OnLoadError;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSaveSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            // 자동 저장 타이머
            if (autoSave && currentGameData != null)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    SaveGame();
                    autoSaveTimer = 0f;
                }
            }
        }
        
        /// <summary>
        /// 저장 시스템 초기화
        /// </summary>
        private void InitializeSaveSystem()
        {
            // 저장 경로 설정
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            // 게임 데이터 초기화
            currentGameData = new GameData();
            
            Debug.Log($"[SaveSystem] 저장 시스템 초기화 완료. 저장 경로: {savePath}");
        }
        
        /// <summary>
        /// 게임 데이터 저장
        /// </summary>
        public void SaveGame()
        {
            try
            {
                // 현재 시간 업데이트
                currentGameData.lastSaveTime = System.DateTime.Now.ToBinary();
                
                // JSON으로 직렬화
                string jsonData = JsonUtility.ToJson(currentGameData, true);
                
                // 암호화 (옵션)
                if (encryptData)
                {
                    jsonData = EncryptData(jsonData);
                }
                
                // 파일에 저장
                File.WriteAllText(savePath, jsonData);
                
                OnDataSaved?.Invoke(currentGameData);
                Debug.Log("[SaveSystem] 게임 데이터 저장 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] 저장 실패: {e.Message}");
                OnSaveError?.Invoke();
            }
        }
        
        /// <summary>
        /// 게임 데이터 로드
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    // 파일에서 읽기
                    string jsonData = File.ReadAllText(savePath);
                    
                    // 복호화 (옵션)
                    if (encryptData)
                    {
                        jsonData = DecryptData(jsonData);
                    }
                    
                    // JSON에서 역직렬화
                    currentGameData = JsonUtility.FromJson<GameData>(jsonData);
                    
                    OnDataLoaded?.Invoke(currentGameData);
                    Debug.Log("[SaveSystem] 게임 데이터 로드 완료");
                }
                else
                {
                    Debug.Log("[SaveSystem] 저장 파일이 없습니다. 새 게임을 시작합니다.");
                    currentGameData = new GameData();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] 로드 실패: {e.Message}");
                OnLoadError?.Invoke();
                currentGameData = new GameData(); // 기본 데이터로 초기화
            }
        }
        
        /// <summary>
        /// 저장 파일 삭제
        /// </summary>
        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    currentGameData = new GameData();
                    Debug.Log("[SaveSystem] 저장 파일 삭제 완료");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] 파일 삭제 실패: {e.Message}");
            }
        }
        
        /// <summary>
        /// 저장 파일 존재 여부 확인
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(savePath);
        }
        
        /// <summary>
        /// 간단한 데이터 암호화
        /// </summary>
        private string EncryptData(string data)
        {
            // 간단한 XOR 암호화 (실제 프로젝트에서는 더 강력한 암호화 사용 권장)
            char[] chars = data.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] ^ 123); // 간단한 키
            }
            return new string(chars);
        }
        
        /// <summary>
        /// 데이터 복호화
        /// </summary>
        private string DecryptData(string encryptedData)
        {
            // XOR 복호화 (암호화와 동일한 과정)
            return EncryptData(encryptedData);
        }
        
        // 데이터 접근 메서드들
        public GameData GetGameData() => currentGameData;
        
        public void SetPlayerLevel(int level) => currentGameData.playerLevel = level;
        public int GetPlayerLevel() => currentGameData.playerLevel;
        
        public void SetPlayerScore(int score) => currentGameData.playerScore = score;
        public int GetPlayerScore() => currentGameData.playerScore;
        
        public void SetBoolValue(string key, bool value) => currentGameData.boolValues[key] = value;
        public bool GetBoolValue(string key, bool defaultValue = false) => 
            currentGameData.boolValues.ContainsKey(key) ? currentGameData.boolValues[key] : defaultValue;
        
        public void SetIntValue(string key, int value) => currentGameData.intValues[key] = value;
        public int GetIntValue(string key, int defaultValue = 0) => 
            currentGameData.intValues.ContainsKey(key) ? currentGameData.intValues[key] : defaultValue;
        
        public void SetFloatValue(string key, float value) => currentGameData.floatValues[key] = value;
        public float GetFloatValue(string key, float defaultValue = 0f) => 
            currentGameData.floatValues.ContainsKey(key) ? currentGameData.floatValues[key] : defaultValue;
        
        public void SetStringValue(string key, string value) => currentGameData.stringValues[key] = value;
        public string GetStringValue(string key, string defaultValue = "") => 
            currentGameData.stringValues.ContainsKey(key) ? currentGameData.stringValues[key] : defaultValue;
    }
    
    /// <summary>
    /// 게임 데이터 구조체
    /// 저장할 모든 데이터를 포함
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        [Header("기본 정보")]
        public int playerLevel = 1;
        public int playerScore = 0;
        public long lastSaveTime;
        
        [Header("설정")]
        public float masterVolume = 1f;
        public float bgmVolume = 0.7f;
        public float sfxVolume = 1f;
        
        [Header("진행 상황")]
        public int currentStage = 1;
        public bool[] unlockedLevels = new bool[50]; // 최대 50개 레벨
        
        [Header("커스텀 데이터")]
        public Dictionary<string, bool> boolValues = new Dictionary<string, bool>();
        public Dictionary<string, int> intValues = new Dictionary<string, int>();
        public Dictionary<string, float> floatValues = new Dictionary<string, float>();
        public Dictionary<string, string> stringValues = new Dictionary<string, string>();
        
        public GameData()
        {
            // 기본값 설정
            unlockedLevels[0] = true; // 첫 번째 레벨은 항상 해금
        }
    }
}