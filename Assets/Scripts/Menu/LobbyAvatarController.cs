using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))]
public class LobbyAvatarController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private float _mouseSensitivity = 15f;
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    [Header("Input Setup")]
    [SerializeField] private PlayerInput _playerInput; 
    [SerializeField] private string _lobbyActionMapName = "Player"; 

    [Header("References")]
    [SerializeField] private Transform _cameraHolder; 

    private CharacterController _controller;
    private Vector3 _velocity;
    private float _cameraPitch = 0f;
    private bool _isLocalOwner = false;

    // Direct Action References (Bypasses Unity Events completely)
    private InputAction _moveAction;
    private InputAction _lookAction;

    void Start()
    {
        _controller = GetComponent<CharacterController>();

        if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();

        FishNet.Object.NetworkObject networkParent = GetComponentInParent<FishNet.Object.NetworkObject>();
        if (networkParent != null && networkParent.IsOwner)
        {
            _isLocalOwner = true;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            InitializeCleanInputMaps();
        }
        else
        {
            if (_playerInput != null) _playerInput.enabled = false;
            enabled = false;
        }
    }

    private void InitializeCleanInputMaps()
    {
        if (_playerInput == null || _playerInput.actions == null) return;

        // Force-disable every action map globally to clear the singleton cache
        foreach (var map in _playerInput.actions.actionMaps)
        {
            map.Disable();
        }

        // Explicitly enable ONLY our walking map
        var defaultMap = _playerInput.actions.FindActionMap(_lobbyActionMapName);
        if (defaultMap != null)
        {
            defaultMap.Enable();
            
            // Cache the raw inputs directly through the map API
            _moveAction = defaultMap.FindAction("Move");
            _lookAction = defaultMap.FindAction("Look");

            Debug.Log($"[InputSystem] Isolated action map: {_lobbyActionMapName} and linked actions directly.");
        }
        else
        {
            Debug.LogWarning($"[InputSystem] Could not find action map: {_lobbyActionMapName}");
        }
    }

    void Update()
    {
        if (!_isLocalOwner) return;

        HandleRotation();
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; 
        }

        // Read values directly from hardware if actions exist
        Vector2 moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

        Vector3 moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
        _controller.Move(moveDirection * _moveSpeed * Time.deltaTime);

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector2 lookInput = _lookAction != null ? _lookAction.ReadValue<Vector2>() : Vector2.zero;

        float lookX = lookInput.x * _mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * lookX);

        float lookY = lookInput.y * _mouseSensitivity * Time.deltaTime;
        _cameraPitch -= lookY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, _minPitch, _maxPitch);

        if (_cameraHolder != null)
        {
            _cameraHolder.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }
    }

    private void OnDisable()
    {
        if (_isLocalOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (_playerInput != null && _playerInput.actions != null)
            {
                var defaultMap = _playerInput.actions.FindActionMap(_lobbyActionMapName);
                defaultMap?.Disable();
            }
        }
    }
}