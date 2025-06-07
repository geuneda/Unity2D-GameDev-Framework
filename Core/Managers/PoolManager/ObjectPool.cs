using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 개별 오브젝트 풀 관리 클래스
/// 동일한 프리팹에 대한 풀링을 관리합니다.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    private GameObject _prefab;
    private int _initialSize;
    private bool _allowExpand;
    private int _maxSize;
    private bool _cullExcess;
    private float _cullDelay;
    
    private Queue<GameObject> _pool = new Queue<GameObject>();
    private List<GameObject> _activeObjects = new List<GameObject>();
    private HashSet<GameObject> _pooledObjects = new HashSet<GameObject>();
    
    private int _lastCullCheck;
    private bool _isInitialized = false;
    
    /// <summary>
    /// 오브젝트 풀을 초기화합니다.
    /// </summary>
    public void Initialize(GameObject prefab, int initialSize, bool allowExpand = true, int maxSize = 100, bool cullExcess = false, float cullDelay = 60f)
    {
        if (_isInitialized) return;
        
        _prefab = prefab;
        _initialSize = initialSize;
        _allowExpand = allowExpand;
        _maxSize = maxSize;
        _cullExcess = cullExcess;
        _cullDelay = cullDelay;
        
        // 풀 초기화
        for (int i = 0; i < _initialSize; i++)
        {
            CreatePooledObject();
        }
        
        _lastCullCheck = Mathf.FloorToInt(Time.time);
        _isInitialized = true;
        
        // 초과 오브젝트 자동 정리 시작
        if (_cullExcess)
        {
            StartCoroutine(CullExcessRoutine());
        }
    }
    
    /// <summary>
    /// 풀에서 오브젝트를 가져와 활성화합니다.
    /// </summary>
    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        if (_pool.Count == 0 && _allowExpand && _activeObjects.Count < _maxSize)
        {
            ExpandPool();
        }
        
        if (_pool.Count == 0)
        {
            Debug.LogWarning($"풀이 비어있고 확장할 수 없습니다: {_prefab.name}");
            return null;
        }
        
        GameObject obj = _pool.Dequeue();
        
        if (obj == null)
        {
            // 손상된 참조가 있으면 새 오브젝트 생성
            obj = CreatePooledObject();
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                Debug.LogError("풀 확장 후에도 오브젝트를 생성할 수 없습니다.");
                return null;
            }
        }
        
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        
        _activeObjects.Add(obj);
        
        // IPoolable 인터페이스가 있으면 OnSpawn 호출
        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawn();
        
        return obj;
    }
    
    /// <summary>
    /// 오브젝트를 비활성화하고 풀로 반환합니다.
    /// </summary>
    public void Despawn(GameObject obj)
    {
        if (!_pooledObjects.Contains(obj))
        {
            Debug.LogWarning($"이 풀에 속하지 않는 오브젝트입니다: {obj.name}");
            return;
        }
        
        // IPoolable 인터페이스가 있으면 OnDespawn 호출
        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnDespawn();
        
        obj.SetActive(false);
        _activeObjects.Remove(obj);
        _pool.Enqueue(obj);
    }
    
    /// <summary>
    /// 현재 활성화된 모든 오브젝트를 풀로 반환합니다.
    /// </summary>
    public void DespawnAll()
    {
        // 복사본을 만들어 반복
        GameObject[] activeArray = _activeObjects.ToArray();
        
        foreach (var obj in activeArray)
        {
            Despawn(obj);
        }
    }
    
    /// <summary>
    /// 풀에 있는 총 오브젝트 수를 조정합니다.
    /// </summary>
    public void Resize(int newSize)
    {
        int currentTotalSize = _pool.Count + _activeObjects.Count;
        
        if (newSize < _activeObjects.Count)
        {
            Debug.LogWarning($"요청한 크기({newSize})가 현재 활성 오브젝트 수({_activeObjects.Count})보다 작습니다. 활성 오브젝트 수로 조정됩니다.");
            newSize = _activeObjects.Count;
        }
        
        if (newSize > currentTotalSize)
        {
            // 풀 확장
            int numToAdd = newSize - currentTotalSize;
            for (int i = 0; i < numToAdd; i++)
            {
                CreatePooledObject();
            }
        }
        else if (newSize < currentTotalSize)
        {
            // 풀 축소 (비활성 오브젝트만 제거)
            int numToRemove = currentTotalSize - newSize;
            for (int i = 0; i < numToRemove; i++)
            {
                if (_pool.Count > 0)
                {
                    GameObject obj = _pool.Dequeue();
                    _pooledObjects.Remove(obj);
                    Destroy(obj);
                }
                else
                {
                    break;
                }
            }
        }
        
        _maxSize = newSize;
    }
    
    /// <summary>
    /// 풀에 오브젝트를 생성합니다.
    /// </summary>
    private GameObject CreatePooledObject()
    {
        GameObject obj = Instantiate(_prefab, transform);
        obj.SetActive(false);
        _pool.Enqueue(obj);
        _pooledObjects.Add(obj);
        return obj;
    }
    
    /// <summary>
    /// 풀을 확장합니다.
    /// </summary>
    private void ExpandPool()
    {
        int currentSize = _pool.Count + _activeObjects.Count;
        int expandSize = Mathf.Min(_initialSize / 2, _maxSize - currentSize);
        
        if (expandSize <= 0)
        {
            return;
        }
        
        for (int i = 0; i < expandSize; i++)
        {
            CreatePooledObject();
        }
    }
    
    /// <summary>
    /// 풀 통계를 반환합니다.
    /// </summary>
    public (int total, int active, int inactive) GetStats()
    {
        int total = _pool.Count + _activeObjects.Count;
        int active = _activeObjects.Count;
        int inactive = _pool.Count;
        
        return (total, active, inactive);
    }
    
    /// <summary>
    /// 주어진 오브젝트가 이 풀에 속하는지 확인합니다.
    /// </summary>
    public bool BelongsToThisPool(GameObject obj)
    {
        return _pooledObjects.Contains(obj);
    }
    
    /// <summary>
    /// 초과 오브젝트를 정리하는 코루틴
    /// </summary>
    private IEnumerator CullExcessRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_cullDelay);
            
            int currentInactiveCount = _pool.Count;
            int currentActiveCount = _activeObjects.Count;
            int totalCount = currentInactiveCount + currentActiveCount;
            
            // 초기 크기보다 많고, 활성 오브젝트보다 비활성 오브젝트가 많을 때만 정리
            if (totalCount > _initialSize && currentInactiveCount > currentActiveCount)
            {
                int numToRemove = Mathf.Min(currentInactiveCount - currentActiveCount, totalCount - _initialSize);
                
                for (int i = 0; i < numToRemove; i++)
                {
                    if (_pool.Count > 0)
                    {
                        GameObject obj = _pool.Dequeue();
                        _pooledObjects.Remove(obj);
                        Destroy(obj);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}

/// <summary>
/// 풀링된 오브젝트가 구현할 수 있는 인터페이스
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// 오브젝트가 풀에서 꺼내져 활성화될 때 호출됩니다.
    /// </summary>
    void OnSpawn();
    
    /// <summary>
    /// 오브젝트가 풀로 반환될 때 호출됩니다.
    /// </summary>
    void OnDespawn();
}