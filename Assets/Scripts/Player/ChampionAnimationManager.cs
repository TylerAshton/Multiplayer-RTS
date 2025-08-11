using Unity.Netcode;
using UnityEngine;

public class ChampionAnimationManager : NetworkBehaviour
{
    [Header("Managers / Player Scripts")]
    [SerializeField] private ChampionManager championManager;
    [SerializeField] private ChampionMovement championMovement;

    [Header("Animation")]
    [SerializeField] private Vector3 movementRotationOffset = Vector3.zero;
    [SerializeField] private float smoothSpeed = 10f;

    private NetCodeAnimationManager nAnimator => championManager.NAnimator;
    private Vector3 velocity => championMovement.Velocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetAnimationParams(Vector3 _movementInput)
    {
        if (!IsServer)
        {
            Debug.LogError("Client attempted to update the animations!");
            return;
        }

        _movementInput = Quaternion.Euler(movementRotationOffset) * _movementInput;

        if (_movementInput.sqrMagnitude < 0.001f) // Smooth lerp to zero when idle
        {
            nAnimator.SetFloat("MoveX", Mathf.Lerp(nAnimator.GetFloat("MoveX"), 0f, smoothSpeed * Time.deltaTime));
            nAnimator.SetFloat("MoveY", Mathf.Lerp(nAnimator.GetFloat("MoveY"), 0f, smoothSpeed * Time.deltaTime));
            nAnimator.SetFloat("SpeedX", Mathf.Lerp(nAnimator.GetFloat("SpeedX"), 0f, smoothSpeed * Time.deltaTime));
            nAnimator.SetFloat("SpeedY", Mathf.Lerp(nAnimator.GetFloat("SpeedY"), 0f, smoothSpeed * Time.deltaTime));
            return;
        }

        // Normalize input to find local direction (relative)
        Vector3 inputDirection = _movementInput.normalized;
        float relativeX = Vector3.Dot(inputDirection, transform.right); // .Dot() Exists!! 
        float relativeZ = Vector3.Dot(inputDirection, transform.forward);


        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Smoothly update animation parameters
        nAnimator.SetFloat("MoveX", Mathf.Lerp(nAnimator.GetFloat("MoveX"), relativeX, smoothSpeed * Time.deltaTime));
        nAnimator.SetFloat("MoveY", Mathf.Lerp(nAnimator.GetFloat("MoveY"), relativeZ, smoothSpeed * Time.deltaTime));
        nAnimator.SetFloat("SpeedX", Mathf.Lerp(nAnimator.GetFloat("SpeedX"), Mathf.Abs(localVelocity.x), smoothSpeed * Time.deltaTime));
        nAnimator.SetFloat("SpeedY", Mathf.Lerp(nAnimator.GetFloat("SpeedY"), Mathf.Abs(localVelocity.z), smoothSpeed * Time.deltaTime));
    }
}
