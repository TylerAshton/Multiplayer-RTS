using UnityEngine;
using Unity.Netcode.Components;

/// <summary>
/// Changes the network transform to be Client Authorative instead of Server (DO NOT USE)
/// </summary>
/// 

public enum AuthorityMode
{
    Server,
    Client
}

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    public AuthorityMode Auth = (AuthorityMode)1;
    protected override bool OnIsServerAuthoritative() => Auth == (AuthorityMode)0;
}
