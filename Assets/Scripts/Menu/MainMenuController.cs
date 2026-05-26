using UnityEngine;
using FishNet.Example;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuRoot; // Assign your "MainMenu" UI object here

    // Renamed from HostButtonPressed to reflect exactly what it does
    public void StartHostLobby() 
    {
        HideMenu();
        SteamNetworkManager.Instance.StartHostLobby();
    }

    // Renamed from JoinButtonPressed
    public void ConnectAsClient() 
    {
        // Note: We don't HideMenu here because the Steam Overlay 
        // might be closed without successfully joining a game.
        SteamNetworkManager.Instance.ConnectAsClient();
    }

    private void HideMenu()
    {
        if (menuRoot != null) 
        {
            menuRoot.SetActive(false);
        }
        
        // Removed the camera-disabling code from here.
        // HangarIntroManager now handles camera states exclusively to prevent conflicts.
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