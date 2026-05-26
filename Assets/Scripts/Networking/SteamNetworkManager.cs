using FishNet.Managing;
using FishNet.Transporting;
using Steamworks;
using UnityEngine;
using FishySteamworks;

namespace FishNet.Example
{
    public class SteamNetworkManager : MonoBehaviour
    {
        public static SteamNetworkManager Instance { get; private set; }

        private NetworkManager _networkManager;
        private CSteamID _currentLobbyId;

        private LocalConnectionState _serverState = LocalConnectionState.Stopped;
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;

        protected Callback<LobbyCreated_t> _lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _networkManager = FishNet.InstanceFinder.NetworkManager;
        }

        private void Start()
        {
            if (_networkManager == null) return;

            if (SteamManager.Initialized)
            {
                _lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
                _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            }

            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

            if (SceneHandler.Instance != null)
            {
                SceneHandler.Instance.LoadSceneLocal("Scene_MainMenu");
            }
            else
            {
                Debug.LogWarning("SteamNetworkManager: SceneHandler.Instance is null in Start. Is it in the Boot scene?");
            }
        }

        #region Steam Lobby Logic

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                _currentLobbyId = (CSteamID)callback.m_ulSteamIDLobby;
                SteamMatchmaking.SetLobbyData(_currentLobbyId, "HostAddress", SteamUser.GetSteamID().ToString());
                Debug.Log($"Steam Lobby Created: {_currentLobbyId}");
            }
        }

        private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            if (_networkManager.TransportManager.Transport is FishySteamworks.FishySteamworks transport)
            {
                transport.SetClientAddress(callback.m_steamIDLobby.ToString());
                _networkManager.ClientManager.StartConnection();
            }
        }

        private void CreateSteamLobby()
        {
            if (!SteamManager.Initialized) return;
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 32);
        }

        private void LeaveSteamLobby()
        {
            if (_currentLobbyId != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(_currentLobbyId);
                _currentLobbyId = CSteamID.Nil;
            }
        }

        #endregion

        #region Public Button API

        public void ConnectAsClient() 
        {
            if (_clientState == LocalConnectionState.Stopped)
            {
                if (SteamManager.Initialized)
                    SteamFriends.ActivateGameOverlay("friends");
            }
            else
            {
                _networkManager.ClientManager.StopConnection();
            }
        }

        public void StartHostLobby()
        {
            if (_serverState == LocalConnectionState.Stopped && _clientState == LocalConnectionState.Stopped)
            {
                _networkManager.ServerManager.StartConnection();
                _networkManager.ClientManager.StartConnection();
                CreateSteamLobby();
            // REMOVED SceneHandler.Instance.LoadGameSceneGlobal here!
            // Because we are staying in Scene_MainMenu to walk around the hangar.
            }
        }

        #endregion

        #region Internal State Helpers

        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            _serverState = obj.ConnectionState;
        }

        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
        }
        
        #endregion

        private void OnDestroy()
        {
            if (_networkManager != null && _networkManager.ServerManager != null)
            {
                _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
                _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
            }
            LeaveSteamLobby();
        }
    }
}