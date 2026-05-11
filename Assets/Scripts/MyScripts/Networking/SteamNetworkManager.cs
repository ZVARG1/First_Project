using FishNet.Managing;
using FishNet.Transporting;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using FishySteamworks;

namespace FishNet.Example
{
    public class SteamNetworkManager : MonoBehaviour
    {
        public static SteamNetworkManager Instance { get; private set; }

        [Header("Connection Indicators")]
        [SerializeField] private Image _serverIndicator;
        [SerializeField] private Image _clientIndicator;

        [Header("Status Colors")]
        [SerializeField] private Color _stoppedColor = Color.red;
        [SerializeField] private Color _changingColor = Color.yellow;
        [SerializeField] private Color _startedColor = Color.green;

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
                // Unparenting ensures DontDestroyOnLoad works if this was a child of a Bootstrapper
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

            UpdateIndicatorColors();

            // Replaced internal LoadScene with SceneHandler call
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
                // Note: SceneHandler is NOT needed here. 
                // FishNet automatically pulls clients into the correct scene.
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

        public void ToggleServer()
        {
            if (_serverState == LocalConnectionState.Stopped)
            {
                _networkManager.ServerManager.StartConnection();
                CreateSteamLobby();
                // New: Using SceneHandler to move players to the map
                SceneHandler.Instance.LoadGameSceneGlobal("Scene_Main");
            }
            else
            {
                _networkManager.ServerManager.StopConnection(true);
                LeaveSteamLobby();
            }
        }

        public void ToggleClient()
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

        public void ToggleHost()
        {
            if (_serverState == LocalConnectionState.Stopped && _clientState == LocalConnectionState.Stopped)
            {
                _networkManager.ServerManager.StartConnection();
                _networkManager.ClientManager.StartConnection();
                CreateSteamLobby();

                // MOVED: We wait for the connection to start before triggering the scene swap
                if (SceneHandler.Instance != null)
                {
                    SceneHandler.Instance.LoadGameSceneGlobal("Scene_Main");
                }
                else
                {
                    Debug.LogError("SteamNetworkManager: Cannot load Game Scene because SceneHandler is missing!");
                }
            }
        }

        #endregion

        #region Internal State Helpers

        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            _serverState = obj.ConnectionState;
            UpdateColor(obj.ConnectionState, _serverIndicator);
        }

        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
            UpdateColor(obj.ConnectionState, _clientIndicator);
        }

        private void UpdateIndicatorColors()
        {
            UpdateColor(_serverState, _serverIndicator);
            UpdateColor(_clientState, _clientIndicator);
        }

        private void UpdateColor(LocalConnectionState state, Image img)
        {
            if (img == null) return;
            img.color = state switch
            {
                LocalConnectionState.Started => _startedColor,
                LocalConnectionState.Stopped => _stoppedColor,
                _ => _changingColor
            };
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