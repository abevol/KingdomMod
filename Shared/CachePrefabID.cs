using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KingdomMod;

public class CachePrefabID
{
    private Dictionary<int, List<GameObject>> _prefabIdCache = new Dictionary<int, List<GameObject>>();

    public void CachePrefabIDs()
    {
        _prefabIdCache.Clear();

        PrefabID[] allPrefabIDs = Object.FindObjectsByType<PrefabID>(FindObjectsSortMode.None);
        foreach (PrefabID prefab in allPrefabIDs)
        {
            int id = prefab.prefabID;
            if (!_prefabIdCache.ContainsKey(id))
            {
                _prefabIdCache[id] = new List<GameObject>();
            }

            _prefabIdCache[id].Add(prefab.gameObject);
        }
    }

    public List<GameObject> FindGameObjects(int targetPrefabID)
    {
        if (_prefabIdCache.ContainsKey(targetPrefabID))
        {
            return _prefabIdCache[targetPrefabID];
        }

        return null;
    }

    public GameObject FindGameObject(int targetPrefabID)
    {
        if (_prefabIdCache.ContainsKey(targetPrefabID))
        {
            return _prefabIdCache[targetPrefabID].FirstOrDefault();
        }

        return null;
    }

}