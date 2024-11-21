using System.Collections.Generic;
using UnityEngine;
 
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance{get;private set;}
    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int initialSize = 10;
    }
 
    public List<Pool> pools;
    public bool expandable = true;
 
    private Dictionary<GameObject, Stack<GameObject>> poolDictionary;
 
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        poolDictionary = new Dictionary<GameObject, Stack<GameObject>>();
 
        foreach (Pool pool in pools)
        {
            Stack<GameObject> objectStack = new Stack<GameObject>();
 
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectStack.Push(obj);
            }
 
            poolDictionary.Add(pool.prefab, objectStack);
        }
    }
 
    public GameObject GetObject(GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogError("Prefab not found in pool dictionary: " + prefab.name);
            return null;
        }
 
        Stack<GameObject> stack = poolDictionary[prefab];
 
        if (stack.Count > 0)
        {
            GameObject obj = stack.Pop();
            obj.SetActive(true);
            return obj;
        }
 
        if (expandable)
        {
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(true);
            return newObj;
        }
 
        return null; // or handle as necessary
    }

    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion quaternion)
    {
        GameObject @object = GetObject(prefab);
        @object.transform.position = position;
        @object.transform.rotation = quaternion;
        return @object;
    }

    public GameObject GetObject(GameObject prefab, Transform parent)
    {
        GameObject @object = GetObject(prefab);
        @object.transform.SetParent(parent);
        return @object;
    }
 
    public void ReturnObject(GameObject obj)
    {
        obj.transform.SetParent(null);
        obj.SetActive(false);
 
        foreach (var pool in pools)
        {
            if (obj.CompareTag(pool.prefab.tag))
            {
                poolDictionary[pool.prefab].Push(obj);
                return;
            }
        }
 
        Debug.LogError("Returned object does not match any pool prefab.");
    }

    public void ReturnObject(GameObject prefab, GameObject obj)
    {
        obj.transform.SetParent(null);
        obj.SetActive(false);
 
        if(poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab].Push(obj);
            return;
        }
 
        Debug.LogError("Returned object does not match any pool prefab.");
    }
}