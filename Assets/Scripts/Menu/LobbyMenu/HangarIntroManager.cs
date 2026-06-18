using UnityEngine;
using UnityEngine.InputSystem;

public class HangarIntroManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private GameObject splashRoot;
    [SerializeField] private GameObject playerController;

    private bool hasStarted = false;

    // 👉 NEW: A master guard variable to block frame-one input leaks
    private bool isAllowedToListen = false;

    void Start()
    {
        hasStarted = false;

        if (playerController != null) playerController.SetActive(false);
        if (splashRoot != null) splashRoot.SetActive(true);
    }

    // 👉 NEW: The transitioner will call this to safely awaken this script
    public void EnableIntroInputListening()
    {
        Debug.Log("[HangarIntro] Master gate opened! Now actively listening for splash transition inputs.");
        isAllowedToListen = true;
    }

    void Update()
    {
        // 👉 FIX: If the transitioner hasn't given the green light, ignore ALL inputs entirely
        if (!isAllowedToListen) return;

        // Fixed: Uses the modern Input System to check if any key/button on any device was pressed
        if (!hasStarted && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            InitializeHangarLobby();
        }
        // Fallback check for gamepads/mouse clicks if a keyboard isn't active
        else if (!hasStarted && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            InitializeHangarLobby();
        }
    }

    private void InitializeHangarLobby()
    {
        hasStarted = true;
        Debug.Log("[HangarIntro] AnyKey detected via InputSystem! Starting transition sequence...");

        if (splashRoot != null)
        {
            splashRoot.SetActive(false);
            Debug.Log("[HangarIntro] Splash UI disabled successfully.");
        }
        else
        {
            Debug.LogError("[HangarIntro] CRITICAL: Splash Root is missing from the Inspector!");
        }

        if (menuController != null)
        {
            Debug.Log("[HangarIntro] Invoking StartHostLobby...");
            menuController.StartHostLobby();
        }
        else
        {
            Debug.LogError("[HangarIntro] MainMenuController dependency is missing!");
        }

        if (playerController != null)
        {
            playerController.SetActive(true);
            Debug.Log("[HangarIntro] Player Controller activated.");
        }
        else
        {
            Debug.LogWarning("[HangarIntro] Player Controller reference is empty. Is FishNet spawning the player dynamically?");
        }
    }
    public void InstantLaunchFromWizard()
    {
        Debug.Log("[HangarIntro] Fast-tracking transition direct from Wizard completion!");
        isAllowedToListen = true;
        InitializeHangarLobby(); // Skip the update loop check and force launch instantly!
    }
}