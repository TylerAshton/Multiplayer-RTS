using UnityEngine;
using Unity.Netcode.Components;

/// <summary>
/// Changes the network transform to be Client Authorative instead of Server (DO NOT USE)
/// </summary>
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
