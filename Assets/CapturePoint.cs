using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CapturePoint : NetworkBehaviour
{
    [SerializeField] float r = 10;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] private LayerMask mask;
    public int champs = 0;
    public int amalgs = 0;
    [SerializeField] private int minChamps = 0;
    [SerializeField] private int minAmalgs = 0;

    [SerializeField]
    public enum owners
    {
        AMALGAM,
        NEUTRAL,
        CHAMPION,
        CONTESTED
    }

    private List<GameObject> Champions = new List<GameObject>();
    private List<GameObject> PriorChampions = new List<GameObject>();
    private Dictionary<GameObject, Action> amalgamDeathHandlers = new();

    private Material[] materials;

    [SerializeField] GameObject circle;
    [SerializeField] ParticleSystem bonfire;
    [SerializeField] GameObject bonfireObj;
    [SerializeField] ShopManager shop;

    public owners owner = owners.NEUTRAL;

    private List<GameObject> goInCapture = new List<GameObject>();

    [SerializeField] GameObject icon;

    private void Awake()
    {
        circle.transform.localScale = new Vector3(r, 1, r);
        circle.transform.position = this.transform.position + offset;
        bonfireObj.transform.position = this.transform.position + offset;
        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.radius = r;
        trigger.center += offset;
        //networkObj = GetComponent<NetworkObject>();
    }

    /// <summary>
    /// Returns the ID of the champion if the player is a champion, otherwise returns 99.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private ulong CheckChampion(GameObject player)
    {
        if (!IsHost) { return 99; }

        if (player == null)
        {
            Debug.LogError("object is null in CheckChampion");
            return 99;
        }

        if (player.tag != "Champion")
        {
            Debug.LogError($"{player.name} is not tagged as champion");
            return 99;
        }

        for (int i = 1; i <= PlayerManager.Instance.getPlayerCount(); i++)
        {
            if (NetworkManager.Singleton.ConnectedClients[(ulong)i].PlayerObject.gameObject == player)
            {
                return (ulong)i;
            }
        }

        Debug.LogError($"{player.name} is not a champion");
        return 99;
    }

    void CheckOwner()
    {
        if (!IsHost) { return; }
        if (champs >= minChamps && amalgs == 0)
        {
            setOwnerRpc(2);
        }
        else if (amalgs >= minAmalgs && champs == 0)
        {
            setOwnerRpc(0);
        }
        else if (champs > 0 && amalgs > 0)
        {
            setOwnerRpc(3);
        }
    }

    [Rpc(SendTo.Everyone)]
    void setOwnerRpc(int ownerInt)
    {
        owner = (owners)ownerInt;
    }

    void Update()
    {
        if (!IsHost) { return; }
        CheckOwner();

        if (owner == owners.AMALGAM)
        {
            TurnOnBonfiresRpc(true, Color.red, 1);
        }
        else if (owner == owners.CHAMPION)
        {
            TurnOnBonfiresRpc(true, Color.blue, 2);
        }
        else if (owner == owners.CONTESTED)
        {
            TurnOnBonfiresRpc(true, Color.green, 0);
        }
        else
        {
            TurnOnBonfiresRpc(false, Color.black, 0);
        }

        icon = MinimapHandler.Instance.changeCampfire(icon, owner.ToString());
    }

    [Rpc(SendTo.Everyone)]
    void TurnOnBonfiresRpc(bool _state, Color _color, int _shopOwner)
    {
        if (_state)
        {
            bonfire.startColor = _color;
            circle.GetComponent<MeshRenderer>().material.color = _color;
        }
        bonfire.enableEmission = _state;
        shop.shopOwner = (ShopManager.shopOwners)_shopOwner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost) { return; }
        if (other.CompareTag("Champion"))
        {
            AddChampRpc(other.GetComponent<NetworkObject>());
        }
        else if (other.CompareTag("Amalgam"))
        {
            AddAmalgRpc(other.GetComponent<NetworkObject>());
            GameObject localAmalg = other.gameObject;
            SubscribeAmalgsOnDeath(localAmalg);
            /*            other.gameObject.GetComponent<Health>().OnDeath -= () => RemoveAmalgsOnDeath(localAmalg);
                        other.gameObject.GetComponent<Health>().OnDeath += () => RemoveAmalgsOnDeath(localAmalg);*/
            //targetHealth.OnDeath += ClearTarget;
        }

        CheckOwner();
        if (owner == owners.CHAMPION) // TODO: Delete this? as CheckOwner does this already
        {
            if (other.CompareTag("Champion"))
            {
                setShopStateRpc(CheckChampion(other.gameObject), true);
            }
            //setShopStateRpc(CheckChampion(other.gameObject), true);
        }
        else
        {
            foreach (GameObject go in goInCapture)
            {
                if (go.CompareTag("Champion"))
                {
                    setShopStateRpc(CheckChampion(go), false);
                    CloseShopRpc(CheckChampion(go));
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    void setShopStateRpc(ulong _ID, bool _state) // HERE
    {
        NetworkManager.Singleton.ConnectedClients[_ID].PlayerObject.gameObject.GetComponent<AnimatedChampion>().inShop = _state;
    }

    [Rpc(SendTo.Everyone)]
    void AddAmalgRpc(NetworkObjectReference amalg)
    {
        amalgs++;
        goInCapture.Add(amalg);
    }
    
    [Rpc(SendTo.Everyone)]
    void RemoveAmalgRpc(NetworkObjectReference amalg)
    {
        amalgs--;
        goInCapture.Remove(amalg);
    }

    [Rpc(SendTo.Everyone)]
    void AddChampRpc(NetworkObjectReference champ)
    {
        champs++;
        goInCapture.Add(champ);
    }

    [Rpc(SendTo.Everyone)]
    void RemoveChampRpc(NetworkObjectReference champ)
    {
        champs--;
        goInCapture.Remove(champ);
    }

    void RemoveAmalgsOnDeath(NetworkObjectReference amalg)
    {
        if (!IsHost) { return; }
        RemoveAmalgRpc(amalg);

        CheckOwner();
        if (owner == owners.CHAMPION)
        {
            foreach (GameObject go in goInCapture)
            {
                if (go.CompareTag("Champion"))
                {
                    setShopStateRpc(CheckChampion(go), true);
                }
            }
        }
        else
        {
            foreach (GameObject go in goInCapture)
            {
                if (go.CompareTag("Champion"))
                {
                    setShopStateRpc(CheckChampion(go), false);
                    CloseShopRpc(CheckChampion(go));
                }
            }
        }
    }

    /// <summary>
    /// Subscribes local amalgam's death event to remove itself from the capture point when it dies.
    /// </summary>
    /// <param name="_localAmalg"></param>
    private void SubscribeAmalgsOnDeath(GameObject _localAmalg)
    {
        if (!IsHost)
        { 
            Debug.LogWarning("SubscribeAmalgsOnDeath called on non-host client, ignoring.");
            return; 
        }
        if (_localAmalg == null)
        {
            Debug.LogError("Local Amalgam is null in SubscribeAmalgsOnDeath");
            return; 
        }

        Health _localAmalgHealth = _localAmalg.GetComponent<Health>();

        // If not already subscribed
        if (!amalgamDeathHandlers.ContainsKey(_localAmalg))
        {
            Action handler = () => RemoveAmalgsOnDeath(_localAmalg);
            _localAmalgHealth.OnDeath += handler;
            amalgamDeathHandlers[_localAmalg] = handler;
        }
    }

    /// <summary>
    /// Unsubscribes the lcal amalgam's death event from removing itself from the capture point when it dies.
    /// </summary>
    /// <param name="_localAmalg"></param>
    private void UnsubscribeAmalgsOnDeath(GameObject _localAmalg)
    {
        if (!IsHost)
        {
            Debug.LogWarning("UnsubscribeAmalgsOnDeath called on non-host client, ignoring.");
            return;
        }
        if (_localAmalg == null)
        {
            Debug.LogError("Local Amalgam is null in UnsubscribeAmalgsOnDeath");
            return;
        }

        // Unsubscribes from the death event then removes it from the dictionary
        if (amalgamDeathHandlers.TryGetValue(_localAmalg, out var handler))
        {
            Health _localAmalgHealth = _localAmalg.GetComponent<Health>();
            _localAmalgHealth.OnDeath -= handler;
            amalgamDeathHandlers.Remove(_localAmalg);
        }
        else
        {
            Debug.LogError($"No death handler found for {_localAmalg.name} in UnsubscribeAmalgsOnDeath");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsHost) { return; }
        if (other.CompareTag("Champion"))
        {
            setShopStateRpc(CheckChampion(other.gameObject), false);
            CloseShopRpc(CheckChampion(other.gameObject));
            RemoveChampRpc(other.GetComponent<NetworkObject>());
        }
        else if (other.CompareTag("Amalgam"))
        {
            GameObject localAmalg = other.gameObject;
            UnsubscribeAmalgsOnDeath(localAmalg);
            RemoveAmalgRpc(other.GetComponent<NetworkObject>());
            /*other.gameObject.GetComponent<Health>().OnDeath -= () => RemoveAmalgsOnDeath(localAmalg);*/
            //targetHealth.OnDeath -= ClearTarget;
        }

        CheckOwner();
        if (owner == owners.CHAMPION)
        {
            foreach(GameObject go in goInCapture)
            {
                if (go.CompareTag("Champion"))
                {
                    setShopStateRpc(CheckChampion(go), true);
                }
            }
        }
        else
        {
            foreach (GameObject go in goInCapture)
            {
                if (go.CompareTag("Champion"))
                {
                    setShopStateRpc(CheckChampion(go), false);
                    CloseShopRpc(CheckChampion(go));
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    void CloseShopRpc(ulong _ID)
    {
        NetworkManager.Singleton.ConnectedClients[_ID].PlayerObject.GetComponent<AnimatedChampion>().CloseShopUI();
    }

}

//RaycastHit[] units = Physics.SphereCastAll(this.transform.position + offset, r, Vector3.forward, 0, mask);

//foreach (RaycastHit unit in units)
//{
//    Debug.Log(unit.collider.transform.name);
//    if (unit.collider.transform.tag == "Champion")
//    {
//        //CheckChampion(unit.collider.transform.gameObject, true);
//        champs++;
//        Champions.Add(unit.collider.transform.gameObject);
//    }
//    else if (unit.collider.transform.tag == "Amalgam")
//    {
//        amalgs++;
//    }
//    else
//    {

//    }
//}

////foreach (GameObject go in Champions)
////{
////    Debug.Log(go.name);
////}

//for (int i = 0; i < PriorChampions.Count; i++)
//{
//    if (Champions.Count > 0)
//    {
//        if (PriorChampions.Contains(Champions[i]))
//        {
//            CheckChampion(Champions[i], true);
//        }
//        else
//        {
//            CheckChampion(Champions[i], false);
//        }
//    }
//    else
//    {
//        CheckChampion(PriorChampions[i], false);
//    }
//}

//PriorChampions = new List<GameObject>(Champions);
//Champions.Clear();
