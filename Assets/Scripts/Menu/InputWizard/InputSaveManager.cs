using UnityEngine;
using UnityEngine.InputSystem;

public static class InputSaveManager
{
    private const string SaveKey = "CustomInputBindings";

    /// <summary>
    /// Saves all active control overrides from an InputActionAsset into PlayerPrefs
    /// </summary>
    public static void SaveBindings(InputActionAsset actions)
    {
        if (actions == null) return;

        // Serializes all overrides into a lightweight JSON string
        string rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(SaveKey, rebinds);
        PlayerPrefs.Save();
        
        Debug.Log("[InputSaveManager] Controls saved successfully.");
    }

    /// <summary>
    /// Loads saved control overrides from PlayerPrefs back into your InputActionAsset
    /// </summary>
    public static void LoadBindings(InputActionAsset actions)
    {
        if (actions == null) return;

        if (PlayerPrefs.HasKey(SaveKey))
        {
            string rebinds = PlayerPrefs.GetString(SaveKey);
            actions.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("[InputSaveManager] Custom controls loaded and applied.");
        }
        else
        {
            Debug.Log("[InputSaveManager] No saved controls found. Using defaults.");
        }
    }
}