using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Added for the New Input System package

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject _escMenuPanel;

    [Header("Explicit Teleport Waypoints")]
    [SerializeField] private Transform _missionControlWaypoint; 
    [SerializeField] private Transform _settingsAreaWaypoint;    
    [SerializeField] private Transform _traitorCornerWaypoint;   

    private bool _isMenuOpen = false;
    private LobbyAvatarController _localPlayerController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Safety check: Ensure a keyboard device is plugged in/active before checking keys
        if (Keyboard.current == null) return;

        // New Input System equivalent of legacy Input.GetKeyDown(KeyCode.Escape)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // DIAGNOSTIC LOG: This will tell us exactly where the chain is breaking
            Debug.Log($"[ESC Diagnostic] Key pressed! Panel Assigned: {_escMenuPanel != null} | Local Player Found: {_localPlayerController != null}");
            
            ToggleESCMenu();
        }
    }

    // This allows our NetworkPlayerSetup to register the local player instance when it spawns
    public void RegisterLocalPlayer(LobbyAvatarController controller)
    {
        _localPlayerController = controller;
        Debug.Log($"[LobbyUI] Local player successfully registered! Reference is: {controller.gameObject.name}");
    }

    public void ToggleESCMenu()
    {
        // If this hits, the menu will silently fail to open
        if (_localPlayerController == null)
        {
            Debug.LogWarning("[LobbyUI] Cannot toggle menu: No Local Player Controller has been registered yet!");
            return;
        }

        if (_escMenuPanel == null)
        {
            Debug.LogError("[LobbyUI] Cannot toggle menu: The ESC Menu Panel slot is empty in the Inspector!");
            return;
        }

        _isMenuOpen = !_isMenuOpen;
        _escMenuPanel.SetActive(_isMenuOpen);

        if (_isMenuOpen)
        {
            // Open Menu: Release the mouse and tell the controller to pause input tracking
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _localPlayerController.SetInputLock(true);
            Debug.Log("[LobbyUI] Menu opened. Inputs locked, cursor freed.");
        }
        else
        {
            // Close Menu: Lock the mouse back down and wake up player input tracking
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _localPlayerController.SetInputLock(false);
            Debug.Log("[LobbyUI] Menu closed. Inputs resumed, cursor locked.");
        }
    }

    // ==========================================
    // EXPLICIT BUTTONS (Mapped to your new design)
    // ==========================================

    public void OnClickTeleportMissionControl()
    {
        if (_localPlayerController != null && _missionControlWaypoint != null)
        {
            _localPlayerController.TeleportTo(_missionControlWaypoint.position);
            ToggleESCMenu();
        }
    }

    public void OnClickTeleportSettingsArea()
    {
        if (_localPlayerController != null && _settingsAreaWaypoint != null)
        {
            _localPlayerController.TeleportTo(_settingsAreaWaypoint.position);
            ToggleESCMenu();
        }
    }

    public void OnClickTeleportTraitorCorner()
    {
        if (_localPlayerController != null && _traitorCornerWaypoint != null)
        {
            _localPlayerController.TeleportTo(_traitorCornerWaypoint.position);
            ToggleESCMenu();
        }
    }

    // ==========================================
    // MODULAR BACKUP METHOD
    // ==========================================
    public void OnClickUniversalTeleport(Transform targetWaypoint)
    {
        if (_localPlayerController != null && targetWaypoint != null)
        {
            _localPlayerController.TeleportTo(targetWaypoint.position);
            ToggleESCMenu();
            Debug.Log($"[UI] Teleported local player to: {targetWaypoint.name}");
        }
    }

    public void OnClickExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}