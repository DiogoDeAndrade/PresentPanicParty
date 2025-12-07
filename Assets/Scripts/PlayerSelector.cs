using System.Collections;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class PlayerSelector : MonoBehaviour
{
    [SerializeField] 
    private int             playerId;
    [Header("Elements")]
    [SerializeField] 
    private ElfCustomizer   _elfCustomizer;
    [Header("Effects")]
    [SerializeField]
    private GameObject      joinEffectPrefab;
    [Header("Input")]
    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl moveControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl continueButton;

    Animator    animator;
    Renderer[]  renderers;
    bool        activePlayer = false;
    bool        locked = false;

    public bool             isActive => activePlayer;
    public bool             isLocked => locked;
    public ElfCustomizer    elfCustomizer => _elfCustomizer;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (playerId >= 0)
        {
            StartCoroutine(SetupInputCR());
        }

        EnableRenderers(false);
    }

    IEnumerator SetupInputCR()
    {
        yield return new WaitForSeconds(0.05f * playerId);

        MasterInputManager.SetupInput(playerId, playerInput);
        moveControl.playerInput = playerInput;
        continueButton.playerInput = playerInput;
    }


    void Update()
    {
        if (!playerInput.enabled) return;

        if (activePlayer)
        {
            if (locked)
            {

            }
            else
            {

            }
        }
        else
        {
            if (continueButton.IsDown())
            {
                StartCoroutine(PlayerJoinCR());
            }
        }
    }

    IEnumerator PlayerJoinCR()
    {
        activePlayer = true;
        Instantiate(joinEffectPrefab, elfCustomizer.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.1f);
        EnableRenderers(true);
    }

    void EnableRenderers(bool b)
    {
        if ((renderers == null) || (renderers.Length == 0))
        {
            renderers = elfCustomizer.GetComponentsInChildren<Renderer>();
        }

        foreach (var renderer in renderers)
        {
            renderer.enabled = b;
        }
    }

    public void Lock(PlayerData playerData)
    {
        locked = true;
        Celebrate();

        GameManager.SetPlayerData(playerId, playerData);
    }

    public void Celebrate()
    {
        int r = Random.Range(0, 3);

        animator.SetTrigger("Emote");
        animator.SetInteger("EmoteType", r);
        animator.SetLayerWeight(animator.GetLayerIndex("Override"), 1.0f);
    }

    public static PlayerSelector FindPlayerById(int playerId)
    {
        var players = FindObjectsByType<PlayerSelector>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.playerId == playerId) return player;
        }

        return null;
    }
}
