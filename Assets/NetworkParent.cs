using Unity.Netcode;
using UnityEngine;

public class NetworkParent : NetworkBehaviour
{
    private Transform parent;
    private void LateUpdate()
    {
        if (parent != null)
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
    }

    public void SetParent(Transform _parent)
    {
        parent = _parent;
    }

}
