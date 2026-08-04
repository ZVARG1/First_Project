using UnityEngine;
using FishNet.Object;
using UnityEngine.SceneManagement;

public class FactionManifestManager : NetworkBehaviour
{
    [Header("Lobby Settings")]
    [Tooltip("The default faction string used on initialization (e.g., 'Human' or 'Alien')")]
    [SerializeField] private string _currentFaction = "Human";
    [SerializeField] private GameObject _humanLobbyAvatarPrefab;
    [SerializeField] private GameObject _alienLobbyAvatarPrefab;

    [Header("Active Combat Selection")]
    [Tooltip("This will be set dynamically via your UI ship-selection screen later")]
    [SerializeField] private CombatEntityData _selectedCombatVehicle;

    private GameObject _currentActiveBody;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // Only the local player who owns this invisible manager should trigger the spawn request!
        if (IsOwner)
        {
            DetermineActiveRepresentation();
        }
    }

    private void DetermineActiveRepresentation()
    {
        Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string activeSceneName = activeScene.name;
        GameObject prefabToSpawn = null;

        // 1. Check for the Hangar / Social Hub Scene
        if (string.Equals(activeSceneName, "Scene_HangarLobby", System.StringComparison.OrdinalIgnoreCase))
        {
            bool isHuman = string.Equals(_currentFaction, "Human", System.StringComparison.OrdinalIgnoreCase);
            prefabToSpawn = isHuman ? _humanLobbyAvatarPrefab : _alienLobbyAvatarPrefab;
        }
        // 2. Check for the Active Map Dogfight Match Scene
        else if (string.Equals(activeSceneName, "Scene_CombatLobby", System.StringComparison.OrdinalIgnoreCase))
        {
            if (_selectedCombatVehicle != null && _selectedCombatVehicle.entityPrefab != null)
            {
                prefabToSpawn = _selectedCombatVehicle.entityPrefab;
                Debug.Log($"[SpawnSystem] Preparing chosen vehicle payload: {_selectedCombatVehicle.entityName}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] No valid combat vehicle ScriptableObject selected!");
            }
        }

        // 3. Request Final Deployment from the Server
        if (prefabToSpawn != null)
        {
            // Instead of local Instantiate(), we pass the object reference to our server request function
            RequestSpawnBodyServer(prefabToSpawn, transform.position, transform.rotation);
        }
    }

    [ServerRpc]
    private void RequestSpawnBodyServer(GameObject prefab, Vector3 spawnPos, Quaternion spawnRot)
    {
        // A. If this connection already has an active body spawned on the server, clean it up first
        if (_currentActiveBody != null)
        {
            ServerManager.Despawn(_currentActiveBody);
        }

        // B. Instantiate the object on the server inside world space (no structural parenting!)
        _currentActiveBody = Instantiate(prefab, spawnPos, spawnRot);

        // C. Officially spawn it across the entire Steam tunnel network, making it visible to everyone
        // and assigning network ownership back to the client who requested it!
        Spawn(_currentActiveBody, Owner);
    }

    public void SetSelectedVehicle(CombatEntityData newVehicleData)
    {
        if (newVehicleData == null)
        {
            Debug.LogError($"[{gameObject.name}] SetSelectedVehicle called with a null data payload!");
            return;
        }

        _selectedCombatVehicle = newVehicleData;
        _currentFaction = newVehicleData.faction; 
        
        Debug.Log($"[Manifest] Updated selection payload to: {_selectedCombatVehicle.entityName} aligned with faction: {_currentFaction}");
    }

    private void OnDestroy()
    {
        // Server cleanup safety guard: if the player disconnects, make sure their physical avatar body is destroyed too
        if (IsServer && _currentActiveBody != null)
        {
            ServerManager.Despawn(_currentActiveBody);
        }
    }
}