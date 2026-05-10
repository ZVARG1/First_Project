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

        // Steam Callbacks
        protected Callback<LobbyCreated_t> _lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;

        private void Awake()
        {
            _networkManager = FindAnyObjectByType<NetworkManager>();
        }

        private void Start()
        {
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found!");
                return;
            }

            if (SteamManager.Initialized)
            {
                _lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
                _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            }

            // Subscribe to FishNet state changes
            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

            UpdateIndicatorColors();
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
            string lobbyID = callback.m_steamIDLobby.ToString();

            if (_networkManager.TransportManager.Transport is FishySteamworks.FishySteamworks transport)
            {
                transport.SetClientAddress(lobbyID);
                _networkManager.ClientManager.StartConnection();
            }
        }

        private void CreateSteamLobby()
        {
            if (!SteamManager.Initialized) return;
            // Matches your 32-player capacity requirement
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
            }
            else
            {
                _networkManager.ServerManager.StopConnection(true);
                _networkManager.ClientManager.StopConnection();
                LeaveSteamLobby();
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
            if (_networkManager != null)
            {
                _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
                _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
            }
            LeaveSteamLobby();
        }
    }
}