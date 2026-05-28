using UnityEngine;
using FishNet.Object;
using UnityEngine.SceneManagement;

public class FactionManifestManager : NetworkBehaviour
{
    [Header("Lobby Settings")]
    [SerializeField] private FactionType _currentFaction = FactionType.Human;
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
        if (_currentActiveBody != null) Destroy(_currentActiveBody);

        Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string activeSceneName = activeScene.name;
        GameObject prefabToSpawn = null;

        if (activeSceneName == "Scene_MainMenu")
        {
            // Pick walking avatar based on faction
            prefabToSpawn = (_currentFaction == FactionType.Human) ? _humanLobbyAvatarPrefab : _alienLobbyAvatarPrefab;
        }
        else if (activeSceneName == "Scene_Main")
        {
            // Pick the specific craft prefab defined in the ScriptableObject asset card
            if (_selectedCombatVehicle != null && _selectedCombatVehicle.entityPrefab != null)
            {
                prefabToSpawn = _selectedCombatVehicle.entityPrefab;
                Debug.Log($"[SpawnSystem] Loading chosen vehicle: {_selectedCombatVehicle.entityName}");
            }
            else
            {
                Debug.LogWarning("[SpawnSystem] No combat vehicle ScriptableObject selected for this player!");
            }
        }

        if (prefabToSpawn != null)
        {
            _currentActiveBody = Instantiate(prefabToSpawn, transform.position, transform.rotation, transform);
        }
    }

    // Public method your UI or Lobby Selection screen can call to change ships before the match starts
    public void SetSelectedVehicle(CombatEntityData newVehicleData)
    {
        _selectedCombatVehicle = newVehicleData;
        _currentFaction = newVehicleData.faction; // Sync faction alignment automatically
    }
}