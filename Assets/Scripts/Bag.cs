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

    Player      ownerPlayer;
    BagUI       bagUI;

    public Player Player => ownerPlayer;

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
        bagUI = Instantiate(bagUIPrefab, canvas.transform);
        bagUI.Bag = this;
        bagUI.trackedObject = bagPoint;
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
            if ((player == ownerPlayer) && (player.isCarrying))
            {
                player.DropGift(this);
            }
        }
    }
}
