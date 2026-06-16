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

    private InputAction _moveAction;
    private InputAction _lookAction;

    // Safety latch: Prevent Update loops from executing until the network tells us we are ready!
    private bool _isInitialized = false;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        // Force components off immediately on Awake to prevent early Unity lifecycle execution
        this.enabled = false;
        if (_controller != null) _controller.enabled = false;
        if (_playerInput != null) _playerInput.enabled = false;
    }

    // Explicitly called ONLY by NetworkPlayerSetup on the owning client machine
    public void ActivateController()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        if (_controller != null) _controller.enabled = true;
        if (_playerInput != null) _playerInput.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InitializeLocalInputMaps();

        _isInitialized = true;
        this.enabled = true; // Turn the Update loop on now that we are safe!

        Debug.Log("[Controller] Local avatar successfully initialized and unlatched!");
    }

    private void InitializeLocalInputMaps()
    {
        if (_playerInput == null || _playerInput.actions == null) return;

        // Clone the input actions map reference to prevent multi-client hardware crossover
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
        if (!_isInitialized) return;

        // If the UI manager has locked our inputs because a menu is open, freeze updates!
        if (_inputLocked) return;

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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _isInitialized = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // If the game is back in focus and the network gatekeeper has initialized us...
        if (hasFocus && _isInitialized)
        {
            // Re-force the cursor to hide and lock to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("[Controller] Game regained focus. Cursor safely re-locked.");
        }
    }
    // Add this variable near the top of your class
    private bool _inputLocked = false;

    public void SetInputLock(bool locked)
    {
        _inputLocked = locked;
    }

    public void TeleportTo(Vector3 destinationPosition)
    {
        // CRUCIAL: CharacterControllers fight manual position updates because of internal physics caching.
        // We must disable it for one frame, snap the position, and turn it back on.
        if (_controller != null)
        {
            _controller.enabled = false;
            transform.position = destinationPosition;
            _controller.enabled = true;

            // Clear any falling velocity so they don't slam down at high speed after snapping
            _velocity = Vector3.zero;
        }
    }
}