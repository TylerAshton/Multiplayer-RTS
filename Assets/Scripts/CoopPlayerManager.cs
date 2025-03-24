using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;
using NUnit.Framework;
using System.Collections.Generic;

/// <summary>
/// Manages all Coop players movement and logic
/// </summary>
public class CoopPlayerManager : NetworkBehaviour
{
    public static CoopPlayerManager Instance;
    
    Vector2 movementVector;
    Dictionary<string, LocalPlayer> localPlayers = new Dictionary<string, LocalPlayer>();
    GameObject player;
    [SerializeField] LocalPlayer local;

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
    /// Adds the parsed localPlayer into our dictionary of players.
    /// </summary>
    /// <param name="_localPlayer"></param>
    /// <returns></returns>
    public void AddPlayer(string _ID, LocalPlayer _localPlayer)
    {
        localPlayers.Add(_ID, _localPlayer);
    }


    public void AddPlayer(string _ID, GameObject _localPlayerGamobject)
    {
        LocalPlayer localPlayer = _localPlayerGamobject.GetComponent<LocalPlayer>();

        localPlayers.Add(_ID, localPlayer);
    }
}
