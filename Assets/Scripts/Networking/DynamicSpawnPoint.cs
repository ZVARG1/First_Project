using UnityEngine;
using FishNet.Component.Spawning;
using System.Linq;

public class DynamicSpawnPoint : MonoBehaviour
{
    void Start()
    {
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();

        if (spawner != null)
        {
            // Register these coordinates to FishNet dynamically
            var spawnList = spawner.Spawns.ToList();
            spawnList.Add(transform);
            spawner.Spawns = spawnList.ToArray();
            
            Debug.Log($"[SpawnSystem] Registered {gameObject.name} as valid coordinates.");
        }
    }

    private void OnDestroy()
    {
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        if (spawner != null && spawner.Spawns != null)
        {
            var spawnList = spawner.Spawns.ToList();
            if (spawnList.Contains(transform))
            {
                spawnList.Remove(transform);
                spawner.Spawns = spawnList.ToArray();
            }
        }
    }
}