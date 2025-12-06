using NaughtyAttributes;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private int             _playerId = -1;
    [SerializeField] 
    private float           moveSpeed = 5.0f;
    [SerializeField] 
    private float           rotationSpeed = 360.0f;
    [SerializeField]
    private float           coalCooldown = 0.5f;
    [SerializeField]
    private float           shootStopTime = 0.2f;
    [SerializeField]
    private Transform       shootPoint;
    [SerializeField]
    private Coal            coalPrefab;
    [SerializeField]
    private Color           hitFlashColor = Color.red;
    [SerializeField]
    private float           hitFlashTime = 0.4f;
    [SerializeField]
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl moveControl;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl aimShootControl;

    Vector2         moveVector;
    Vector2         aimVector;
    Animator        animator;
    Rigidbody       rb;
    float           coalTimer;
    float           moveStopTimer;
    bool            shootEnable;
    Vector2         lastShootDir;
    ElfCustomizer   elfCustomizer;

    public int playerId => _playerId;

    void Start()
    {
        if (playerId >= 0)
        {
            MasterInputManager.SetupInput(playerId, playerInput);
            moveControl.playerInput = playerInput;
            aimShootControl.playerInput = playerInput;
        }
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        elfCustomizer = GetComponent<ElfCustomizer>();
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
        aimVector = aimShootControl.GetAxis2();

        if (moveStopTimer > 0.0f)
        {
            moveStopTimer -= Time.deltaTime;
            moveVector = Vector2.zero;
        }

        UpdateDirection();

        animator.SetFloat("AbsCurrentSpeed", rb.linearVelocity.x0z().magnitude);

        if ((coalTimer > 0.0f) && (!shootEnable))
        {
            coalTimer -= Time.deltaTime;
        }
        if (coalTimer <= 0.0f)
        {
            if (aimVector.magnitude > 0.3f)
            {
                shootEnable = true;
                animator.SetTrigger("Throw");

                lastShootDir = aimVector;
                coalTimer = coalCooldown;
                moveStopTimer = shootStopTime;
            }
        }
    }

    void UpdateDirection()
    {
        if (shootEnable)
        {
            transform.rotation = Quaternion.LookRotation(lastShootDir.x0y(), Vector3.up);
        }
        else
        {
            if (moveVector.magnitude > 0.1f)
            {
                var targetRotation = Quaternion.LookRotation(moveVector.x0y(), Vector3.up); ;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void ShootEnded()
    {
        shootEnable = false;
    }

    public void ReleaseCoal()
    {
        var coalInstance = Instantiate(coalPrefab, shootPoint.position, Quaternion.LookRotation(lastShootDir.x0y(), Vector3.up));
        coalInstance.Owner = playerId;
    }

    [Button("Trigger Hit")]
    public void HitCoal()
    {
        // Effect
        var material = elfCustomizer.material;

        material.SetColor("_Color_3", hitFlashColor);
        material.TweenColor(gameObject, "_Color_3", hitFlashColor.ChangeAlpha(0.0f), hitFlashTime);

        animator.SetTrigger("Hit");
    }
}
