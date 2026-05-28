using UnityEngine;
using UnityEngine.InputSystem; // NEW: Required for modern input parsing

public class HangarIntroManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private GameObject splashRoot; 
    [SerializeField] private GameObject playerController; 

    private bool hasStarted = false;

    void Start()
    {
        hasStarted = false;
        
        if (playerController != null) playerController.SetActive(false);
        if (splashRoot != null) splashRoot.SetActive(true);
    }

    void Update()
    {
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

        // 1. Force the UI away FIRST
        if (splashRoot != null)
        {
            splashRoot.SetActive(false);
            Debug.Log("[HangarIntro] Splash UI disabled successfully.");
        }
        else
        {
            Debug.LogError("[HangarIntro] CRITICAL: Splash Root is missing from the Inspector!");
        }

        // 2. Networking Handshake
        if (menuController != null)
        {
            Debug.Log("[HangarIntro] Invoking StartHostLobby...");
            menuController.StartHostLobby();
        }
        else
        {
            Debug.LogError("[HangarIntro] MainMenuController dependency is missing!");
        }

        // 3. Unlock Character Movement Safely
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
}