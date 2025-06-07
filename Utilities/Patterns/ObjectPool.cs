using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 오브젝트 풀링 시스템
/// 오브젝트의 생성 및 삭제를 최적화하여 성능을 향상시킵니다.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    
    #region 싱글톤
    private static ObjectPool _instance;
    
    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ObjectPool");
                _instance = go.AddComponent<ObjectPool>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    #endregion
    
    [SerializeField] private List<Pool> _pools = new List<Pool>();
    private Dictionary<string, Queue<GameObject>> _poolDictionary;
    private Dictionary<GameObject, string> _spawnedObjects;
    
    private bool _isInitialized = false;
    
    /// <summary>
    /// 오브젝트 풀을 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;
        
        _poolDictionary = new Dictionary<string, Queue<GameObject>>();
        _spawnedObjects = new Dictionary<GameObject, string>();
        
        foreach (Pool pool in _pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            GameObject poolParent = new GameObject($"{pool.tag}_Pool");
            poolParent.transform.SetParent(transform);
            
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, poolParent.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            
            _poolDictionary.Add(pool.tag, objectPool);
        }
        
        _isInitialized = true;
        Debug.Log("ObjectPool이 초기화되었습니다.");
    }
    
    /// <summary>
    /// 풀에 새로운 프리팹을 등록합니다.
    /// </summary>
    public void RegisterPool(string tag, GameObject prefab, int size)
    {
        if (_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"이미 '{tag}' 태그를 가진 풀이 존재합니다.");
            return;
        }
        
        Queue<GameObject> objectPool = new Queue<GameObject>();
        
        GameObject poolParent = new GameObject($"{tag}_Pool");
        poolParent.transform.SetParent(transform);
        
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent.transform);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
        
        _poolDictionary.Add(tag, objectPool);
        
        // Pool 리스트에도 추가
        Pool newPool = new Pool
        {
            tag = tag,
            prefab = prefab,
            size = size
        };
        
        _pools.Add(newPool);
    }
    
    /// <summary>
    /// 풀에서 오브젝트를 가져와 지정된 위치에 스폰합니다.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"'{tag}' 태그를 가진 풀이 존재하지 않습니다.");
            return null;
        }
        
        // 풀에 오브젝트가 없으면 동적으로 확장
        if (_poolDictionary[tag].Count == 0)
        {
            ExpandPool(tag);
        }
        
        GameObject objectToSpawn = _poolDictionary[tag].Dequeue();
        
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);
        
        // IPooledObject 인터페이스 구현 확인 및 호출
        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        pooledObj?.OnObjectSpawn();
        
        // 스폰된 오브젝트 추적
        _spawnedObjects[objectToSpawn] = tag;
        
        return objectToSpawn;
    }
    
    /// <summary>
    /// 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        if (_spawnedObjects.TryGetValue(obj, out string tag))
        {
            ReturnToPool(obj, tag);
        }
        else
        {
            Debug.LogWarning("이 오브젝트는 풀에서 스폰되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// 오브젝트를 지정된 태그의 풀로 반환합니다.
    /// </summary>
    public void ReturnToPool(GameObject obj, string tag)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"'{tag}' 태그를 가진 풀이 존재하지 않습니다.");
            return;
        }
        
        obj.SetActive(false);
        _poolDictionary[tag].Enqueue(obj);
        
        // 스폰된 오브젝트 목록에서 제거
        if (_spawnedObjects.ContainsKey(obj))
        {
            _spawnedObjects.Remove(obj);
        }
    }
    
    /// <summary>
    /// 풀의 크기를 확장합니다.
    /// </summary>
    private void ExpandPool(string tag)
    {
        Pool poolToExpand = _pools.Find(p => p.tag == tag);
        
        if (poolToExpand == null)
        {
            Debug.LogError($"'{tag}' 태그를 가진 풀 정보를 찾을 수 없습니다.");
            return;
        }
        
        Transform poolParent = transform.Find($"{tag}_Pool");
        
        // 원래 크기의 절반만큼 추가
        int expandSize = Mathf.Max(1, poolToExpand.size / 2);
        
        Debug.Log($"'{tag}' 풀을 {expandSize}개 확장합니다.");
        
        for (int i = 0; i < expandSize; i++)
        {
            GameObject obj = Instantiate(poolToExpand.prefab, poolParent);
            obj.SetActive(false);
            _poolDictionary[tag].Enqueue(obj);
        }
        
        // Pool 크기 업데이트
        poolToExpand.size += expandSize;
    }
    
    /// <summary>
    /// 태그에 해당하는 모든 활성 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void ReturnAllToPool(string tag)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogError($"'{tag}' 태그를 가진 풀이 존재하지 않습니다.");
            return;
        }
        
        // 현재 스폰된 모든 오브젝트 중 해당 태그를 가진 것을 찾아 반환
        var objectsToReturn = new List<GameObject>();
        
        foreach (var kvp in _spawnedObjects)
        {
            if (kvp.Value == tag)
            {
                objectsToReturn.Add(kvp.Key);
            }
        }
        
        foreach (var obj in objectsToReturn)
        {
            ReturnToPool(obj, tag);
        }
    }
    
    /// <summary>
    /// 모든 활성 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void ReturnAllToPool()
    {
        var objectsToReturn = new List<GameObject>(_spawnedObjects.Keys);
        
        foreach (var obj in objectsToReturn)
        {
            string tag = _spawnedObjects[obj];
            ReturnToPool(obj, tag);
        }
    }
}

/// <summary>
/// 풀링된 오브젝트가 구현해야 하는 인터페이스
/// </summary>
public interface IPooledObject
{
    void OnObjectSpawn();
}