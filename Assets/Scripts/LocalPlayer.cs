using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;

public class LocalPlayer : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    RelayManager manager = RelayManager.Instance;
    Rigidbody rb;

    float moveHorizontal, moveVertical;
    Vector2 movementVector;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //manager.CreatePlayerRpc();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        Move();
    }

    void Move()
    {
        rb.linearVelocity = movementVector * moveSpeed;
    }

    public void CheckMove(InputAction.CallbackContext context)
    {
        movementVector = context.ReadValue<Vector2>();
    }
}
