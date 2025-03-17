using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;

public class CoopPlayerManager : NetworkBehaviour
{
    public static CoopPlayerManager Instance;
    
    Vector2 movementVector;
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

    public void CheckMove(InputAction.CallbackContext context)
    {
        //moveHorizontal = Input.GetAxisRaw("Horizontal");
        //moveVertical = Input.GetAxisRaw("Vertical");
        //movement = new Vector2 (moveHorizontal, moveVertical).normalized;

        movementVector = context.ReadValue<Vector2>();
    }
}
