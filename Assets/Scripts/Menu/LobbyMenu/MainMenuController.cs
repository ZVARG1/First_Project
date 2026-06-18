using UnityEngine;
using FishNet.Example;

public class MainMenuController : MonoBehaviour
{

    // Renamed from HostButtonPressed to reflect exactly what it does
    public void StartHostLobby() 
    {
        SteamNetworkManager.Instance.StartHostLobby();
    }

    // Renamed from JoinButtonPressed
    public void ConnectAsClient() 
    {
        // Note: We don't HideMenu here because the Steam Overlay 
        // might be closed without successfully joining a game.
        SteamNetworkManager.Instance.ConnectAsClient();
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