using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
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
    private float           carryMoveMultiplier = 1.0f;
    [SerializeField] 
    private float           rotationSpeed = 360.0f;
    [SerializeField]
    private float           coalCooldown = 0.5f;
    [SerializeField]
    private float           shootStopTime = 0.2f;
    [SerializeField]
    private Transform       shootPoint;
    [SerializeField]
    private Renderer        elfRenderer;       
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
    private Transform[]     carryPoints;
    [SerializeField]
    private int             elfMaxCarry = 1;
    [SerializeField]
    private int             krampusMaxCarry = 4;
    [SerializeField]
    private GameObject      dropEffectPrefab;
    [Header("Essence")]
    [SerializeField]
    private int             _maxEssence = 20;
    [SerializeField]
    private Color           essenceFlashColor = Color.yellow;
    [SerializeField]
    private float           essenceFlashTime = 0.4f;
    [SerializeField]
    private GameObject      krampusRoot;
    [SerializeField]
    private Renderer        krampusRenderer;
    [SerializeField]
    private float           krampusEssenceDrain = 2.0f;
    [SerializeField]
    private GameObject      krampusEffectPrefab;
    [SerializeField]
    private GameObject      bloodPoolPrefab;
    [SerializeField]
    private GameObject      bloodSplatterPrefab;
    [Header("Soul")]
    [SerializeField]
    private GameObject      soulEffectPrefab;
    [SerializeField]
    private GameObject      soulPrefab;
    [Header("Input")]
    [SerializeField]
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl moveControl;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl aimShootControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl toggleKrampus;
    [Header("UI")]
    [SerializeField]
    private Hypertag        mainCanvasTag;
    [SerializeField]
    private PlayerUI        playerUIPrefab;
    [SerializeField]
    private Transform       uiPoint;

    Vector2         moveVector;
    Vector2         aimVector;
    Animator        elfAnimator;
    Animator        krampusAnimator;
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
    List<Gift>      carryObjs;
    float           carryTimer;
    int             _score;
    float           _essence;
    bool            isKrampus;
    Material        krampusMaterial;
    bool            attacking;

    public int playerId => _playerId;
    public int coalCount => (isKrampus) ? (0) : (_coalCount);
    public float coalGatherProgress => coalGatherTime / coalGatherDuration;
    public bool invulnerable => _invulnerable || isKrampus;
    public int  score => _score;
    public float essence => _essence;
    public int maxEssence => _maxEssence;
    public float essencePercentage => _essence / (float)_maxEssence;

    void Start()
    {
        if (playerId >= 0)
        {
            StartCoroutine(SetupInputCR());
        }

        rb = GetComponent<Rigidbody>();
        elfAnimator = GetComponent<Animator>();
        krampusAnimator = krampusRoot.GetComponent<Animator>();
        elfCustomizer = GetComponent<ElfCustomizer>();

        var canvas = mainCanvasTag.FindFirst<Canvas>();
        playerUI = Instantiate(playerUIPrefab, canvas.transform);
        playerUI.Player = this;
        playerUI.trackedObject = uiPoint;

        _coalCount = startCoal;

        krampusRenderer.enabled = false;
        krampusMaterial = new Material(krampusRenderer.material);
        krampusRenderer.material = krampusMaterial;

        carryObjs = new();
        for (int i = 0; i < Mathf.Max(elfMaxCarry, krampusMaxCarry); i++)
        {
            carryObjs.Add(null);
        }
    }

    IEnumerator SetupInputCR()
    {
        yield return new WaitForSeconds(0.05f * playerId);

        MasterInputManager.SetupInput(playerId, playerInput);
        moveControl.playerInput = playerInput;
        aimShootControl.playerInput = playerInput;
        toggleKrampus.playerInput = playerInput;
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
        if (!rb.isKinematic)
        {
            float s = moveSpeed;
            if (isCarrying) s *= carryMoveMultiplier;
            var tmp = moveVector.x0y() * s;
            tmp.y = rb.linearVelocity.y;
            rb.linearVelocity = tmp;
        }
    }

    void Update()
    {
        if (!playerInput.enabled) return;

        moveVector = moveControl.GetAxis2();
        aimVector = aimShootControl.GetAxis2();

        if (moveStopTimer > 0.0f)
        {
            moveStopTimer -= Time.deltaTime;
            moveVector = Vector2.zero;
        }

        UpdateDirection();

        elfAnimator.SetFloat("AbsCurrentSpeed", rb.linearVelocity.x0z().magnitude);
        krampusAnimator.SetFloat("AbsCurrentSpeed", rb.linearVelocity.x0z().magnitude);

        if ((coalTimer > 0.0f) && (!shootEnable))
        {
            coalTimer -= Time.deltaTime;
        }
        if ((coalTimer <= 0.0f) && (_coalCount > 0) && (!isKrampus))
        {
            if (aimVector.magnitude > 0.3f)
            {
                ReleaseCarry();

                shootEnable = true;
                elfAnimator.SetTrigger("Throw");

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

        for (int i = 0; i < carryObjs.Count; i++)
        {
            if (carryObjs[i])
            {
                carryObjs[i].transform.position = carryPoints[i].position;
                carryObjs[i].transform.rotation = carryPoints[i].rotation;
            }
        }

        if (isKrampus)
        {
            _essence = Mathf.Max(0, _essence - krampusEssenceDrain * Time.deltaTime);
            if (_essence <= 0.0f)
            {
                TransformToElf();
            }
            else 
            {
                if (!attacking)
                {
                    // Check if there's another player in range
                    var playersInRange = GetPlayersInRange(2.0f);
                    if (playersInRange.Count > 0)
                    {
                        attacking = true;
                        krampusAnimator.SetTrigger("Swipe");
                    }
                }
            }
        }
        else
        {
            if ((toggleKrampus.IsDown()) && (moveStopTimer <= 0.0f) && (essence == _maxEssence))
            {
                TransformToKrampus();
            }
        }
    }

    List<Player> GetPlayersInRange(float radius)
    {
        List<Player> ret = new();
        var colliders = Physics.OverlapSphere(transform.position, radius, 1 << gameObject.layer);
        foreach (var collider in colliders)
        {
            var otherPlayer = collider.GetComponent<Player>();
            if ((otherPlayer != this) && (!otherPlayer.invulnerable))
            {
                Vector3 toEnemy = (otherPlayer.transform.position - transform.position).normalized;
                float   angle = Vector3.Angle(transform.forward, toEnemy);
                if (angle < 45.0f)
                {
                    ret.Add(otherPlayer);
                }
            }
        }

        return ret;
    }

    public void FinishAttack()
    {
        attacking = false;
        var players = GetPlayersInRange(1.5f);
        foreach (var player in players)
        {
            player.Kill();
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
        ReleaseCarry();

        // Effect
        FlashColor(hitFlashColor, hitFlashTime);

        stun++;
        if (stun >= maxStun)
        {
            Stun();
        }
        else
        {
            elfAnimator.SetTrigger("Hit");
        }
    }

    private void FlashColor(Color flashColor, float flashTime)
    {
        if (isKrampus)
        {
            krampusMaterial.SetColor("_Color_3", flashColor);
            krampusMaterial.TweenColor(gameObject, "_Color_3", flashColor.ChangeAlpha(0.0f), flashTime);
        }
        else
        {
            var material = elfCustomizer.material;

            material.SetColor("_Color_3", flashColor);
            material.TweenColor(gameObject, "_Color_3", flashColor.ChangeAlpha(0.0f), flashTime);
        }
    }

    public void SpawnStunEffect()
    {
        if (getUpTimer > 0.0f)
        {
            stunEffect = Instantiate(stunEffectPrefab, stunEffectSpawnPoint.position, stunEffectSpawnPoint.rotation);
        }
    }

    [Button("Debug: Kill")]
    void Kill()
    {
        ReleaseCarry();

        FlashColor(hitFlashColor, hitFlashTime);
        moveStopTimer = float.MaxValue;
        elfAnimator.SetLayerWeight(elfAnimator.GetLayerIndex("Override"), 1.0f);
        elfAnimator.SetTrigger("Stun");
        _invulnerable = true;

        EnablePhysics(false);

        StartCoroutine(KillCR());
    }

    public void Ressurrect()
    {
        StartCoroutine(RessurrectCR());
    }

    IEnumerator RessurrectCR()
    {
        Instantiate(soulEffectPrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.1f);
        elfRenderer.enabled = false;
        yield return new WaitForSeconds(0.5f);

        Bag bag = Bag.FindBagById(playerId);
        var spawnPos = bag.SpawnPoint;
        transform.position = spawnPos.position;

        Instantiate(soulEffectPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.1f);
        elfRenderer.enabled = true;
        GetUp();
    }

    IEnumerator KillCR()
    {
        for (int i = 0; i < 4; i++)
        {
            var pt = LevelManager.GetRandomPositionOnNavMesh(transform.position, 1.0f);
            if (pt != Vector3.zero)
            {
                Instantiate(bloodSplatterPrefab, pt + Vector3.up * 0.025f, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(1.5f);

        Instantiate(bloodPoolPrefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);

        Instantiate(soulPrefab, transform.position, transform.rotation);
    }

    void Stun()
    {
        moveStopTimer = float.MaxValue;
        elfAnimator.SetLayerWeight(elfAnimator.GetLayerIndex("Override"), 1.0f);
        elfAnimator.SetTrigger("Stun");
        getUpTimer = stunDuration;
        _invulnerable = true;
        EnablePhysics(false);
    }

    void EnablePhysics(bool b)
    {
        rb.isKinematic = !b;
        var colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = b;
        }
    }

    void GetUp()
    {
        if (stunEffect) Destroy(stunEffect);
        elfAnimator.SetTrigger("GetUp");
    }

    public void RestartMove()
    {
        elfAnimator.SetLayerWeight(elfAnimator.GetLayerIndex("Override"), 0.0f);
        moveStopTimer = 0.0f;
        stun = 0;
        _invulnerable = false;
        EnablePhysics(true);
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

    private bool isCarrying
    {
        get
        {
            for (int i = 0; i < carryObjs.Count; i++)
            {
                if (carryObjs[i]) return true;
            }

            return false;
        }
    }

    public int GetSlot()
    {
        int maxSlots = (isKrampus) ? (krampusMaxCarry) : (elfMaxCarry);
        for (int i = 0; i < maxSlots; i++)
        {
            if (carryObjs[i] == null) return i;
        }

        return -1;
    }

    public bool Carry(Gift gift)
    {
        int slot = GetSlot();
        if (slot == -1) return false;
        if (moveStopTimer > 0.0f) return false;
        if (_invulnerable) return false;

        carryObjs[slot] = gift;
        carryTimer = carryCooldown;

        return true;
    }

    void ReleaseCarry()
    {
        for (int i = 0; i < carryObjs.Count; i++)
        {
            if (carryObjs[i] == null) continue;
            carryObjs[i].Release();
            carryObjs[i] = null;
        }

        carryTimer = carryCooldown;
    }
    public void DropGift(Bag bag)
    {
        bool dropOne = false;
        for (int i = 0; i < carryObjs.Count; i++)
        {
            if (carryObjs[i] == null) continue;
            Destroy(carryObjs[i].gameObject);

            if (Globals.ownerWinsDrop)
                bag.Player._score++;
            else
                _score++;

            dropOne = true;
        }

        if (dropOne)
        {
            Instantiate(dropEffectPrefab, bag.transform.position + Vector3.up * 0.1f, Quaternion.identity);
        }
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

    public void AddEssence(int value)
    {
        _essence = Mathf.Min(_essence + value, _maxEssence);

        FlashColor(essenceFlashColor, essenceFlashTime);

        elfAnimator.SetTrigger("Cheer");
    }

    [Button("Debug: Transform to Krampus")]
    void DebugTransformToKrampus()
    {
        _essence = 1000.0f;
        TransformToKrampus();
    }

    void TransformToKrampus()
    {
        ReleaseCarry();

        StartCoroutine(TransformToKrampusCR());
    }

    IEnumerator TransformToKrampusCR()
    {
        moveStopTimer = 0.4f;
        Instantiate(krampusEffectPrefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);
        yield return new WaitForSeconds(0.2f);
        elfRenderer.enabled = false;
        yield return new WaitForSeconds(0.1f);
        isKrampus = true;
        krampusRenderer.enabled = true;
    }

    void TransformToElf()
    {
        ReleaseCarry();

        StartCoroutine(TransformToElfCR());
    }

    IEnumerator TransformToElfCR()
    {
        isKrampus = false;
        moveStopTimer = 0.4f;
        Instantiate(krampusEffectPrefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        krampusRenderer.enabled = false;
        elfRenderer.enabled = true;
    }
}
