using FishNet.Object;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Responsible for spawning and replacing the player's active in-game
/// representation (lobby avatar or combat aircraft) based on the current scene.
/// </summary>
public class FactionManifestManager : NetworkBehaviour
{
    #region Inspector

    private const string LogPrefix = "[Representation]";

    [Header("Lobby Settings")]
    [Tooltip("Current player faction.")]
    [SerializeField] private string _currentFaction = Factions.Human;

    [SerializeField] private GameObject _humanLobbyAvatarPrefab;
    [SerializeField] private GameObject _alienLobbyAvatarPrefab;

    [Header("Combat Selection")]
    [Tooltip("Selected combat vehicle.")]
    [SerializeField] private CombatEntityData _selectedCombatVehicle;

    #endregion

    #region Runtime

    /// <summary>
    /// Currently spawned representation owned by this player.
    /// </summary>
    private GameObject _currentRepresentationInstance;

    #endregion

    #region Unity Callbacks

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            return;
        }

        SpawnCurrentRepresentation();
    }

    private void OnDestroy()
    {
        if (!IsServerInitialized || _currentRepresentationInstance == null)
        {
            return;
        }

        ServerManager.Despawn(_currentRepresentationInstance);
    }

    #endregion

    #region Representation Selection

    /// <summary>
    /// Determines which representation should be active and requests the server
    /// to spawn it.
    /// </summary>
    private void SpawnCurrentRepresentation()
    {
        GameObject prefab = GetRepresentationPrefab();

        if (prefab == null)
        {
            return;
        }

        RequestSpawnBodyServer(prefab, transform.position, transform.rotation);
    }

    /// <summary>
    /// Returns the correct representation prefab for the active scene.
    /// </summary>
    private GameObject GetRepresentationPrefab()
    {
        Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string sceneName = activeScene.name;

        if (string.Equals(sceneName, SceneNames.HangarLobby, System.StringComparison.OrdinalIgnoreCase))
        {
            return GetLobbyRepresentation();
        }

        if (string.Equals(sceneName, SceneNames.CombatLobby, System.StringComparison.OrdinalIgnoreCase))
        {
            return GetCombatRepresentation();
        }

        Debug.LogWarning($"{LogPrefix} No representation defined for scene '{sceneName}'.");

        return null;
    }

    /// <summary>
    /// Returns the appropriate lobby avatar based on the current faction.
    /// </summary>
    private GameObject GetLobbyRepresentation()
    {
        bool isHuman = string.Equals(
            _currentFaction,
            Factions.Human,
            System.StringComparison.OrdinalIgnoreCase);

        return isHuman
            ? _humanLobbyAvatarPrefab
            : _alienLobbyAvatarPrefab;
    }

    /// <summary>
    /// Returns the currently selected combat vehicle prefab.
    /// </summary>
    private GameObject GetCombatRepresentation()
    {
        if (_selectedCombatVehicle == null)
        {
            Debug.LogWarning($"{LogPrefix} No combat vehicle selected.");
            return null;
        }

        if (_selectedCombatVehicle.entityPrefab == null)
        {
            Debug.LogWarning($"{LogPrefix} '{_selectedCombatVehicle.entityName}' has no prefab assigned.");
            return null;
        }

        Debug.Log($"{LogPrefix} Preparing '{_selectedCombatVehicle.entityName}'.");

        return _selectedCombatVehicle.entityPrefab;
    }

    #endregion

    #region Networking

    /// <summary>
    /// Replaces the player's current representation on the server.
    /// </summary>
    [ServerRpc]
    private void RequestSpawnBodyServer(GameObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (_currentRepresentationInstance != null)
        {
            ServerManager.Despawn(_currentRepresentationInstance);
        }

        _currentRepresentationInstance = Instantiate(prefab, spawnPosition, spawnRotation);

        Spawn(_currentRepresentationInstance, Owner);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Updates the player's selected combat vehicle and synchronizes the
    /// player's faction with the vehicle.
    /// </summary>
    public void SetSelectedVehicle(CombatEntityData newVehicleData)
    {
        if (newVehicleData == null)
        {
            Debug.LogError($"{LogPrefix} Attempted to assign a null CombatEntityData.");
            return;
        }

        _selectedCombatVehicle = newVehicleData;
        _currentFaction = newVehicleData.faction;

        Debug.Log($"{LogPrefix} Selected vehicle: {_selectedCombatVehicle.entityName} | Faction: {_currentFaction}");
    }

    #endregion
}