using System.Collections;
using UnityEngine;

public class InputWizardTransitioner : MonoBehaviour
{
    public static InputWizardTransitioner Instance { get; private set; }

    [Header("Core Setup Step")]
    [SerializeField] private InputWizardManager _inputWizard;
    
    [Header("Core System Toggles")]
    [SerializeField] private HangarIntroManager _hangarIntroManager;
    [SerializeField] private LobbyUIManager _lobbyUiManager;
    [SerializeField] private MainMenuController _mainMenuController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        #if UNITY_EDITOR
        PlayerPrefs.DeleteKey("HasCompletedInputWizard");
        PlayerPrefs.Save();
        #endif
    }

    private IEnumerator Start()
    {
        // --- PHASE 1: LOCK DOWN SYSTEM MANAGERS ---
        SetGameplaySystemsActive(false);

        if (_inputWizard != null) _inputWizard.gameObject.SetActive(true);

        // --- PHASE 2: EVALUATE WIZARD REQUIREMENT ---
        bool needsControlSetup = PlayerPrefs.GetInt("HasCompletedInputWizard", 0) == 0;
        bool ranWizardThisSession = false;

        if (needsControlSetup && _inputWizard != null)
        {
            Debug.Log("[Transitioner] Control mapping missing. Deploying Input Wizard.");
            ranWizardThisSession = true;
            yield return new WaitUntil(() => _inputWizard.IsWizardComplete);
        }
        
        if (_inputWizard != null) _inputWizard.gameObject.SetActive(false);

        // --- PHASE 3: THE HANDOVER GATES ---
        Debug.Log("[Transitioner] Handing control over to gameplay systems.");
        SetGameplaySystemsActive(true);

        if (_hangarIntroManager != null)
        {
            if (ranWizardThisSession)
            {
                // 👉 FIX: Don't make them press another button. Drop them straight into the simulation!
                _hangarIntroManager.InstantLaunchFromWizard();
            }
            else
            {
                // Normal launch: Open the input check gate and let them press any key on the splash screen
                _hangarIntroManager.EnableIntroInputListening();
            }
        }
    }

    private void SetGameplaySystemsActive(bool isActive)
    {
        if (_hangarIntroManager != null) _hangarIntroManager.gameObject.SetActive(isActive);
        if (_lobbyUiManager != null) _lobbyUiManager.gameObject.SetActive(isActive);
        if (_mainMenuController != null) _mainMenuController.gameObject.SetActive(isActive);
    }
}