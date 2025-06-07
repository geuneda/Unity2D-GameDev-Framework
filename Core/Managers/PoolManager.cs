using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 오브젝트 풀 관리자 클래스
/// 게임 내 다양한 오브젝트 풀을 관리합니다.
/// </summary>
public class PoolManager : MonoBehaviour
{
    #region 싱글톤
    private static PoolManager _instance;
    
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PoolManager");
                _instance = go.AddComponent<PoolManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion
    
    [System.Serializable]
    public class PoolGroup
    {
        public string groupName;
        public List<PoolData> pools = new List<PoolData>();
    }
    
    [System.Serializable]
    public class PoolData
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
        public bool allowExpand = true;
        public int maxSize = 100;
        public bool cullExcess = false;
        public float cullDelay = 60f; // 60초 이후 초과 오브젝트 제거
    }
    
    [SerializeField] private List<PoolGroup> _poolGroups = new List<PoolGroup>();
    [SerializeField] private bool _autoInitializeOnAwake = true;
    [SerializeField] private bool _debugMode = false;
    
    private Dictionary<string, ObjectPool> _objectPools = new Dictionary<string, ObjectPool>();
    private Dictionary<string, Transform> _poolGroupTransforms = new Dictionary<string, Transform>();
    
    private bool _isInitialized = false;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (_autoInitializeOnAwake)
        {
            Initialize();
        }
    }
    
    /// <summary>
    /// 풀 매니저를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;
        
        foreach (var group in _poolGroups)
        {
            // 그룹 부모 오브젝트 생성
            GameObject groupObject = new GameObject($"{group.groupName}_Pools");
            groupObject.transform.SetParent(transform);
            _poolGroupTransforms[group.groupName] = groupObject.transform;
            
            foreach (var poolData in group.pools)
            {
                CreatePool(group.groupName, poolData);
            }
        }
        
        _isInitialized = true;
        
        if (_debugMode)
        {
            Debug.Log("PoolManager가 초기화되었습니다.");
        }
    }
    
    /// <summary>
    /// 새 오브젝트 풀을 생성합니다.
    /// </summary>
    private void CreatePool(string groupName, PoolData poolData)
    {
        string poolKey = $"{groupName}/{poolData.tag}";
        
        if (_objectPools.ContainsKey(poolKey))
        {
            Debug.LogWarning($"이미 '{poolKey}' 키를 가진 풀이 존재합니다.");
            return;
        }
        
        GameObject poolObject = new GameObject($"{poolData.tag}_Pool");
        poolObject.transform.SetParent(_poolGroupTransforms[groupName]);
        
        ObjectPool objectPool = poolObject.AddComponent<ObjectPool>();
        objectPool.Initialize(poolData.prefab, poolData.initialSize, poolData.allowExpand, poolData.maxSize, poolData.cullExcess, poolData.cullDelay);
        
        _objectPools.Add(poolKey, objectPool);
        
        if (_debugMode)
        {
            Debug.Log($"'{poolKey}' 풀이 생성되었습니다. 초기 크기: {poolData.initialSize}");
        }
    }
    
    /// <summary>
    /// 풀에 새 오브젝트 타입을 런타임에 등록합니다.
    /// </summary>
    public void RegisterPool(string groupName, string tag, GameObject prefab, int initialSize = 10)
    {
        if (!_poolGroupTransforms.ContainsKey(groupName))
        {
            // 새 그룹 생성
            GameObject groupObject = new GameObject($"{groupName}_Pools");
            groupObject.transform.SetParent(transform);
            _poolGroupTransforms[groupName] = groupObject.transform;
            
            // 그룹도 추가
            PoolGroup newGroup = new PoolGroup { groupName = groupName };
            _poolGroups.Add(newGroup);
        }
        
        PoolData poolData = new PoolData
        {
            tag = tag,
            prefab = prefab,
            initialSize = initialSize
        };
        
        // 그룹에 풀 데이터 추가
        var group = _poolGroups.Find(g => g.groupName == groupName);
        if (group != null)
        {
            group.pools.Add(poolData);
        }
        
        CreatePool(groupName, poolData);
    }
    
    /// <summary>
    /// 풀에서 오브젝트를 가져와 지정된 위치에 스폰합니다.
    /// </summary>
    public GameObject Spawn(string groupName, string tag, Vector3 position, Quaternion rotation)
    {
        string poolKey = $"{groupName}/{tag}";
        
        if (!_objectPools.TryGetValue(poolKey, out ObjectPool pool))
        {
            Debug.LogError($"'{poolKey}' 키를 가진 풀이 존재하지 않습니다.");
            return null;
        }
        
        return pool.Spawn(position, rotation);
    }
    
    /// <summary>
    /// 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void Despawn(GameObject obj)
    {
        foreach (var pool in _objectPools.Values)
        {
            if (pool.BelongsToThisPool(obj))
            {
                pool.Despawn(obj);
                return;
            }
        }
        
        Debug.LogWarning($"풀에서 관리하지 않는 오브젝트입니다: {obj.name}");
    }
    
    /// <summary>
    /// 특정 그룹의 모든 활성 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void DespawnAllInGroup(string groupName)
    {
        foreach (var kvp in _objectPools)
        {
            if (kvp.Key.StartsWith($"{groupName}/"))
            {
                kvp.Value.DespawnAll();
            }
        }
    }
    
    /// <summary>
    /// 특정 태그의 모든 활성 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void DespawnAllWithTag(string groupName, string tag)
    {
        string poolKey = $"{groupName}/{tag}";
        
        if (_objectPools.TryGetValue(poolKey, out ObjectPool pool))
        {
            pool.DespawnAll();
        }
    }
    
    /// <summary>
    /// 모든 활성 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void DespawnAll()
    {
        foreach (var pool in _objectPools.Values)
        {
            pool.DespawnAll();
        }
    }
    
    /// <summary>
    /// 특정 풀의 크기를 조정합니다.
    /// </summary>
    public void ResizePool(string groupName, string tag, int newSize)
    {
        string poolKey = $"{groupName}/{tag}";
        
        if (_objectPools.TryGetValue(poolKey, out ObjectPool pool))
        {
            pool.Resize(newSize);
        }
    }
    
    /// <summary>
    /// 특정 풀의 상태 정보를 반환합니다.
    /// </summary>
    public (int total, int active, int inactive) GetPoolStats(string groupName, string tag)
    {
        string poolKey = $"{groupName}/{tag}";
        
        if (_objectPools.TryGetValue(poolKey, out ObjectPool pool))
        {
            return pool.GetStats();
        }
        
        return (0, 0, 0);
    }
}