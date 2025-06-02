using UnityEngine;

class KnightShop : Shop
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("I AM KNIGHT");
    }
}

