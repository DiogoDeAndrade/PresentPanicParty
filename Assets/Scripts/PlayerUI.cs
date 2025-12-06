using UC;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : UITrackObject
{
    [Header("PlayerUI Elements")]
    [SerializeField]
    private Image[] coalDisplay;
    [SerializeField]
    private Image   coalGatherBar;
    [SerializeField]
    private CanvasGroup coalGatherDisplay;

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

        var coalProgress = _player.coalGatherProgress;
        if (coalProgress > 0.0f)
        {
            coalGatherDisplay.FadeIn(0.2f);
            coalGatherBar.fillAmount = coalProgress;
        }
        else
        {
            coalGatherDisplay.FadeOut(0.2f);
        }
    }
}
