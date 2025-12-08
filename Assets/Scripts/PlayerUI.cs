using UC;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("PlayerUI Elements")]
    [SerializeField]
    private Image[] coalDisplay;
    [SerializeField]
    private Image coalGatherBar;
    [SerializeField]
    private CanvasGroup coalGatherDisplay;
    [SerializeField]
    private Image   essenceBar;
    [SerializeField]
    private CanvasGroup essenceDisplay;
    [SerializeField]
    private TextMeshProUGUI giftText;
    [SerializeField]
    private RawImage        portraitImage;

    Player _player;
    
    public Transform trackedObject
    {
        set
        {
            var trackers = GetComponentsInChildren<UITrackObject>();
            foreach (var tracker in trackers) tracker.trackedObject = value;
        }
    }

    public Player Player
    {
        set { _player = value; }
    }

    public Texture portrait
    {
        set
        {
            portraitImage.texture = value;
        }
    }

    protected void Start()
    {
        //coalGatherDisplay.alpha = 0.0f;
        //essenceDisplay.alpha = 0.0f;

        var rectTransform = transform as RectTransform;

        switch (_player.playerId)
        {
            case 0:
                rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
                rectTransform.pivot = new Vector2(0.0f, 1.0f);
                rectTransform.anchoredPosition = new Vector2(105.0f, -5.0f);
                break;
            case 1:
                rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
                rectTransform.anchorMax = new Vector2(1.0f, 0.0f);
                rectTransform.pivot = new Vector2(1.0f, 0.0f);
                rectTransform.anchoredPosition = new Vector2(75.0f, 15.0f);
                break;
            case 2:
                rectTransform.anchorMin = new Vector2(1.0f, 1.0f);
                rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                rectTransform.pivot = new Vector2(1.0f, 1.0f);
                rectTransform.anchoredPosition = new Vector2(75.0f, -5.0f);
                break;
            case 3:
                rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                rectTransform.anchorMax = new Vector2(0.0f, 0.0f);
                rectTransform.pivot = new Vector2(0.0f, 0.0f);
                rectTransform.anchoredPosition = new Vector2(105.0f, 15.0f);
                break;
            default:
                break;
        }
    }

    protected void Update()
    {
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

        essenceBar.fillAmount = _player.essencePercentage;

        giftText.text = $"x{_player.score}";
    }
}
