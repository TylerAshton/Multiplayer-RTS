using UnityEngine;

class ClericShop : Shop
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("I AM CLERIC");
    }
}

