using Unity.Netcode;
using UnityEngine;

public class FMODSoundEnabler : NetworkBehaviour
{
    private void Awake()
    {


        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner)
        {
            gameObject.AddComponent<FMODUnity.StudioListener>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
