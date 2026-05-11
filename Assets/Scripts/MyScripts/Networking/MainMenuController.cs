using UnityEngine;
using FishNet.Example;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot; // Assign your "MainMenu" object here

    public void HostButtonPressed() 
    {
        HideMenu();
        SteamNetworkManager.Instance.ToggleHost();
    }

    public void ServerButtonPressed() 
    {
        // For a dedicated server, we still hide the menu 
        // even though there's no "player" locally.
        HideMenu();
        SteamNetworkManager.Instance.ToggleServer();
    }

    public void JoinButtonPressed() 
    {
        // Note: We don't HideMenu here because the Steam Overlay 
        // might be closed without joining a game.
        SteamNetworkManager.Instance.ToggleClient();
    }

    private void HideMenu()
    {
        if(menuRoot != null) menuRoot.SetActive(false);
        
        // As we discovered earlier, killing the camera helps CachyOS/Linux
        // clear the render buffer properly.
        Camera menuCam = Camera.main;
        if (menuCam != null) menuCam.enabled = false;
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}