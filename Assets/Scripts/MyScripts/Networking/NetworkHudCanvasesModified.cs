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
        
        private NetworkManager _networkManager;
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;

        protected Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;

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
                _currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
                Debug.Log($"Lobby Created Successfully! ID: {_currentLobbyId}");
            }
        }

        private void CreateSteamLobby()
        {
            if (!SteamManager.Initialized) return;
            // Visible to friends so they can use the "Join Game" button
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 32);
        }

        private void CloseSteamLobby()
        {
            if (_currentLobbyId != CSteamID.Nil)
            {
                SteamMatchmaking.LeaveLobby(_currentLobbyId);
                _currentLobbyId = CSteamID.Nil;
                Debug.Log("Left Steam Lobby.");
            }
        }

        private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            string hostSteamID = callback.m_steamIDFriend.m_SteamID.ToString();
            
            // Note: Keep your refactor plan for the 'Invoke' and 'Reflection' bit 
            // once you settle the Assembly Definition issue!
            MonoBehaviour steamTransport = _networkManager.GetComponentInChildren<MonoBehaviour>();
            if (steamTransport.GetType().Name == "FishySteamworks")
            {
                steamTransport.Invoke("SetClientAddress", 0f); 
                _networkManager.ClientManager.StartConnection();
            }
        }

        #region Button Actions

        /// <summary>
        /// Starts or Stops a Dedicated Server. Now handles Steam Lobby visibility.
        /// </summary>
        public void OnClick_DedicatedServer()
        {
            if (_serverState == LocalConnectionState.Stopped)
            {
                _networkManager.ServerManager.StartConnection();
                // Create a lobby so friends can see the "Dedicated" host in their overlay
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
            if (_clientState == LocalConnectionState.Stopped)
                _networkManager.ClientManager.StartConnection();
            else
                _networkManager.ClientManager.StopConnection();
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

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
            if (GUILayout.Button(_serverState == LocalConnectionState.Stopped ? "Start Dedicated Server" : "Stop Server"))
                OnClick_DedicatedServer();
            GUILayout.Space(10);
            if (GUILayout.Button(_clientState == LocalConnectionState.Stopped ? "Join as Client" : "Disconnect Client"))
                OnClick_JoinClient();
            GUILayout.Space(10);
            bool isHosting = (_serverState == LocalConnectionState.Started && _clientState == LocalConnectionState.Started);
            if (GUILayout.Button(isHosting ? "Stop Hosting" : "Host (Server + Client)"))
                OnClick_Host();
            GUILayout.EndArea();
        }

        // Cleanup on application quit or object destruction to ensure the lobby dies
        private void OnDisable()
        {
            CloseSteamLobby();
        }
    }
}