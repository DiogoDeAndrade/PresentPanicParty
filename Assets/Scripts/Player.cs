using NaughtyAttributes;
using System;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class Player : MonoBehaviour
{
    [SerializeField]
    private int             _playerId = -1;
    [SerializeField] 
    private float           moveSpeed = 5.0f;
    [SerializeField] 
    private float           carryMoveMultiplier = 1.0f;
    [SerializeField] 
    private float           rotationSpeed = 360.0f;
    [SerializeField]
    private float           coalCooldown = 0.5f;
    [SerializeField]
    private float           shootStopTime = 0.2f;
    [SerializeField]
    private Transform       shootPoint;
    [Header("Coal")]
    [SerializeField]
    private Coal            coalPrefab;
    [SerializeField]
    private int             startCoal;
    [SerializeField]
    private int             maxCoal;
    [SerializeField]
    private float           coalGatherDuration = 1.0f;
    [Header("Hit")]
    [SerializeField]
    private Color           hitFlashColor = Color.red;
    [SerializeField]
    private float           hitFlashTime = 0.4f;
    [SerializeField]
    private int             maxStun = 2;
    [SerializeField]
    private float           stunDuration = 10.0f;
    [SerializeField]
    private GameObject      stunEffectPrefab;
    [SerializeField]
    private Transform       stunEffectSpawnPoint;
    [Header("Carry")]
    [SerializeField]
    private float           carryCooldown = 1.0f;
    [SerializeField]
    private Transform       carryPoint;
    [SerializeField]
    private GameObject      dropEffectPrefab;
    [Header("Input")]
    [SerializeField]
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl moveControl;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl aimShootControl;
    [Header("UI")]
    [SerializeField]
    private Hypertag        mainCanvasTag;
    [SerializeField]
    private PlayerUI        playerUIPrefab;
    [SerializeField]
    private Transform       uiPoint;

    Vector2         moveVector;
    Vector2         aimVector;
    Animator        animator;
    Rigidbody       rb;
    float           coalTimer;
    float           moveStopTimer;
    bool            shootEnable;
    Vector2         lastShootDir;
    ElfCustomizer   elfCustomizer;
    PlayerUI        playerUI;
    int             _coalCount;
    float           coalGatherTime;
    int             stun;
    float           getUpTimer;
    bool            _invulnerable;
    GameObject      stunEffect;
    Gift            carryObj;
    float           carryTimer;
    int             _score;

    public int playerId => _playerId;
    public int coalCount => _coalCount;
    public float coalGatherProgress => coalGatherTime / coalGatherDuration;
    public bool invulnerable => _invulnerable;
    public int  score => _score;

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

        var canvas = mainCanvasTag.FindFirst<Canvas>();
        playerUI = Instantiate(playerUIPrefab, canvas.transform);
        playerUI.Player = this;
        playerUI.trackedObject = uiPoint;

        _coalCount = startCoal;
    }

    private void OnDestroy()
    {
        if (playerUI)
        {
            Destroy(playerUI.gameObject);
        }
    }

    private void FixedUpdate()
    {
        float s = moveSpeed;
        if (isCarrying) s *= carryMoveMultiplier;
        var tmp = moveVector.x0y() * s;
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
        if ((coalTimer <= 0.0f) && (_coalCount > 0))
        {
            if (aimVector.magnitude > 0.3f)
            {
                ReleaseCarry();

                shootEnable = true;
                animator.SetTrigger("Throw");

                lastShootDir = aimVector;
                coalTimer = coalCooldown;
                moveStopTimer = shootStopTime;

                _coalCount--;
            }
        }

        if (getUpTimer > 0.0f)
        {
            getUpTimer -= Time.deltaTime;
            if (getUpTimer <= 0.0f)
            {
                GetUp();
            }
        }

        if (carryTimer > 0.0f)
        {
            carryTimer -= Time.deltaTime;
        }

        if (carryObj)
        {
            carryObj.transform.position = carryPoint.position;
            carryObj.transform.rotation = carryPoint.rotation;
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

    public void HitCoal()
    {
        // Effect
        var material = elfCustomizer.material;

        material.SetColor("_Color_3", hitFlashColor);
        material.TweenColor(gameObject, "_Color_3", hitFlashColor.ChangeAlpha(0.0f), hitFlashTime);

        stun++;
        if (stun >= maxStun)
        {
            Stun();
        }
        else
        {
            animator.SetTrigger("Hit");
        }
    }

    public void SpawnStunEffect()
    {
        stunEffect = Instantiate(stunEffectPrefab, stunEffectSpawnPoint.position, stunEffectSpawnPoint.rotation);
    }

    void Stun()
    {
        ReleaseCarry();

        moveStopTimer = float.MaxValue;
        animator.SetLayerWeight(animator.GetLayerIndex("Override"), 1.0f);
        animator.SetTrigger("Stun");
        getUpTimer = stunDuration;
        _invulnerable = true;
    }

    void GetUp()
    {
        if (stunEffect) Destroy(stunEffect);
        animator.SetTrigger("GetUp");
    }

    public void RestartMove()
    {
        animator.SetLayerWeight(animator.GetLayerIndex("Override"), 0.0f);
        moveStopTimer = 0.0f;
        stun = 0;
        _invulnerable = false;
    }

    internal void AddGatherCoal(float deltaTime)
    {
        if (_coalCount < maxCoal)
        {
            coalGatherTime += deltaTime;
            if (coalGatherTime > coalGatherDuration)
            {
                _coalCount++;
                ResetGatherCoal();
            }
        }
        else
        {
            coalGatherTime = 0.0f;
        }
    }

    internal void ResetGatherCoal()
    {
        coalGatherTime = 0.0f;
    }

    public bool isCarrying => (carryObj != null);

    internal bool Carry(Gift gift)
    {
        if (carryObj) return false;
        if (moveStopTimer > 0.0f) return false;
        if (_invulnerable) return false;

        carryObj = gift;
        carryTimer = carryCooldown;

        return true;
    }

    void ReleaseCarry()
    {
        if (carryObj == null) return;
        carryObj.Release();
        carryObj = null;

        carryTimer = carryCooldown;
    }
    public void DropGift(Bag bag)
    {
        if (carryObj == null) return;
        Destroy(carryObj.gameObject);

        Instantiate(dropEffectPrefab, bag.transform.position + Vector3.up * 0.1f, Quaternion.identity);

        if (Globals.ownerWinsDrop)
            bag.Player._score++;
        else
            _score++;
    }

    public static Player FindPlayerById(int playerId)
    {
        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.playerId == playerId) return player;
        }

        return null;
    }
}
