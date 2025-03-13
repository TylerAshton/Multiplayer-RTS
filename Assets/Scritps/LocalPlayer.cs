using UnityEngine;
using Unity.Netcode;
using System.Globalization;

public class LocalPlayer : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 4f;

    Rigidbody rb;

    float moveHorizontal, moveVertical;
    Vector2 movement;

    int facingDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        Move();
    }

    void Move()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");
        movement = new Vector2 (moveHorizontal, moveVertical).normalized;
        rb.linearVelocity = movement * moveSpeed;
    }
}
