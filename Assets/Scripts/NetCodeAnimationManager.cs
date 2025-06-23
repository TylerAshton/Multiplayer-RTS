using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class manages animation syncronisation across the network as NetCode doesn't handle this very well.
/// Use this as opposed to the Animator component directly to ensure that all clients see the same animations.
/// </summary>
public class NetCodeAnimationManager : NetworkBehaviour
{
    private Animator animator;

    private void Awake()
    {
        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError($"Animator is required for {this.name}");
        }
    }

    public void SetTrigger(string _triggerName)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to set animation triggers");
            return;
        }

        SetTriggerClientRpc(_triggerName);
    }

    [ClientRpc]
    private void SetTriggerClientRpc(string _triggerName)
    {
        animator.SetTrigger(_triggerName);
    }

    /// <summary>
    /// This works the same as Animator.SetBool.
    /// </summary>
    /// <param name="param"></param>
    /// <param name="value"></param>
    public void SetFloat(string param, float value)
    {
        if (!IsServer)
        {
            //Debug.LogError("Client attempted to set animation floats");
            return;
        }
        animator.SetFloat(param, value);
    }

    /// <summary>
    /// Works the same as Animator.GetFloat
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public float GetFloat(string param)
    {
        return animator.GetFloat(param);
    }

}
