using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InputWizardManager : MonoBehaviour
{
    [Header("UI Component Links")]
    [SerializeField] private TextMeshProUGUI _displayText;
    [SerializeField] private Image _backgroundImage;

    [Header("Master Panel Control (Fading)")]
    [SerializeField] private CanvasGroup _navigationCanvasGroup;

    [Header("Universal Font States")]
    [SerializeField] private TMP_FontAsset _baselineFont;
    [SerializeField] private TMP_FontAsset _glitchFont;
    [SerializeField] private TMP_FontAsset _decryptedFont;

    [Header("Visual Resources")]
    [SerializeField] private Sprite _humanHangarBackground;
    [SerializeField] private Sprite _alienGlitchBackground;

    [Header("Interactive Pages (1 to 3)")]
    [SerializeField] private GameObject[] _interactivePages;

    [Header("Navigation Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _nextButton;

    [Header("Timing Configurations")]
    [SerializeField] private float _typeSpeed = 0.05f;
    [SerializeField] private float _buttonFadeDuration = 1.0f;

    [SerializeField] private InputActionAsset playerInputActions;

    // 👉 THE FIX: Added public property so InputWizardTransitioner can track the state
    public bool IsWizardComplete { get; private set; } = false;

    private int _currentPageIndex = 0;
    private bool _isIntroRunning = false;
    private string _coreIntroText = "Welcome back, pilot...";

    private void Start()
    {
        // 🛠️ CLEANUP: Removed PlayerPrefs checks and rapid reset logic here.
        // InputWizardTransitioner is completely managing activation/deactivation now.
        InitializeWizard();
    }
    private void Awake()
    {
        // 👉 ADD THESE LINES: Restores their personal setup immediately on boot
        if (playerInputActions != null)
        {
            InputSaveManager.LoadBindings(playerInputActions);
        }
        else
        {
            Debug.LogError("[Wizard] Player Input Actions asset is missing from the Inspector!");
        }
    }

    private void InitializeWizard()
    {
        _currentPageIndex = 0;
        IsWizardComplete = false; // Reset state on bootup

        foreach (GameObject page in _interactivePages)
        {
            if (page != null) page.SetActive(false);
        }

        // Drop the master panel alpha to zero immediately
        if (_navigationCanvasGroup != null)
        {
            _navigationCanvasGroup.alpha = 0f;
            _navigationCanvasGroup.interactable = false;
            _navigationCanvasGroup.blocksRaycasts = false;
        }

        // Ensure both GameObjects are physically active in the hierarchy so the layout works
        if (_backButton != null) _backButton.gameObject.SetActive(true);
        if (_nextButton != null) _nextButton.gameObject.SetActive(true);

        // Apply distinct visual visibility rule to the Back button components right away
        SetBackButtonVisualState(false);

        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator RunIntroSequence()
    {
        _isIntroRunning = true;
        _displayText.text = "";

        // --- STEP 1: Baseline UI Layer ---
        _displayText.font = _baselineFont;
        _backgroundImage.sprite = _humanHangarBackground;
        _backgroundImage.color = Color.white;

        foreach (char letter in _coreIntroText.ToCharArray())
        {
            _displayText.text += letter;
            yield return new WaitForSeconds(_typeSpeed);
        }

        float typedTime = _coreIntroText.Length * _typeSpeed;
        float remainingHumanTime = Mathf.Max(0f, 3.0f - typedTime);
        yield return new WaitForSeconds(remainingHumanTime);

        // --- STEP 2: The Glitch Loop ---
        SetInterfaceState(_glitchFont, _alienGlitchBackground, new Color(1f, 1f, 1f, 0.7f));
        yield return new WaitForSeconds(0.25f);

        SetInterfaceState(_baselineFont, _humanHangarBackground, Color.white);
        yield return new WaitForSeconds(0.25f);

        SetInterfaceState(_glitchFont, _alienGlitchBackground, new Color(1f, 1f, 1f, 0.8f));
        yield return new WaitForSeconds(0.25f);

        SetInterfaceState(_baselineFont, _humanHangarBackground, Color.white);
        yield return new WaitForSeconds(0.25f);

        // --- STEP 3: Decrypted State Override ---
        SetInterfaceState(_decryptedFont, _humanHangarBackground, Color.white);

        // --- STEP 4: Smooth Master Layout Fade-In ---
        if (_navigationCanvasGroup != null)
        {
            float counter = 0f;
            while (counter < _buttonFadeDuration)
            {
                counter += Time.deltaTime;
                _navigationCanvasGroup.alpha = Mathf.Lerp(0f, 1f, counter / _buttonFadeDuration);
                yield return null;
            }

            _navigationCanvasGroup.alpha = 1f;
            _navigationCanvasGroup.interactable = true;
            _navigationCanvasGroup.blocksRaycasts = true;
        }

        // Force explicit evaluation of page rules now that panels are visible
        UpdatePageVisibility();

        _isIntroRunning = false;
    }

    private void SetInterfaceState(TMP_FontAsset activeFont, Sprite background, Color bgColor)
    {
        if (activeFont != null) _displayText.font = activeFont;
        if (background != null) _backgroundImage.sprite = background;
        _backgroundImage.color = bgColor;
        _displayText.UpdateFontAsset();
    }

    public void OnClickNext()
    {
        if (_isIntroRunning) return;

        if (_currentPageIndex < _interactivePages.Length)
        {
            _currentPageIndex++;
            UpdatePageVisibility();
        }
        else
        {
            FinishWizard();
        }
    }

    public void OnClickBack()
    {
        if (_isIntroRunning || _currentPageIndex <= 1) return;

        _currentPageIndex--;
        UpdatePageVisibility();
    }

    private void UpdatePageVisibility()
    {
        _displayText.gameObject.SetActive(_currentPageIndex == 0);

        for (int i = 0; i < _interactivePages.Length; i++)
        {
            if (_interactivePages[i] != null)
            {
                _interactivePages[i].SetActive((i + 1) == _currentPageIndex);
            }
        }

        // Control the back button using specific functional rendering toggles instead of layout breakers
        bool shouldShowBack = _currentPageIndex > 1;
        SetBackButtonVisualState(shouldShowBack);
    }

    private void SetBackButtonVisualState(bool isVisible)
    {
        if (_backButton == null) return;

        // Toggle interactivity
        _backButton.enabled = isVisible;

        // Toggle graphic rendering components so the layout placeholder remains perfectly stable
        if (_backButton.TryGetComponent<Image>(out var btnImage))
        {
            btnImage.enabled = isVisible;
        }

        // Toggle any text inside the button container completely off/on
        if (_backButton.TryGetComponent<CanvasRenderer>(out var renderer))
        {
            renderer.cull = !isVisible;
        }

        foreach (var textComponent in _backButton.GetComponentsInChildren<TextMeshProUGUI>())
        {
            textComponent.enabled = isVisible;
        }
    }

    private void FinishWizard()
    {
        Debug.Log("[Wizard] Input settings locked down. Initializing core lobby link.");

        // 👉 ADD THIS LINE: Save the modified bindings right before marking complete!
        InputSaveManager.SaveBindings(playerInputActions);

        PlayerPrefs.SetInt("HasCompletedInputWizard", 1);
        PlayerPrefs.Save();

        IsWizardComplete = true;
        gameObject.SetActive(false);
    }
}