using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : UITrackObject
{
    [Header("PlayerUI Elements")]
    [SerializeField]
    private Image[] coalDisplay;

    Player _player;

    public Player Player
    {
        set { _player = value; }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        var coalCount = _player.coalCount;

        for (int i = 0; i < Mathf.Min(coalCount, coalDisplay.Length); i++)
        {
            coalDisplay[i].gameObject.SetActive(true);
        }
        for (int i = Mathf.Min(coalCount, coalDisplay.Length); i < coalDisplay.Length; i++)
        {
            coalDisplay[i].gameObject.SetActive(false);
        }
    }
}
