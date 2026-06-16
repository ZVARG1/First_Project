using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputWizardManager : MonoBehaviour
{
    [Header("UI Component Links")]
    [SerializeField] private TextMeshProUGUI _displayText;
    [SerializeField] private Image _backgroundImage;

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
    [SerializeField] private float _staticDuration = 0.8f;

    private int _currentPageIndex = 0; // Page 0 is our cinematic intro
    private bool _isIntroRunning = false;

    // Cinematic Text String Configurations
    private string _phase1Text = "Welcome back, pilot...";
    private string _phase2Text = "<font=\"AlienFont_Asset\"><color=#00FFCC>WARNING: NEURAL LINK COMPROMISED.</color></font>";

    private void Start()
    {
        // 0 = New Player, 1 = Completed
        if (PlayerPrefs.GetInt("HasCompletedInputWizard", 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        InitializeWizard();
    }

    private void InitializeWizard()
    {
        _currentPageIndex = 0;
        
        // Hide all setup panels initially during the intro cinematic
        foreach (GameObject page in _interactivePages)
        {
            if (page != null) page.SetActive(false);
        }

        // Lock navigation inputs until the sequence lands safely
        _backButton.gameObject.SetActive(false);
        _nextButton.gameObject.SetActive(false);

        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator RunIntroSequence()
    {
        _isIntroRunning = true;
        _displayText.text = "";
        _backgroundImage.sprite = _humanHangarBackground;
        _backgroundImage.color = Color.white;

        // --- PHASE 1: Human Typewriter ---
        foreach (char letter in _phase1Text.ToCharArray())
        {
            _displayText.text += letter;
            yield return new WaitForSeconds(_typeSpeed);
        }
        yield return new WaitForSeconds(1.0f);

        // --- PHASE 2: Static Noise Distortion ---
        float elapsedStaticTime = 0f;
        _backgroundImage.sprite = _alienGlitchBackground;

        while (elapsedStaticTime < _staticDuration)
        {
            _backgroundImage.color = new Color(1f, 1f, 1f, Random.Range(0.4f, 0.9f));
            _displayText.text = GenerateGlitchGarbage(Random.Range(15, 30));
            yield return new WaitForSeconds(0.06f); 
            elapsedStaticTime += 0.06f;
        }

        _backgroundImage.color = Color.white;
        _displayText.text = "";

        // --- PHASE 3: Alien Language Overlay ---
        _displayText.text = _phase2Text;
        _displayText.maxVisibleCharacters = 0;

        // Force TMPro to update mesh info so character count reads accurately with styling tags
        _displayText.ForceMeshUpdate(); 
        int totalVisibleCharacters = _displayText.textInfo.characterCount;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            _displayText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(_typeSpeed * 0.5f);
        }

        // --- INTRO COMPLETE: Unlock Wizard Navigation ---
        _isIntroRunning = false;
        _nextButton.gameObject.SetActive(true);
        TMPro.TextMeshProUGUI nextText = _nextButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nextText != null) nextText.text = "PROCEED >";
    }

    public void OnClickNext()
    {
        if (_isIntroRunning) return;

        if (_currentPageIndex < _interactivePages.Length) // Note: length matches remaining steps
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
        // Hide the main narrative text once the user progresses into the configurations
        _displayText.gameObject.SetActive(_currentPageIndex == 0);

        // Turn pages on or off based on our index tracking (offset by 1 because page 0 was the intro)
        for (int i = 0; i < _interactivePages.Length; i++)
        {
            if (_interactivePages[i] != null)
            {
                _interactivePages[i].SetActive((i + 1) == _currentPageIndex);
            }
        }

        // Manage button display configurations
        _backButton.gameObject.SetActive(_currentPageIndex > 1);
        
        TMPro.TextMeshProUGUI nextText = _nextButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nextText != null)
        {
            nextText.text = (_currentPageIndex == _interactivePages.Length) ? "INITIALIZE LINK" : "NEXT >";
        }
    }

    private string GenerateGlitchGarbage(int length)
    {
        char[] chars = "!@#$%^&*()_+==}{[]|?/<>:;".ToCharArray();
        string garbage = "";
        for (int i = 0; i < length; i++)
        {
            garbage += chars[Random.Range(0, chars.Length)];
        }
        return garbage;
    }

    private void FinishWizard()
    {
        Debug.Log("[Wizard] Input settings locked down. Initializing core lobby link.");
        PlayerPrefs.SetInt("HasCompletedInputWizard", 1);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}