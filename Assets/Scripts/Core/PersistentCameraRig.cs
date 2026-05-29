using UnityEngine;

public class PersistentCameraRig : MonoBehaviour
{
    public static PersistentCameraRig Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera _mainCamera;
    [Tooltip("The Cinemachine Brain component attached to this GameObject or its child")]
    [SerializeField] private MonoBehaviour _cinemachineBrain; // Using MonoBehaviour to support both CM v2 and v3 types cleanly

    public Camera MainCamera => _mainCamera;

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
            Debug.Log($"[Camera] Duplicate CameraRig detected on scene load. Destroying old local instance on: {gameObject.name}");
            Destroy(gameObject);
        }
    }
}