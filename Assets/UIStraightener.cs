using UnityEngine;

/// <summary>
/// Hacky af script that makes parented ui face forward
/// </summary>
public class UIStraightener : MonoBehaviour
{
    Transform target;
    Vector3 offset;
    public void Init(Transform _target)
    {
        target = _target;

        offset = transform.position - target.position;
    }
    private void LateUpdate()
    {
        MoveUI();
        RotateUI();
    }

    private void MoveUI()
    {
        transform.position = target.position + offset;
    }

    /// <summary>
    /// Rotates the UI element to face the camera partially.
    /// </summary>
    private void RotateUI()
    {
        Vector3 targetDirection = (Camera.main.transform.position - transform.position).normalized;

        Quaternion fullRotation = Quaternion.LookRotation(targetDirection);
        Vector3 euler = fullRotation.eulerAngles;
        float xRotation = euler.x;
        float yRotation = euler.y;

        // Snap Y to face Coop or RTS
        if (yRotation > 180f)
        {
            yRotation = 270f;
        }
        else
        {
            yRotation = 90f;
        }

        Quaternion adjustedRotation = Quaternion.Euler(xRotation, yRotation, transform.rotation.eulerAngles.z);

        transform.rotation = adjustedRotation;
    }
}
