using UnityEngine;
using UnityEngine.SceneManagement; // For local loads
using FishNet;
using FishNet.Managing.Scened; // For global loads

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    // Standard Unity Load (Local)
    public void LoadSceneLocal(string sceneName)
    {
        // Explicitly using the Unity namespace to avoid ambiguity
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    // FishNet Global Load (Networked)
    public void LoadGameSceneGlobal(string mapName)
    {
        if (InstanceFinder.ServerManager.Started)
        {
            SceneLoadData sld = new SceneLoadData(new SceneLookupData(mapName));

            sld.PreferredActiveScene = new PreferredScene(new SceneLookupData(mapName));

            SceneUnloadData sud = new SceneUnloadData(new SceneLookupData("Scene_MainMenu"));

            InstanceFinder.SceneManager.LoadGlobalScenes(sld);
            InstanceFinder.SceneManager.UnloadGlobalScenes(sud);
        }
    }
}