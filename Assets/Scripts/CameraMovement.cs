using Cinemachine;
using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent (typeof(RTSPlayerControls))]
public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private RTSPlayerControls rtsPlayerControls;
    [SerializeField] private bool isPanning = false;
    [SerializeField] private float panMultiplier = 0.1f;
    [SerializeField] private float maxPanningSpeed = 1;
    [SerializeField] private float maxZoomedPanningSpeed = 0.1f;
    [SerializeField] private float panningEdgeThreshold = 100;
    [SerializeField] private float maxZoom = 300;
    [SerializeField] private float targetZoom = 200;
    private float currentZoom;
    [SerializeField] private float zoomSensitivity = 1;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private Transform panningTarget;
    [SerializeField] private Vector3 panningRotationOffset;
    private float minZoom = 0;
    private Vector3 startPosition = Vector3.zero;
    private CameraSpawner cameraSpawner;
    private Camera cameraComp;
    private GameObject mainCamera;
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineTransposer transposer;
    private Vector3 originalFollowOffset;
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
            return;
        }
        if (panningTarget == null)
        {
            Debug.LogError($"{nameof(panningTarget)} is null!");
        }
    }

    /// <summary>
    /// Sets up the Camera Movement variables for the mainCamera, run once the Main Camera has been spawned.
    /// </summary>
    public void Init()
    {
        mainCamera = Camera.main.gameObject;
        startPosition = mainCamera.transform.position;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        currentZoom = targetZoom;

        if (!mainCamera.TryGetComponent<Camera>(out cameraComp))
        {
            Debug.LogError($"{nameof(Camera)} was not found in MainCamera!");
            return;
        }

        if (!TryGetComponent<CameraSpawner>(out cameraSpawner))
        {
            Debug.LogError($"{nameof(CameraSpawner)} was not found on {gameObject.name} and is required for {GetType().Name}!");
            return;
        }

        if (!TryGetComponent<RTSPlayerControls>(out rtsPlayerControls))
        {
            Debug.LogError($"{nameof(RTSPlayerControls)} was not found on {gameObject.name} and is required for {GetType().Name}!");
            return;
        }

        SetVirtualCamera(cameraSpawner.VirtualCamera);
        if (virtualCamera == null)
        {
            Debug.LogError($"{nameof(CinemachineVirtualCamera)} is " +
            $"required to be set via {nameof(SetVirtualCamera)} before {nameof(Init)} is called!");
            return;
        }

        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        originalFollowOffset = transposer.m_FollowOffset;
    }

    public void SetVirtualCamera(CinemachineVirtualCamera _virtualCamera)
    {
        if (_virtualCamera == null)
        {
            Debug.LogError($"{nameof(_virtualCamera)} was null!");
        }

        virtualCamera = _virtualCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (isPanning)
        {
            ApplyPan(GetManualPanVector());
        }
        else
        {
            //ApplyPan(isMouseNearScreenEdge());
        }

        UpdateCurrentZoom();
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        //panningTarget.transform.position = startPosition + (mainCamera.transform.forward * targetZoom);
        Vector3 offset = originalFollowOffset + (mainCamera.transform.forward * currentZoom);
        transposer.m_FollowOffset = offset;
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
        
        float panLerpValue = targetZoom / maxZoom;
        panLerpValue = 1 - panLerpValue;

        float currentPanSpeed = Mathf.Lerp(maxZoomedPanningSpeed, maxPanningSpeed, panLerpValue);

        panningVector = panningVector * panMultiplier;
        panningVector = Vector3.ClampMagnitude(panningVector, currentPanSpeed);

        panningVector = Quaternion.Euler(panningRotationOffset) * panningVector;

        return panningVector;
    }

    /// <summary>
    /// Moves the camera in accordance to the panningVector
    /// </summary>
    /// <param name="_panningVector"></param>
    private void ApplyPan(Vector3 _panningVector)
    {
        panningTarget.position += _panningVector * Time.deltaTime;

        // Clamp to bounds
        panningTarget.position = ClampToBounds(panningTarget.position, MapManager.MapBounds);


    }

    /// <summary>
    /// Clamps the Vector3 pos within the bounds given. Used to panning clamps
    /// </summary>
    /// <param name="_position"></param>
    /// <param name="_bounds"></param>
    /// <returns></returns>
    private Vector3 ClampToBounds(Vector3 _position, Bounds _bounds)
    {                   // My day is ruined.... it doesn't slot nicely :(
        return new Vector3( Mathf.Clamp(_position.x, _bounds.min.x, _bounds.max.x),
                            Mathf.Clamp(_position.y, _bounds.min.y, _bounds.max.y),
                            Mathf.Clamp(_position.z, _bounds.min.z, _bounds.max.z)
        );
    }

    /// <summary>
    /// Adds to the zoom target.
    /// </summary>
    /// <param name="_zoomChange"></param>
    public void AdjustZoomTarget(float _zoomChange)
    {
        float newZoom = Mathf.Clamp(targetZoom + (_zoomChange * zoomSensitivity), minZoom, maxZoom);
        targetZoom = newZoom;
    }

    private void UpdateCurrentZoom()
    {
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSpeed);
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
