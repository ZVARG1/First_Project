using UnityEngine;
using FishNet.Object;

public class NetworkPlayerSetup : NetworkBehaviour
{
    [Header("References to Isolate")]
    [SerializeField] private LobbyAvatarController _localMovementController;
    [SerializeField] private Transform _cameraHolder;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            // ==========================================
            // LOCAL OWNER: Wake up local views and inputs
            // ==========================================
            if (_cameraHolder != null) _cameraHolder.gameObject.SetActive(true);
            
            if (_localMovementController != null) 
            {
                _localMovementController.ActivateController();
                
                // HANDSHAKE: Register this specific local instance with our UI system!
                if (LobbyUIManager.Instance != null)
                {
                    LobbyUIManager.Instance.RegisterLocalPlayer(_localMovementController);
                    Debug.Log("[NetworkSetup] Local player successfully registered with LobbyUIManager.");
                }
            }

            Debug.Log("[NetworkSetup] Successfully identified owner and triggered local controller activation.");
        }
        else
        {
            // ==========================================
            // REMOTE PROXY: Keep everything asleep
            // ==========================================
            if (_localMovementController != null) _localMovementController.enabled = false;
            if (_cameraHolder != null) _cameraHolder.gameObject.SetActive(false);

            CharacterController proxyController = GetComponent<CharacterController>();
            if (proxyController != null) proxyController.enabled = false;

            Debug.Log("[NetworkSetup] Successfully isolated remote proxy structure.");
        }
    }
}