using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private RTSPlayerControls rtsPlayerControls;
    [SerializeField] private bool isPanning = false;
    [SerializeField] private float panMultiplier = 0.1f;
    [SerializeField] private float maxPanningSpeed = 1;
    private Vector2 panStartPos;
    private Vector2 screenPosition => rtsPlayerControls.ScreenPosition;

    // Update is called once per frame
    void Update()
    {
        if (isPanning)
        {
            PanCamera();
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

    private void PanCamera()
    {
        Vector2 direction = screenPosition - panStartPos;

        // convert to vector 3
        Vector3 panningVector = new Vector3 { x = direction.x, y = 0, z = direction.y };

        panningVector = panningVector * panMultiplier;
        panningVector = Vector3.ClampMagnitude(panningVector, maxPanningSpeed);

        transform.position += panningVector;
    }

}
