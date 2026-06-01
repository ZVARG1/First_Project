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

        // State flag to distinguish intentional room migration from accidental disconnects
        private bool _isTransitioning = false; 

        protected Callback<LobbyCreated_t> _lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;
        protected Callback<LobbyDataUpdate_t> _lobbyDataUpdated;

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
                _lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdated);
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
            _currentLobbyId = callback.m_steamIDLobby;
            _isTransitioning = true; 

            TearDownLocalHub();

            // Kick off the upgraded active polling routine
            StartCoroutine(WaitAndConnectToFriend(callback.m_steamIDLobby));
        }

        private System.Collections.IEnumerator WaitAndConnectToFriend(CSteamID lobbyId)
        {
            Debug.Log("[SocialHub] Waiting for local network cleanup to complete...");

            // 1. Let FishNet drop its current sockets safely
            while (_serverState != LocalConnectionState.Stopped || _clientState != LocalConnectionState.Stopped)
            {
                yield return null;
            }

            Debug.Log("[SocialHub] Local network clean. Requesting metadata refresh from Steam...");

            // 2. Force Steam to download data fields for this specific foreign lobby
            SteamMatchmaking.RequestLobbyData(lobbyId);

            float timeoutTimer = 0f;
            string hostAddress = string.Empty;

            // 3. Actively check the metadata cache frame-by-frame for up to 5 seconds
            while (string.IsNullOrEmpty(hostAddress) && timeoutTimer < 5.0f)
            {
                hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, "HostAddress");
                
                if (string.IsNullOrEmpty(hostAddress))
                {
                    timeoutTimer += Time.deltaTime;
                    yield return null; 
                }
            }

            // 4. Connect if resolved, otherwise fall back to a safe solo hub
            if (!string.IsNullOrEmpty(hostAddress))
            {
                Debug.Log($"[SocialHub] Handshake parameter resolved: {hostAddress}. Connecting via FishySteamworks...");
                FishySteamworks.FishySteamworks transport = _networkManager.TransportManager.GetTransport<FishySteamworks.FishySteamworks>();
                if (transport != null)
                {
                    transport.SetClientAddress(hostAddress);
                    _networkManager.ClientManager.StartConnection();
                    _isTransitioning = false; 
                }
            }
            else
            {
                Debug.LogError("[SocialHub] Steam failed to return HostAddress within timeout. Restoring solo hub...");
                _isTransitioning = false;
                StartHostLobby();
            }
        }

        private void OnLobbyDataUpdated(LobbyDataUpdate_t callback)
        {
            if (_currentLobbyId == CSteamID.Nil || (CSteamID)callback.m_ulSteamIDLobby != _currentLobbyId) return;

            FishySteamworks.FishySteamworks transport = _networkManager.TransportManager.GetTransport<FishySteamworks.FishySteamworks>();
            if (transport == null) return;

            if (_clientState != LocalConnectionState.Stopped && _clientState != LocalConnectionState.Starting) return;
            if (_serverState != LocalConnectionState.Stopped) return;

            string hostAddress = SteamMatchmaking.GetLobbyData(_currentLobbyId, "HostAddress");

            if (!string.IsNullOrEmpty(hostAddress))
            {
                if (_clientState == LocalConnectionState.Starting) return;

                Debug.Log($"[SocialHub] Catch-up Sync! Received HostAddress: {hostAddress}. Resolving connection handshake...");
                transport.SetClientAddress(hostAddress);
                _networkManager.ClientManager.StartConnection();
                _isTransitioning = false; 
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

        private void TearDownLocalHub()
        {
            Debug.Log("[SocialHub] Dismantling personal lobby bubble to join a friend...");
            LeaveSteamLobby();

            if (_networkManager.ServerManager.Started)
            {
                _networkManager.ServerManager.StopConnection(true);
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

            if (_clientState == LocalConnectionState.Stopped)
            {
                if (!_isTransitioning && gameObject.scene.isLoaded && Steamworks.SteamAPI.IsSteamRunning())
                {
                    Debug.Log("[SocialHub] Disconnected from session. Restoring solo sandbox room...");
                    StartHostLobby();
                }
                else if (_isTransitioning)
                {
                    Debug.Log("[SocialHub] Client connection stopped locally for intentional room migration. Reboot sequence bypassed safely.");
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                if (_networkManager.ServerManager != null)
                {
                    _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
                }

                if (_networkManager.ClientManager != null)
                {
                    _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
                }
            }

            try
            {
                if (Steamworks.SteamAPI.IsSteamRunning())
                {
                    LeaveSteamLobby();
                }
            }
            catch (System.InvalidOperationException)
            {
                Debug.Log("[NetworkManager] Caught Steamworks shutdown exception safely during scene exit.");
            }
        }
    }
}