using UC;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] 
    private float           moveSpeed = 5.0f;
    [SerializeField] 
    private float           rotationSpeed = 360.0f;
    [SerializeField]
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl moveControl;

    Vector2     moveVector;
    Animator    animator;
    Rigidbody   rb;

    void Start()
    {
        moveControl.playerInput = playerInput;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        var tmp = moveVector.x0y() * moveSpeed;
        tmp.y = rb.linearVelocity.y;
        rb.linearVelocity = tmp;
    }

    void Update()
    {
        moveVector = moveControl.GetAxis2();

        if (moveVector.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(moveVector.x0y(), Vector3.up); ;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        animator.SetFloat("AbsCurrentSpeed", rb.linearVelocity.x0z().magnitude);
    }
}
