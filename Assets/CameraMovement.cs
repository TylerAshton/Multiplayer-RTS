using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private RTSPlayerControls rtsPlayerControls;
    [SerializeField] private bool isPanning = false;
    [SerializeField] private float panMultiplier = 0.1f;
    [SerializeField] private float maxPanningSpeed = 1;
    [SerializeField] private float panningEdgeThreshold = 100;
    [SerializeField] private float minFOV = 15;
    [SerializeField] private float maxFOV = 60;
    [SerializeField] private float zoomSensitivity = 1;
    Camera camera;
    private Vector2 panStartPos;
    private Vector2 screenPosition => rtsPlayerControls.MouseScreenPos;

    float screenWidth = Screen.width;
    float screenHeight = Screen.height;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPanning)
        {
            ApplyPan(GetManualPanVector());
        }
        else
        {
            ApplyPan(isMouseNearScreenEdge());
        }
    }

    public void StartPanning(Vector2 _screenPosition)
    {
        panStartPos = _screenPosition;
        isPanning = true;
    }

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

    private void ApplyPan(Vector3 _panningVector)
    {
        transform.position += _panningVector * Time.deltaTime;
    }

    public void ApplyZoom(int axis)
    {
        camera.fieldOfView -= axis * zoomSensitivity; // Adjust FOV
        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, minFOV, maxFOV); // Clamp zoom range
    }

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
