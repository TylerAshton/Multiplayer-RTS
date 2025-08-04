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

/*        Vector3 targetDirection = (Camera.main.transform.position - transform.position).normalized;

        // Calculate the desired rotation with LookRotation
        Quaternion fullRotation = Quaternion.LookRotation(targetDirection);

        // Extract only the X rotation (pitch)
        Vector3 euler = fullRotation.eulerAngles;
        float xRotation = euler.x;

        // Construct a new Quaternion with only X rotation, keeping Y and Z as original
        Quaternion limitedRotation = Quaternion.Euler(xRotation, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        // Apply limited rotation
        transform.rotation = limitedRotation;*/
    }

    private void MoveUI()
    {
        transform.position = target.position + offset;
    }
}
