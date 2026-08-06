using UnityEngine;

/// <summary>
/// Handles player teleportation between predefined lobby locations.
/// </summary>
public class LobbyTeleportManager : MonoBehaviour
{
    [Header("Waypoints")]

    [SerializeField] private Transform _missionControlWaypoint;
    [SerializeField] private Transform _settingsAreaWaypoint;
    [SerializeField] private Transform _traitorCornerWaypoint;

    private LobbyAvatarController LocalPlayer =>
        LobbyUIManager.Instance == null
            ? null
            : typeof(LobbyUIManager)
                .GetField("_localPlayerController",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.GetValue(LobbyUIManager.Instance) as LobbyAvatarController;

    #region Button Events

    public void OnClickMissionControl()
    {
        Teleport(_missionControlWaypoint);
    }

    public void OnClickSettingsArea()
    {
        Teleport(_settingsAreaWaypoint);
    }

    public void OnClickTraitorCorner()
    {
        Teleport(_traitorCornerWaypoint);
    }

    public void OnClickUniversalTeleport(Transform targetWaypoint)
    {
        Teleport(targetWaypoint);
    }

    #endregion

    #region Helpers

    private void Teleport(Transform destination)
    {
        if (LocalPlayer == null || destination == null)
            return;

        LocalPlayer.TeleportTo(destination.position);

        LobbyUIManager.Instance.ToggleESCMenu();

        Debug.Log($"[Teleport] Player moved to '{destination.name}'.");
    }

    #endregion
}