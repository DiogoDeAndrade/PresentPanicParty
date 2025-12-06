using TMPro;
using UC;
using UnityEngine;
using UnityEngine.UI;

public class BagUI : UITrackObject
{
    [Header("BagUI Elements")]
    [SerializeField]
    private TextMeshProUGUI text;

    Bag _bag;

    public Bag Bag
    {
        set { _bag = value; }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        text.text = $"{_bag.Player.score}";
    }
}
