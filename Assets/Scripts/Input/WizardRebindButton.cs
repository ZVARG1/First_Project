using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class WizardRebindButton : MonoBehaviour
{
    [Header("Input Target")]
    [SerializeField] private InputActionReference _targetActionReference;
    
    // 👉 NEW: Allows you to pick which part of a composite (like Up, Down, Left, Right) this button changes!
    [Tooltip("Leave at 0 for single keys. For composites, 1 is usually the first part, 2 is second, etc.")]
    [SerializeField] private int _bindingIndex = 0; 

    [Header("UI Display Components")]
    [SerializeField] private TextMeshProUGUI _bindingNameText;
    [SerializeField] private TextMeshProUGUI _currentKeyDisplayText;
    [SerializeField] private Button _rebindButton;

    private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    private void Start()
    {
        if (_targetActionReference != null)
        {
            UpdateDisplayUI();
            _rebindButton.onClick.AddListener(StartRebindSequence);
        }
    }

    private void UpdateDisplayUI()
    {
        // Fix: Shows the specific binding part's name if it's an axis composite
        if (_targetActionReference.action.bindings[_bindingIndex].isComposite)
        {
            _bindingNameText.text = _targetActionReference.action.name;
        }
        else
        {
            _bindingNameText.text = $"{_targetActionReference.action.name} ({_targetActionReference.action.bindings[_bindingIndex].name})";
        }
        
        // 👉 Fix: Fetches the display string for the *specific index* instead of the whole group
        _currentKeyDisplayText.text = _targetActionReference.action.GetBindingDisplayString(_bindingIndex);
    }

    private void StartRebindSequence()
    {
        if (_targetActionReference == null) return;

        _targetActionReference.action.Disable();
        _currentKeyDisplayText.text = "< Press Any Key >";
        _rebindButton.interactable = false;

        _rebindOperation = _targetActionReference.action.PerformInteractiveRebinding()
            // 👉 NEW: Tells the operation exactly which sub-binding index to overwrite!
            .WithTargetBinding(_bindingIndex)
            .WithControlsExcluding("Mouse/position")
            .WithControlsExcluding("Mouse/delta")
            .OnMatchWaitForAnother(0.1f) 
            .OnComplete(operation => CleanUpRebind(true))
            .OnCancel(operation => CleanUpRebind(false));

        _rebindOperation.Start();
    }

    private void CleanUpRebind(bool success)
    {
        _rebindOperation.Dispose(); 
        _targetActionReference.action.Enable();
        _rebindButton.interactable = true;

        if (success)
        {
            UpdateDisplayUI();
            InputSaveManager.SaveBindings(_targetActionReference.action.actionMap.asset);
        }
    }
}