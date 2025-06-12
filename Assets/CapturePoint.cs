using System.Collections.Generic;
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

    private NetworkObject networkObj;

    private void Awake()
    {
        circle.transform.localScale = new Vector3(r, 1, r);
        circle.transform.position = this.transform.position + offset;
        bonfireObj.transform.position = this.transform.position + offset;
        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.radius = r;
        trigger.center += offset;
        networkObj = GetComponent<NetworkObject>();
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
            owner = owners.CHAMPION;
        }
        else if (amalgs >= minAmalgs && champs == 0)
        {
            owner = owners.AMALGAM;
        }
        else if (champs > 0 && amalgs > 0)
        {
            owner = owners.CONTESTED;
        }
    }

    void Update()
    {
        if (!IsHost) { return; }
        CheckOwner();

        if (owner == owners.AMALGAM)
        {
            TurnOnBonfiresRpc(true, Color.red, 1);
            //bonfire.enableEmission = true;
            //bonfire.startColor = Color.red;
            //circle.GetComponent<MeshRenderer>().material.color = Color.red;
            //shop.shopOwner = ShopManager.shopOwners.AMALGAM;
        }
        else if (owner == owners.CHAMPION)
        {
            TurnOnBonfiresRpc(true, Color.blue, 2);
            //bonfire.enableEmission = true;
            //bonfire.startColor = Color.blue;
            //circle.GetComponent<MeshRenderer>().material.color = Color.blue;
            //shop.shopOwner = ShopManager.shopOwners.CHAMPION;
        }
        else if (owner == owners.CONTESTED)
        {
            TurnOnBonfiresRpc(true, Color.green, 0);
            //bonfire.enableEmission = true;
            //bonfire.startColor = Color.green;
            //circle.GetComponent<MeshRenderer>().material.color = Color.green;
            //shop.shopOwner = ShopManager.shopOwners.NONE;
        }
        else
        {
            TurnOnBonfiresRpc(false, Color.black, 0);
            //bonfire.enableEmission = false;
            //circle.GetComponent<MeshRenderer>().material.color = Color.grey;
            //shop.shopOwner = ShopManager.shopOwners.NONE;
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
            champs++;
            CheckOwner();
            if (owner == owners.CHAMPION)
            {
                setShopStateRpc(CheckChampion(other.gameObject), true);
            }
        }
        else if (other.CompareTag("Amalgam"))
        {
            amalgs++;
            CheckOwner();
            other.gameObject.GetComponent<Health>().OnDeath -= RemoveAmalgsOnDeath;
            other.gameObject.GetComponent<Health>().OnDeath += RemoveAmalgsOnDeath;
            //targetHealth.OnDeath += ClearTarget;
        }
    }

    [Rpc(SendTo.Everyone)]
    void setShopStateRpc(ulong _ID, bool _state)
    {
        NetworkManager.Singleton.ConnectedClients[_ID].PlayerObject.gameObject.GetComponent<AnimatedChampion>().inShop = _state;
    }

    void RemoveAmalgsOnDeath()
    {
        if (!IsHost) { return; }
        amalgs--;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsHost) { return; }
        if (other.CompareTag("Champion"))
        {
            setShopStateRpc(CheckChampion(other.gameObject), false);
            CloseShopRpc(CheckChampion(other.gameObject));
            champs--;
        }
        else if (other.CompareTag("Amalgam"))
        {
            other.gameObject.GetComponent<Health>().OnDeath -= RemoveAmalgsOnDeath;
            amalgs--;
            //targetHealth.OnDeath -= ClearTarget;
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
