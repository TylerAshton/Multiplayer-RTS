using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;

public class LocalPlayer : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    RelayManager manager;
    Rigidbody rb;

    float moveHorizontal, moveVertical;
    Vector3 movementVector;

    void Start()
    {
        manager = RelayManager.Instance;
        rb = GetComponent<Rigidbody>();
        //manager.CreatePlayerServerRpc();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        MoveServerAuth();
    }

    void MoveServerAuth()
    {
        MoveServerRpc(movementVector);
    }

    [ServerRpc(RequireOwnership = false)]
    void MoveServerRpc(Vector3 movementVector)
    {
        rb.linearVelocity = movementVector * moveSpeed;
    }

    public void CheckMove(InputAction.CallbackContext context)
    {
        movementVector.x = context.ReadValue<Vector2>().x;
        movementVector.z = context.ReadValue<Vector2>().y;
    }
}
