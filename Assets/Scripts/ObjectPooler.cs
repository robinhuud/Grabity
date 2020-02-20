using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    [SerializeField]
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private Dictionary<string, GameObject> prefabDictionary;

    // Fake singleton
    public static ObjectPooler Instance;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>();
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for(int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            prefabDictionary.Add(pool.tag, pool.prefab);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary.ContainsKey(tag))
        {
            if(poolDictionary[tag].Count == 0)
            {
                GameObject obj = Instantiate(prefabDictionary[tag]);
                obj.SetActive(false);
                poolDictionary[tag].Enqueue(obj);
            }
            GameObject objToSpawn = poolDictionary[tag].Dequeue();

            objToSpawn.SetActive(true);
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;

            IPooledObject pooledObj = objToSpawn.GetComponent<IPooledObject>();

            if(pooledObj != null)
            {
                pooledObj.OnObjectSpawn();
            }

            return objToSpawn;
        } else
        {
            return null;
        }
    }

    public void ReleaseFromPool(string tag, ref GameObject escapee)
    {
        escapee.GetComponent<IPooledObject>().OnObjectDespawn();
        escapee.SetActive(false);
        poolDictionary[tag].Enqueue(escapee);
    }
}
