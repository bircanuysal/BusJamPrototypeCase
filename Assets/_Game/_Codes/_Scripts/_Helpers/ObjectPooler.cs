using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PoolableObjectTypes
{
    None,
}
[System.Serializable]
public struct Pool
{
    public PoolableObjectTypes type;
    public bool isSpecificForMode;
    public GameObject prefab;
    public int size;
}
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    public List<Pool> pools = new();
    private Dictionary<PoolableObjectTypes, HashSet<GameObject>> poolDictionary = new();

    private void Awake()
    {
        Instance = this;

        foreach (var pool in pools)
        {
            HashSet<GameObject> objectPool = new();
            poolDictionary.Add(pool.type, objectPool);

            if (pool.isSpecificForMode)
            {
                continue;
            }

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Add(obj);
            }
        }
    } 
    public GameObject GetPooledObject(PoolableObjectTypes type, bool setObjActive = true)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning("Pool with type " + type + " doesn't exist.");
            return null;
        }

        GameObject objToReturn = null;

        if (poolDictionary[type].Count == 0)
        {
            Debug.LogWarning("Pool with type " + type + " is empty.");
            OnPoolIsEmpty(type);
        }

        objToReturn = poolDictionary[type].SelectAndRemove();

        while (objToReturn == null)
        {
            if (poolDictionary[type].Count == 0)
            {
                OnPoolIsEmpty(type);
            }

            objToReturn = poolDictionary[type].SelectAndRemove();
        }

        objToReturn.SetActive(setObjActive);
        return objToReturn;
    }

    private GameObject OnPoolIsEmpty(PoolableObjectTypes type)
    {
        GameObject objToReturn = null;

        for (int i = 0; i < pools.Count; i++)
        {
            if (pools[i].type == type)
            {
                for (int j = 0; j < 10; j++)
                {
                    objToReturn = Instantiate(pools[i].prefab);
                    objToReturn.SetActive(false);
                    // Add the new object to the pool
                    poolDictionary[type].Add(objToReturn);
                }
                break;
            }
        }
        return objToReturn;
    }

    public GameObject GetPooledObject(PoolableObjectTypes type, Transform parentObj, bool setObjActive = true)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning("Pool with type " + type + " doesn't exist.");
            return null;
        }

        GameObject objToReturn;

        if (poolDictionary[type].Count == 0)
        {
            objToReturn = OnPoolIsEmpty(type);
        }

        objToReturn = poolDictionary[type].SelectAndRemove();

        while (objToReturn == null)
        {
            if (poolDictionary[type].Count == 0)
            {
                OnPoolIsEmpty(type);
            }

            objToReturn = poolDictionary[type].SelectAndRemove();
        }


        objToReturn.transform.SetParent(parentObj);
        objToReturn.transform.localPosition = Vector3.zero;
        objToReturn.transform.localEulerAngles = Vector3.zero;
        objToReturn.SetActive(setObjActive);

        return objToReturn;
    }
    public void ReturnToPool(GameObject objToReturn)
    {
        if (objToReturn == null) return;

        if (!objToReturn.activeSelf) { return; }

        PoolableObjectTypes type = PoolableObjectTypes.None;

        if (objToReturn.TryGetComponent(out IObjectPoolable objectPoolable))
        {
            type = objectPoolable.PoolableObjectType();
        }

        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning("Trying to return not pooled object to pool " + objToReturn.name);
            return;
        }

        if (poolDictionary[type].Add(objToReturn))
        {
            objectPoolable.OnReturnToPool();
            objToReturn.SetActive(false);
        }
    }
    public void ReturntoPoolDelayed(GameObject objToReturn, float delay)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            ReturnToPool(objToReturn);
        });
    }
}
