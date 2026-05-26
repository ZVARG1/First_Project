using UnityEngine;

public class HangarIntroManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private GameObject splashRoot; // The transparent UI overlay text object
    [SerializeField] private GameObject playerController; // Your local movement script / GameObject

    private bool hasStarted = false;

    void Start()
    {
        hasStarted = false;
        
        // Ensure baseline state at boot
        if (playerController != null) playerController.SetActive(false);
        if (splashRoot != null) splashRoot.SetActive(true);
    }

    void Update()
    {
        // Detect input trigger safely
        if (!hasStarted && Input.anyKeyDown)
        {
            InitializeHangarLobby();
        }
    }

    private void InitializeHangarLobby()
    {
        hasStarted = true;
        Debug.Log("[HangarIntro] AnyKey detected! Starting transition sequence...");

        // 1. Force the UI away FIRST so the player never gets stuck staring at a frozen screen
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