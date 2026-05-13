using UnityEngine;

public class HangarIntroManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private GameObject splashRoot; // The UI text object
    [SerializeField] private GameObject playerController; // Your movement script

    [Header("Standard Cameras")]
    [SerializeField] private Camera introCamera; // The cinematic static camera
    [SerializeField] private Camera playerCamera; // The camera attached to your player

    private bool hasStarted = false;

    void Start()
    {
        // 1. Initial State: Intro Cam on, Player Cam off
        hasStarted = false;
        
        introCamera.enabled = true;
        playerCamera.enabled = false;
        
        playerController.SetActive(false);
        splashRoot.SetActive(true);
    }

    void Update()
    {
        // 2. The "Any Key" Trigger
        if (!hasStarted && Input.anyKeyDown)
        {
            InitializeHangar();
        }
    }

    private void InitializeHangar()
    {
        hasStarted = true;

        // 3. Networking: Create the Lobby
        menuController.HostButtonPressed();

        // 4. Manual Camera Swap
        introCamera.enabled = false;
        playerCamera.enabled = true;

        // 5. World: Remove UI and enable player movement
        splashRoot.SetActive(false);
        playerController.SetActive(true);
    }
}