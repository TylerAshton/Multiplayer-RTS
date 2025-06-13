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

    private Material[] materials;

    [SerializeField] GameObject circle;
    [SerializeField] ParticleSystem bonfire;
    [SerializeField] GameObject bonfireObj;
    [SerializeField] ShopManager shop;

    public owners owner = owners.NEUTRAL;

    private List<GameObject> goInCapture = new List<GameObject>();

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

    private ulong CheckChampion(GameObject player)
    {
        if (!IsHost) { return 99; }
        for (int i = 1; i <= PlayerManager.Instance.getPlayerCount(); i++)
        {
            if (NetworkManager.Singleton.ConnectedClients[(ulong)i].PlayerObject.gameObject == player)
            {
                return (ulong)i;
            }
        }
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
            other.gameObject.GetComponent<Health>().OnDeath -= () => RemoveAmalgsOnDeath(localAmalg);
            other.gameObject.GetComponent<Health>().OnDeath += () => RemoveAmalgsOnDeath(localAmalg);
            //targetHealth.OnDeath += ClearTarget;
        }

        CheckOwner();
        if (owner == owners.CHAMPION)
        {
            setShopStateRpc(CheckChampion(other.gameObject), true);
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
    void setShopStateRpc(ulong _ID, bool _state)
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
            other.gameObject.GetComponent<Health>().OnDeath -= () => RemoveAmalgsOnDeath(localAmalg);
            RemoveAmalgRpc(other.GetComponent<NetworkObject>());
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
