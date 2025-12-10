using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UC.HealthSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private int             _playerId = -1;
    [SerializeField] 
    private float           moveSpeed = 5.0f;
    [SerializeField] 
    private float           krampusMoveMultiplier = 1.0f;
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
    [SerializeField]
    private SoundDef        hitSound;
    [SerializeField]
    private SoundDef        stunSound;
    [SerializeField]
    private SoundDef        deathSound;
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
    [SerializeField]
    private SoundDef        dropSound;
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
    [SerializeField]
    private SoundDef        transformToKrampusSound;
    [SerializeField]
    private SoundDef        transformToElfSound;
    [Header("Soul")]
    [SerializeField]
    private GameObject      soulEffectPrefab;
    [SerializeField]
    private Soul            soulPrefab;
    [SerializeField]
    private SoundDef        teleportSound;
    [Header("Input")]
    [SerializeField]
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl moveControl;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl aimShootControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton, Tooltip("This one only is applicable if we're using mouse + keyboard")]
    private UC.InputControl shootControlIfMouse;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl toggleKrampus;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl teabagControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl continueButton;
    [Header("UI")]
    [SerializeField]
    private Hypertag        mainCanvasTag;
    [SerializeField]
    private PlayerUI        playerUIPrefab;
    [SerializeField]
    private Transform       uiPoint;
    [SerializeField]
    private Camera          portraitCamera;
    [SerializeField]
    private Camera          portraitCameraKrampus;
    [SerializeField]
    private Camera          portraitCameraDeath;
    [SerializeField]
    private Hypertag        mainCameraTag;

    Vector2 moveVector;
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
    bool            _dead = false;
    GameObject      stunEffect;
    List<Gift>      carryObjs;
    float           carryTimer;
    int             _score;
    float           _essence;
    bool            isKrampus;
    Material        krampusMaterial;
    bool            attacking;
    Camera          mainCamera;

    public int playerId => _playerId;
    public int coalCount => (isKrampus) ? (0) : (_coalCount);
    public float coalGatherProgress => coalGatherTime / coalGatherDuration;
    public bool invulnerable => _invulnerable || isKrampus;
    public int  score => _score;
    public float essence => _essence;
    public int maxEssence => _maxEssence;
    public float essencePercentage => _essence / (float)_maxEssence;
    
    public bool canMove => (moveStopTimer <= 0.0f);
    public bool isDead => _dead;

    void Start()
    {
        var playerData = GameManager.GetPlayerData(playerId);
        if (playerData == null)
        {
            Destroy(gameObject);
            return;
        }

        if (playerId >= 0)
        {
            StartCoroutine(SetupInputCR());
        }

        rb = GetComponent<Rigidbody>();
        elfAnimator = GetComponent<Animator>();
        krampusAnimator = krampusRoot.GetComponent<Animator>();
        elfCustomizer = GetComponent<ElfCustomizer>();
        elfCustomizer.SetColors(playerData.hatColor, playerData.hairColor, playerData.clothesColor);

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

        if ((portraitCamera) || (portraitCameraKrampus) || (portraitCameraDeath))
        {
            RenderTexture renderTexture = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);
            portraitCamera.enabled = true;
            if (portraitCamera) portraitCamera.targetTexture = renderTexture;
            if (portraitCameraKrampus) portraitCameraKrampus.targetTexture = renderTexture;
            if (portraitCameraDeath) portraitCameraDeath.targetTexture = renderTexture;

            playerUI.portrait = renderTexture;
        }

        mainCamera = mainCameraTag.FindFirst<Camera>();
    }

    IEnumerator SetupInputCR()
    {
        yield return new WaitForSeconds(0.05f * playerId);

        MasterInputManager.SetupInput(playerId, playerInput);
        moveControl.playerInput = playerInput;
        aimShootControl.playerInput = playerInput;
        toggleKrampus.playerInput = playerInput;
        teabagControl.playerInput = playerInput;
        continueButton.playerInput = playerInput;
        shootControlIfMouse.playerInput = playerInput;

        if (aimShootControl.IsMouseLike())
        {
            Cursor.visible = true;
        }
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
            if (isKrampus) s *= krampusMoveMultiplier;
            var tmp = moveVector.x0y() * s;
            tmp.y = rb.linearVelocity.y;
            rb.linearVelocity = tmp;
        }
    }

    void Update()
    {
        portraitCamera.enabled = elfRenderer.enabled && !isDead;
        portraitCameraKrampus.enabled = krampusRenderer.enabled && !isDead;
        portraitCameraDeath.enabled = isDead;

        if (!playerInput.enabled) return;

        if (LevelManager.isDone)
        {
            if (continueButton.IsDown())
            {
                FullscreenFader.FadeOut(0.5f, Color.black, () =>
                {
                    SceneManager.LoadScene(0);
                });
            }

            return;
        }

        moveVector = moveControl.GetAxis2();
        aimVector = aimShootControl.GetAxis2();
        if (aimShootControl.IsMouseLike())
        {
            aimVector = Vector2.zero;
            if (shootControlIfMouse.IsDown())
            {
                Vector3 playerCenter = transform.position + Vector3.up * 0.5f;
                Vector3 mouseWorld = MouseToWorldXZ(mainCamera, 0.5f);

                Vector3 dir3D = mouseWorld - playerCenter;

                // Convert to XZ 2D vector (your aimVector)
                aimVector = new Vector2(dir3D.x, dir3D.z).normalized;
            }
        }

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
        if ((coalTimer <= 0.0f) && (_coalCount > 0) && (!isKrampus) && (canMove))
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
                    var playersInRange = GetPlayersInCone(2.0f, 45.0f);
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
            if (canMove)
            {
#if UNITY_EDITOR
                if ((toggleKrampus.IsDown())/* && (essence == _maxEssence)*/)
                {
                    DebugTransformToKrampus();
                    //TransformToKrampus();
                }
#else
                if ((toggleKrampus.IsDown()) && (essence == _maxEssence))
                {
                    TransformToKrampus();
                }
#endif
            }
        }

        if ((teabagControl.IsDown()) && (canMove))
        {
            var animator = (isKrampus) ? krampusAnimator : elfAnimator;
            animator.SetTrigger("Crouch");
            moveStopTimer = float.MaxValue;
        }

    }

    // For animation to reset the teabag
    public void GetBackUp(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        moveStopTimer = 0.0f;
    }

    List<Player> GetPlayersInCone(float radius, float maxAngle)
    {
        List<Player> ret = new();
        var colliders = Physics.OverlapSphere(transform.position, radius, 1 << gameObject.layer);
        foreach (var collider in colliders)
        {
            var otherPlayer = collider.GetComponent<Player>();
            if ((otherPlayer != this) && (!otherPlayer._invulnerable))
            {
                Vector3 toEnemy = (otherPlayer.transform.position - transform.position).x0z().normalized;
                float   angle = Vector3.Angle(transform.forward, toEnemy);
                if (angle < maxAngle)
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
        var players = GetPlayersInCone(1.25f, 45.0f);
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
            if ((moveVector.magnitude > 0.1f) && (canMove))
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

        if (aimShootControl.IsMouseLike())
        {
            // Reduce autoaim
            coalInstance.angleTolerance = 10.0f;
        }
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
            hitSound?.Play();
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

        if (isKrampus) TransformToElf();

        deathSound?.Play();
        FlashColor(hitFlashColor, hitFlashTime);
        moveStopTimer = float.MaxValue;
        elfAnimator.ChangeLayerWeight("Override", 1.0f, 0.1f);
        elfAnimator.SetTrigger("Stun");
        _invulnerable = true;
        _dead = true;

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
        teleportSound?.Play();
        yield return new WaitForSeconds(0.1f);
        elfRenderer.enabled = false;
        yield return new WaitForSeconds(0.5f);

        Bag bag = Bag.FindBagById(playerId);
        var spawnPos = bag.SpawnPoint;
        transform.position = spawnPos.position;

        teleportSound?.Play();
        Instantiate(soulEffectPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.1f);
        elfRenderer.enabled = true;
        _dead = false;
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

        var soul = Instantiate(soulPrefab, transform.position, transform.rotation);
        soul.playerId = playerId;
    }

    void Stun()
    {
        stunSound?.Play();
        moveStopTimer = float.MaxValue;
        elfAnimator.ChangeLayerWeight("Override", 1.0f, 0.1f);
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
        elfAnimator.ChangeLayerWeight("Override", 0.0f, 0.1f);
        moveStopTimer = 0.0f;
        stun = 0;
        _invulnerable = false;
        EnablePhysics(true);
    }

    public bool AddGatherCoal(float deltaTime)
    {
        if (_coalCount < maxCoal)
        {
            coalGatherTime += deltaTime;
            if (coalGatherTime > coalGatherDuration)
            {
                _coalCount++;
                ResetGatherCoal();
            }
            return true;
        }
        else
        {
            coalGatherTime = 0.0f;
            return false;
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
            dropSound?.Play();
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
        transformToKrampusSound?.Play();
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
        transformToElfSound?.Play();
        isKrampus = false;
        moveStopTimer = 0.4f;
        Instantiate(krampusEffectPrefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        krampusRenderer.enabled = false;
        elfRenderer.enabled = true;
    }

    enum EmoteType { Dance0 = 0, Dance1 = 1, Dance = 2,
                 Cry0 = 3, Cry1 = 4 };

    public void Celebrate()
    {
        int r = UnityEngine.Random.Range(0, 3);
        Emote((EmoteType)(r + EmoteType.Dance0));

        StartCoroutine(CelebrateCR());
    }

    IEnumerator CelebrateCR()
    {
        while (true)
        {
            Instantiate(soulEffectPrefab, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(UnityEngine.Random.Range(4.0f, 8.0f));
        }
    }

    public void Lose()
    {
        int r = UnityEngine.Random.Range(0, 2);
        Emote((EmoteType)(r + EmoteType.Cry0));
    }

    void Emote(EmoteType type)
    {
        moveStopTimer = float.MaxValue;
        EnablePhysics(false);

        var animator = (isKrampus) ? (krampusAnimator) : (elfAnimator);

        animator.SetTrigger("Emote");
        animator.SetInteger("EmoteType", (int)type);
        animator.ChangeLayerWeight("Override", 1.0f, 0.1f);
    }

    public static Vector3 MouseToWorldXZ(Camera cam, float yLevel)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, yLevel, 0f));

        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return Vector3.zero; // fallback
    }
}
