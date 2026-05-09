using FishNet.Managing;
using FishNet.Transporting;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using FishySteamworks;

namespace FishNet.Example
{
    public class NetworkHudCanvases : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Color _stoppedColor = Color.red;
        [SerializeField] private Color _changingColor = Color.yellow;
        [SerializeField] private Color _startedColor = Color.green;

        [Header("Indicators")]
        [SerializeField] private Image _serverIndicator;
        [SerializeField] private Image _clientIndicator;

        private CSteamID _currentLobbyId;
        protected Callback<LobbyCreated_t> _lobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;

        private NetworkManager _networkManager;
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;

        private void Start()
        {
            _networkManager = FindAnyObjectByType<NetworkManager>();

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

            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

            UpdateColor(LocalConnectionState.Stopped, _serverIndicator);
            UpdateColor(LocalConnectionState.Stopped, _clientIndicator);
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                _currentLobbyId = (CSteamID)callback.m_ulSteamIDLobby;
                Debug.Log($"Lobby Created Successfully! ID: {_currentLobbyId}");

                // Optional: Set the lobby data so Steam knows which server to join
                SteamMatchmaking.SetLobbyData(_currentLobbyId, "HostAddress", SteamUser.GetSteamID().ToString());
            }
        }

        private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            // Extract the Lobby ID from the callback
            string lobbyID = callback.m_steamIDLobby.ToString();

            // Set the address and connect using FishySteamworks
            if (_networkManager.TransportManager.Transport is FishySteamworks.FishySteamworks transport)
            {
                transport.SetClientAddress(lobbyID);
                _networkManager.ClientManager.StartConnection();
                Debug.Log($"Connecting to Lobby: {lobbyID}");
            }
        }

        private void CreateSteamLobby()
        {
            if (!SteamManager.Initialized) return;
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 32);
        }

        private void CloseSteamLobby()
        {
            if (_currentLobbyId != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(_currentLobbyId);
                _currentLobbyId = CSteamID.Nil;
            }
        }

        #region Button Actions

        public void OnClick_DedicatedServer()
        {
            if (_serverState == LocalConnectionState.Stopped)
            {
                _networkManager.ServerManager.StartConnection();
                CreateSteamLobby();
            }
            else
            {
                _networkManager.ServerManager.StopConnection(true);
                CloseSteamLobby();
            }
        }

        public void OnClick_JoinClient()
        {
            // Disconnect if already connected
            if (_clientState != LocalConnectionState.Stopped)
            {
                _networkManager.ClientManager.StopConnection();
                return;
            }

            if (SteamManager.Initialized)
            {
                SteamFriends.ActivateGameOverlay("friends");

                Debug.Log("Opening Steam Friends list.");
            }
            else
            {
                Debug.LogError("Steam is not initialized!");
            }
        }

        public void OnClick_Host()
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
                CloseSteamLobby();
            }
        }

        #endregion

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

        private void OnDisable()
        {
            CloseSteamLobby();
        }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 250, 400));

            // Server Button
            string serverText = (_serverState == LocalConnectionState.Stopped) ? "Start Dedicated Server" : "Stop Server";
            if (GUILayout.Button(serverText, GUILayout.Height(30)))
                OnClick_DedicatedServer();

            GUILayout.Space(10);

            string clientText = (_clientState == LocalConnectionState.Stopped) ? "Open Friends to Join" : "Disconnect Client";
            if (GUILayout.Button(clientText, GUILayout.Height(30)))
                OnClick_JoinClient();

            GUILayout.Space(10);

            bool isHosting = (_serverState == LocalConnectionState.Started && _clientState == LocalConnectionState.Started);
            string hostText = isHosting ? "Stop Hosting" : "Host (Server + Client)";
            if (GUILayout.Button(hostText, GUILayout.Height(30)))
                OnClick_Host();

            GUILayout.EndArea();
        }

    }
}