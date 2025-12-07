using UC;
using UnityEngine;

public class Bag : MonoBehaviour
{
    [SerializeField] 
    private int         playerId;
    [SerializeField] 
    private Renderer    mainRenderer;
    [Header("UI")]
    [SerializeField]
    private Hypertag    mainCanvasTag;
    [SerializeField]
    private BagUI       bagUIPrefab;
    [SerializeField]
    private Transform   bagPoint;
    [SerializeField]
    private Transform   _spawnPoint;

    Player      ownerPlayer;
    BagUI       bagUI;

    public Player       Player => ownerPlayer;
    public Transform    SpawnPoint => _spawnPoint;

    private void Start()
    {
        ownerPlayer = Player.FindPlayerById(playerId);
        if (ownerPlayer != null)
        {
            var customizer = ownerPlayer.GetComponent<ElfCustomizer>();
            if (customizer != null)
            {
                var color = customizer.GetHatColor();

                var material = new Material(mainRenderer.material);
                material.name = "CustomizedMaterial";

                material.SetColor("_Color_0", color);
                mainRenderer.material = material;
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        var canvas = mainCanvasTag.FindFirst<Canvas>();
        if (bagUIPrefab)
        {
            bagUI = Instantiate(bagUIPrefab, canvas.transform);
            bagUI.Bag = this;
            bagUI.trackedObject = bagPoint;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (Globals.canDropOnAnyBag)
        {
            player?.DropGift(this);
        }
        else
        {
            if (player == ownerPlayer)
            {
                player.DropGift(this);
            }
        }
    }

    public static Bag FindBagById(int playerId)
    {
        var bags = FindObjectsByType<Bag>(FindObjectsSortMode.None);
        foreach (var bag in bags)
        {
            if (bag.playerId == playerId) return bag;
        }

        return null;
    }
}
