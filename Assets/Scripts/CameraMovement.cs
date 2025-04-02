using Unity.Netcode;
using UnityEngine;

[RequireComponent (typeof(RTSPlayerControls))]
public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private RTSPlayerControls rtsPlayerControls;
    [SerializeField] private bool isPanning = false;
    [SerializeField] private float panMultiplier = 0.1f;
    [SerializeField] private float maxPanningSpeed = 1;
    [SerializeField] private float panningEdgeThreshold = 100;
    [SerializeField] private float minFOV = 15;
    [SerializeField] private float maxFOV = 60;
    [SerializeField] private float zoomSensitivity = 1;
    Camera cameraComp;
    GameObject mainCamera;
    private NetworkObject networkObject;
    private Vector2 panStartPos;
    private Vector2 screenPosition => rtsPlayerControls.MouseScreenPos;

    float screenWidth = Screen.width;
    float screenHeight = Screen.height;

    private void Awake()
    {
        if (!TryGetComponent<NetworkObject>(out networkObject))
        {
            Debug.LogError("Network object is required for cameraMovement");
        }
    }

    /// <summary>
    /// Sets up the Camera Movement variables for the mainCamera, run once the Main Camera has been spawned.
    /// </summary>
    public void Init()
    {
        mainCamera = Camera.main.gameObject;
        cameraComp = mainCamera.GetComponent<Camera>();
        rtsPlayerControls = GetComponent<RTSPlayerControls>();

        

    }

    // Update is called once per frame
    void Update()
    {
        if (networkObject.IsOwner)
        {
            if (isPanning)
            {
                ApplyPan(GetManualPanVector());
            }
            else
            {
                //ApplyPan(isMouseNearScreenEdge());
            }
        }
    }

    /// <summary>
    /// Called when panning should start to get the start position of the mouse
    /// on the screen in order to determine panning
    /// </summary>
    /// <param name="_screenPosition"></param>
    public void StartPanning(Vector2 _screenPosition)
    {
        panStartPos = _screenPosition;
        isPanning = true;
    }

    /// <summary>
    /// Sets the isPanning var to false effectively stopping the panning logic
    /// </summary>
    public void StopPanning()
    {
        isPanning = false;
    }

    /// <summary>
    /// Ran while the user is holding down middlemouse, returns a Vector3 direction vector from where the 
    /// player started holding down the middle mouse towards where the mouse currently is in screenSpace.
    /// </summary>
    /// <returns></returns>
    private Vector3 GetManualPanVector()
    {
        Vector2 direction = screenPosition - panStartPos;

        // convert to vector 3
        Vector3 panningVector = new Vector3 { x = direction.x, y = 0, z = direction.y };

        panningVector = panningVector * panMultiplier;
        panningVector = Vector3.ClampMagnitude(panningVector, maxPanningSpeed);

        return panningVector;
    }

    /// <summary>
    /// Moves the camera in accordance to the panningVector
    /// </summary>
    /// <param name="_panningVector"></param>
    private void ApplyPan(Vector3 _panningVector)
    {
        mainCamera.transform.position += _panningVector * Time.deltaTime;
    }

    /// <summary>
    /// Reduces the fov of the camere by the axis given, scaled by zoomSensitivity and clamped within min/mav FOV
    /// </summary>
    /// <param name="axis"></param>
    public void ApplyZoom(int axis)
    {
        cameraComp.fieldOfView -= axis * zoomSensitivity; // Adjust FOV
        cameraComp.fieldOfView = Mathf.Clamp(cameraComp.fieldOfView, minFOV, maxFOV); // Clamp zoom range
    }

    /// <summary>
    /// If the mouse is near the screen edge, returns a vector representing which edges. Otherwise returns a Vector3.Zero
    /// </summary>
    /// <returns></returns>
    private Vector3 isMouseNearScreenEdge()
    {
        // Check if mouse is near edges
        bool isNearLeft = screenPosition.x <= panningEdgeThreshold;
        bool isNearRight = screenPosition.x >= screenWidth - panningEdgeThreshold;
        bool isNearTop = screenPosition.y >= screenHeight - panningEdgeThreshold;
        bool isNearBottom = screenPosition.y <= panningEdgeThreshold;

        Vector3 edgeVector = new Vector3 { x = 0, y = 0, z = 0 };

        if (isNearLeft) // TODO: This is dumb
        {
            edgeVector.x = -maxPanningSpeed;
        }
        if (isNearRight)
        {
            edgeVector.x = maxPanningSpeed;
        }
        if (isNearTop)
        {
            edgeVector.z = maxPanningSpeed;
        }
        if (isNearBottom)
        {
            edgeVector.z = -maxPanningSpeed;
        }

        return edgeVector;
    }

}
