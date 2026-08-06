using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the lobby user interface, including the ESC menu,
/// cursor state, and player input locking.
/// </summary>
public class LobbyUIManager : MonoBehaviour
{
    #region Singleton

    public static LobbyUIManager Instance { get; private set; }

    #endregion

    #region Inspector

    [Header("UI Panels")]
    [SerializeField] private GameObject _escMenuPanel;

    #endregion

    #region Runtime

    private bool _isMenuOpen;
    private LobbyAvatarController _localPlayerController;

    #endregion

    #region Unity Events

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleESCMenu();
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Registers the local player's controller after it has been spawned.
    /// </summary>
    public void RegisterLocalPlayer(LobbyAvatarController controller)
    {
        _localPlayerController = controller;

        Debug.Log($"[LobbyUI] Registered local player: {controller.gameObject.name}");
    }

    /// <summary>
    /// Opens or closes the ESC menu.
    /// </summary>
    public void ToggleESCMenu()
    {
        if (!ValidateMenuState())
            return;

        _isMenuOpen = !_isMenuOpen;

        _escMenuPanel.SetActive(_isMenuOpen);

        UpdateCursorState(_isMenuOpen);
        _localPlayerController.SetInputLock(_isMenuOpen);

        Debug.Log(_isMenuOpen
            ? "[LobbyUI] ESC menu opened."
            : "[LobbyUI] ESC menu closed.");
    }

    public bool IsMenuOpen => _isMenuOpen;

    #endregion

    #region Helpers

    private bool ValidateMenuState()
    {
        if (_localPlayerController == null)
        {
            Debug.LogWarning("[LobbyUI] Local player has not been registered.");
            return false;
        }

        if (_escMenuPanel == null)
        {
            Debug.LogError("[LobbyUI] ESC Menu Panel reference is missing.");
            return false;
        }

        return true;
    }

    private static void UpdateCursorState(bool menuOpen)
    {
        Cursor.lockState = menuOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = menuOpen;
    }

    #endregion

    #region UI Buttons

    public void OnClickExitGame()
    {
        Debug.Log("[LobbyUI] Exiting application.");
        Application.Quit();
    }

    #endregion
}