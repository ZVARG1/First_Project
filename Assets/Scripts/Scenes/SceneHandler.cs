using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;

// Alias to avoid conflicts with FishNet's SceneManager.
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// Handles scene transitions for both local (Unity) and
/// networked (FishNet) scene loading.
/// </summary>
public class SceneHandler : MonoBehaviour
{
    #region Singleton

    public static SceneHandler Instance { get; private set; }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Local Scene Loading

    /// <summary>
    /// Loads a scene locally using Unity's SceneManager.
    /// This transition is not synchronized over the network.
    /// </summary>
    /// <param name="sceneName">Scene to load.</param>
    public void LoadSceneLocal(string sceneName)
    {
        UnitySceneManager.LoadScene(sceneName);
    }

    #endregion

    #region Network Scene Loading

    /// <summary>
    /// Loads a scene globally for every connected player using FishNet.
    /// Intended for transitions into multiplayer gameplay.
    /// </summary>
    /// <param name="sceneName">Scene to load.</param>
    public void LoadGameSceneGlobal(string sceneName)
    {
        if (!InstanceFinder.ServerManager.Started)
        {
            Debug.LogWarning("[SceneHandler] Cannot load a global scene because the server is not running.");
            return;
        }

        SceneLoadData loadData = new SceneLoadData(new SceneLookupData(sceneName))
        {
            PreferredActiveScene = new PreferredScene(new SceneLookupData(sceneName))
        };

        SceneUnloadData unloadData = new SceneUnloadData(
            new SceneLookupData(SceneNames.HangarLobby));

        InstanceFinder.SceneManager.LoadGlobalScenes(loadData);
        InstanceFinder.SceneManager.UnloadGlobalScenes(unloadData);

        Debug.Log($"[SceneHandler] Loading global scene '{sceneName}'.");
    }

    #endregion
}