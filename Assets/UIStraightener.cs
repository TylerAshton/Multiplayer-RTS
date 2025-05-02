using UnityEngine;

/// <summary>
/// Hacky af script that makes parented ui face forward
/// </summary>
public class UIStraightener : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward);
    }
}
