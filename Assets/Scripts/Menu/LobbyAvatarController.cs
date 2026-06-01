using UnityEngine;
using UnityEngine.InputSystem; 
using FishNet.Object; 

[RequireComponent(typeof(CharacterController))]
public class LobbyAvatarController : NetworkBehaviour 
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

    private InputAction _moveAction;
    private InputAction _lookAction;

    public override void OnStartClient()
    {
        base.OnStartClient();

        _controller = GetComponent<CharacterController>();
        if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();

        if (IsOwner)
        {
            _isLocalOwner = true;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Wake up input ONLY for the actual owner
            if (_playerInput != null) _playerInput.enabled = true; 

            InitializeCleanInputMaps();
        }
        else
        {
            // Explicitly strip/disable remote proxy clones
            if (_playerInput != null)
            {
                _playerInput.enabled = false;
                _playerInput.actions = null; 
            }

            AudioListener remoteListener = GetComponentInChildren<AudioListener>();
            if (remoteListener != null) remoteListener.enabled = false;

            Camera remoteCamera = GetComponentInChildren<Camera>();
            if (remoteCamera != null) remoteCamera.enabled = false;

            enabled = false; 
        }
    }

    private void InitializeCleanInputMaps()
    {
        if (_playerInput == null || _playerInput.actions == null) return;

        _playerInput.actions = Instantiate(_playerInput.actions);

        foreach (var map in _playerInput.actions.actionMaps)
        {
            map.Disable();
        }

        var defaultMap = _playerInput.actions.FindActionMap(_lobbyActionMapName);
        if (defaultMap != null)
        {
            defaultMap.Enable();
            _moveAction = defaultMap.FindAction("Move");
            _lookAction = defaultMap.FindAction("Look");
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
        if (_controller == null || !_controller.enabled) return;

        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; 
        }

        Vector2 moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

        Vector3 moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
        Vector3 finalVelocity = moveDirection * _moveSpeed;

        _velocity.y += _gravity * Time.deltaTime;
        finalVelocity.y = _velocity.y;

        _controller.Move(finalVelocity * Time.deltaTime);
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