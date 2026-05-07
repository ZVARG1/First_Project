using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;

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

        private NetworkManager _networkManager;
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;

        private void Start()
        {
            _networkManager = FindObjectOfType<NetworkManager>();

            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found!");
                return;
            }

            // Subscribe to events
            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;

            // Initial UI state
            UpdateColor(LocalConnectionState.Stopped, _serverIndicator);
            UpdateColor(LocalConnectionState.Stopped, _clientIndicator);
        }

        private void OnDestroy()
        {
            if (_networkManager == null) return;
            _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }

        #region Button Actions

        /// <summary>
        /// Starts or Stops a Dedicated Server (No Local Player).
        /// </summary>
        public void OnClick_DedicatedServer()
        {
            if (_serverState == LocalConnectionState.Stopped)
                _networkManager.ServerManager.StartConnection();
            else
                _networkManager.ServerManager.StopConnection(true);
        }

        /// <summary>
        /// Starts or Stops a Client connection to join a server.
        /// </summary>
        public void OnClick_JoinClient()
        {
            if (_clientState == LocalConnectionState.Stopped)
                _networkManager.ClientManager.StartConnection();
            else
                _networkManager.ClientManager.StopConnection();
        }

        /// <summary>
        /// Starts both Server and Client (Standard "Host" mode).
        /// </summary>
        public void OnClick_Host()
        {
            if (_serverState == LocalConnectionState.Stopped && _clientState == LocalConnectionState.Stopped)
            {
                _networkManager.ServerManager.StartConnection();
                _networkManager.ClientManager.StartConnection();
            }
            else
            {
                _networkManager.ServerManager.StopConnection(true);
                _networkManager.ClientManager.StopConnection();
            }
        }

        #endregion

        #region State Management

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
        #endregion

        // Simple IMGUI for testing
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
    }
}