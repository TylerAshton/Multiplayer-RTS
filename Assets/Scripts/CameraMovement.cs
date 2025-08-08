using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Moves the panning target to a clamped position within the bounds of the map based on how far the camera is zoomed out.
    /// </summary>
    private void ClampPanningTargetToBounds() // TODO: This has a minor flaw due to the camera being at an angle, we will get different corners. If we could somehow use a 2nd camera or something to get the bounds of the camera view, that would be better. But it's good enough for showcase
    {
        Vector3[] corners = GetCameraCornersOnTargetPlane();
        Bounds camBounds = ConvertCornersToBounds(corners);

        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(panningTarget.position.x, MapManager.MapBounds.min.x + camBounds.extents.x, MapManager.MapBounds.max.x - camBounds.extents.x),
            panningTarget.position.y,
            Mathf.Clamp(panningTarget.position.z, MapManager.MapBounds.min.z + camBounds.extents.z, MapManager.MapBounds.max.z - camBounds.extents.z)
        );

/*        Debug.DrawLine(panningTarget.position, panningTarget.position + new Vector3(camBounds.extents.x, 0, 0), Color.red);*/

        panningTarget.position = clampedPos;
    }

    private void ApplyZoom()
    {
        //panningTarget.transform.position = startPosition + (mainCamera.transform.forward * targetZoom);
        Vector3 offset = originalFollowOffset + (mainCamera.transform.forward * currentZoom);
        transposer.m_FollowOffset = offset;

        // Reclamp as corner bounds have changed due to zoom
        ClampPanningTargetToBounds();
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
        Vector3 newPosition = panningTarget.position += _panningVector * Time.deltaTime;
        panningTarget.position = newPosition;

        // Clamp to bounds
        ClampPanningTargetToBounds();
    }

    /// <summary>
    /// Legit just converts the 4 corners to a Bounds object.
    /// </summary>
    /// <param name="_corners"></param>
    /// <returns></returns>
    private Bounds ConvertCornersToBounds(Vector3[] _corners)
    {
        if (_corners.Length != 4)
        {
            Debug.LogError("Invalid number of corners provided. Expected 4.");
            return new Bounds();
        }

        Vector3 min = _corners[0];
        Vector3 max = _corners[0];
        for (int i = 1; i < _corners.Length; i++)
        {
            min = Vector3.Min(min, _corners[i]);
            max = Vector3.Max(max, _corners[i]);
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    /// <summary>
    /// Returns the 4 corners of the camera's view to the cameratarget plane.
    /// </summary>
    /// <returns></returns>
    private Vector3[] GetCameraCornersOnTargetPlane()
    {
        Plane targetPlane = new Plane(Vector3.up, panningTarget.position); // Deterministic plane instead of the boundsSurface
        Vector3[] screenCorners = new Vector3[4];
        Vector3[] corners = new Vector3[4];

        Vector3 topLeft = new Vector3(0, Screen.height);
        Vector3 topRight = new Vector3(Screen.width, Screen.height);
        Vector3 bottomLeft = new Vector3(0, 0);
        Vector3 bottomRight = new Vector3(Screen.width, 0);

        screenCorners[0] = topLeft;
        screenCorners[1] = topRight;
        screenCorners[2] = bottomLeft;
        screenCorners[3] = bottomRight;

        Camera cam = Camera.main;
        for (int i = 0; i < 4; i++)
        {
            Ray ray = cam.ScreenPointToRay(screenCorners[i]);
            if (targetPlane.Raycast(ray, out float enter))
            {
                corners[i] = ray.GetPoint(enter);
            }
            else
            {
                Debug.LogError("Ray did not intersect with target plane.");
            }
        }

        return corners;
    }

    private void OnDrawGizmos()
    {
        Vector3[] corners = GetCameraCornersOnTargetPlane();

        foreach (Vector3 corr in corners)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(corr, 5);
        }

        /*Debug.Log($"Distance from panning target to camera bounds (X,Z): {camBounds.extents}");*/
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

    /*/// <summary>
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

        if (isNearLeft) 
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
    }*/

    /*/// <summary> A previous approach to clamping that didn't work as it relied on the camera corners which would change constantly
    /// Returns a clamp pos to keep the corners Vector3[] pos's within the bounds given. Used to panning clamps
    /// </summary>
    /// <param name="_position"></param>
    /// <param name="_bounds"></param>
    /// <returns></returns>
    private Vector3 ClampToBounds(Vector3 _position, Vector3[] _worldCorners, Bounds _bounds)
    {
        List<Vector3> adjustmentVectors = new List<Vector3>();

        // All vectors given should be actual valeus
        if (_worldCorners.Any(n => n == Vector3.zero))
        {
            Debug.LogError("Invalid corner position");
            return Vector3.zero;
        }

        // Begin churning through the corners to see if they're out of bounds, and if so assign an adjustmentVector to correct the position
        for (int i = 0; i < _worldCorners.Length; i++)
        {
            Vector3 corner = _worldCorners[i];
            corner.y = _bounds.center.y; // Don't check z bounds


            if (_bounds.Contains(corner))
            {
                continue;
            }

            Vector3 closestBound = _bounds.ClosestPoint(corner);

            // Combine vectors together
            Vector3 adjustmentVector = closestBound - corner;
            adjustmentVectors.Add(adjustmentVector);
            
            Debug.Log($"{adjustmentVector}: Corner {i}!");
        }

        // Don't fucking bother if there's no adjustment to combine
        if (adjustmentVectors.Count == 0)
        {
            return _position;
        }

        // Combine all adjustment vectors together so we don't double do things.
        // We do this by taking highest abs value, but we retain the sign (the + or -)
        Vector3 combinedVector = Vector3.zero;
        foreach (Vector3 adjustmentVector in adjustmentVectors)
        {
            combinedVector.x = Mathf.Abs(combinedVector.x) > Mathf.Abs(adjustmentVector.x) ? combinedVector.x : adjustmentVector.x;
            combinedVector.y = Mathf.Abs(combinedVector.y) > Mathf.Abs(adjustmentVector.y) ? combinedVector.y : adjustmentVector.y;
            combinedVector.z = Mathf.Abs(combinedVector.z) > Mathf.Abs(adjustmentVector.z) ? combinedVector.z : adjustmentVector.z;
        }

        Vector3 newPosition = _position + combinedVector;

        return newPosition;
    }*/

    /*private Vector3[] GetCameraCorners() // This previous approach worked fine but it was a bit dependent on a debug boundsSurface
    {
        Vector3[] output = new Vector3[4];

        LayerMask environmentMask = LayerMask.GetMask("BoundsSurface");

        Vector3 topLeft = Vector3.zero;
        Vector3 topRight = Vector3.zero;
        Vector3 bottomLeft = Vector3.zero;
        Vector3 bottomRight = Vector3.zero;

        // 
        Ray rayBL = Camera.main.ScreenPointToRay(new Vector3(0, 0, 0));
        Ray rayTL = Camera.main.ScreenPointToRay(new Vector3(0, screenHeight, 0));
        Ray rayBR = Camera.main.ScreenPointToRay(new Vector3(screenWidth, 0, 0));
        Ray rayTR = Camera.main.ScreenPointToRay(new Vector3(screenWidth, screenHeight, 0));

        if (Physics.Raycast(rayBL, out RaycastHit hitBL, Mathf.Infinity, environmentMask))
        {
            bottomLeft = hitBL.point;
        }
        if (Physics.Raycast(rayTL, out RaycastHit hitTL, Mathf.Infinity, environmentMask))
        {
            topLeft = hitTL.point;
        }
        if (Physics.Raycast(rayBR, out RaycastHit hitBR, Mathf.Infinity, environmentMask))
        {
            bottomRight = hitBR.point;
        }
        if (Physics.Raycast(rayTR, out RaycastHit hitTR, Mathf.Infinity, environmentMask))
        {
            topRight = hitTR.point;
        }

        output[0] = bottomLeft;
        output[1] = topLeft;
        output[2] = bottomRight;
        output[3] = topRight;

        return output;
    }*/

}
