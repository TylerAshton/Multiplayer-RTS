using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;
using NUnit.Framework;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages all Coop players movement and logic
/// </summary>
public class CoopPlayerManager : NetworkBehaviour
{
    public static CoopPlayerManager Instance;

    Vector2 movementVector;
    public Dictionary<ulong, GameObject> playerPrefabs = new Dictionary<ulong, GameObject>();
    //[SerializeField] LocalPlayer local;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (!IsOwner) { return; }

    }

    //public void CheckMove(InputAction.CallbackContext context)
    //{
    //    //moveHorizontal = Input.GetAxisRaw("Horizontal");
    //    //moveVertical = Input.GetAxisRaw("Vertical");
    //    //movement = new Vector2 (moveHorizontal, moveVertical).normalized;

    //    movementVector = context.ReadValue<Vector2>();
    //}

    /// <summary>
    /// Adds the parsed Player Prefab into our dictionary of players.
    /// </summary>
    /// <param name="_Prefab"></param>
    /// <returns></returns>
    public void AddPlayer(ulong _ID, GameObject _Prefab)
    {
        try
        {
            playerPrefabs.Add(_ID, _Prefab);
            
        }
        catch (ArgumentException)
        {
            playerPrefabs.Remove(_ID);
            playerPrefabs.Add(_ID, _Prefab);
        }
    }

    public void AddPlayer(ulong _ID, NetworkObject _NetworkObject)
    {
        AddPlayer(_ID, _NetworkObject.gameObject);
    }
}