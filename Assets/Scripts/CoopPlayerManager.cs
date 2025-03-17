using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;

public class CoopPlayerManager : NetworkBehaviour
{
    public static GameManger Instance;
    
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
        player = this.GameObject;
    }

    void Update()
    {
        if (!IsOwner) { return; }
        Move();
    }

    public void CheckMove(InputAction.CallbackContext context)
    {
        //moveHorizontal = Input.GetAxisRaw("Horizontal");
        //moveVertical = Input.GetAxisRaw("Vertical");
        //movement = new Vector2 (moveHorizontal, moveVertical).normalized;

        movementVector = context.ReadValue<Vector2>();
    }
}
