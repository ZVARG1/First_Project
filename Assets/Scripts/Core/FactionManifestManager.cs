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
        DetermineActiveRepresentation();
    }

    private void DetermineActiveRepresentation()
    {
        // Always clean up old remnants before instantiating a new body asset
        if (_currentActiveBody != null) Destroy(_currentActiveBody);

        Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string activeSceneName = activeScene.name;
        GameObject prefabToSpawn = null;

        // 1. Check for the Hangar / Social Hub Scene
        if (string.Equals(activeSceneName, "Scene_MainMenu", System.StringComparison.OrdinalIgnoreCase))
        {
            // Case-insensitive string matching prevents minor string-entry layout typos from breaking spawn logic
            bool isHuman = string.Equals(_currentFaction, "Human", System.StringComparison.OrdinalIgnoreCase);
            prefabToSpawn = isHuman ? _humanLobbyAvatarPrefab : _alienLobbyAvatarPrefab;
        }
        // 2. Check for the Active Map Dogfight Match Scene
        else if (string.Equals(activeSceneName, "Scene_Main", System.StringComparison.OrdinalIgnoreCase))
        {
            if (_selectedCombatVehicle != null && _selectedCombatVehicle.entityPrefab != null)
            {
                prefabToSpawn = _selectedCombatVehicle.entityPrefab;
                Debug.Log($"[SpawnSystem] Loading chosen vehicle: {_selectedCombatVehicle.entityName}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] No valid combat vehicle ScriptableObject selected for this player sequence!");
            }
        }

        // 3. Final Deployment Handshake
        if (prefabToSpawn != null)
        {
            _currentActiveBody = Instantiate(prefabToSpawn, transform.position, transform.rotation, transform);
        }
    }

    /// <summary>
    /// Public API for your hangar terminal interface to swap vehicle profiles and update target assets dynamically.
    /// </summary>
    public void SetSelectedVehicle(CombatEntityData newVehicleData)
    {
        if (newVehicleData == null)
        {
            Debug.LogError($"[{gameObject.name}] SetSelectedVehicle called with a null data payload!");
            return;
        }

        _selectedCombatVehicle = newVehicleData;
        _currentFaction = newVehicleData.faction; // Sync faction alignment string automatically from the ScriptableObject
        
        Debug.Log($"[Manifest] Updated selection payload to: {_selectedCombatVehicle.entityName} aligned with faction: {_currentFaction}");
    }
}