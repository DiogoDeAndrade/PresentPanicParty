using System.Collections;
using TMPro;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSelectorUI : MonoBehaviour
{
    [SerializeField] 
    private int playerId;
    [SerializeField]
    private CanvasGroup playerJoinComponent;
    [SerializeField]
    private CanvasGroup customizationComponents;
    [SerializeField]
    private UIDiscreteColorSelector hatColorUI;
    [SerializeField]
    private UIDiscreteColorSelector hairColorUI;
    [SerializeField]
    private UIDiscreteColorSelector clothesColorUI;
    [SerializeField]
    private UIButton    readyButton;

    PlayerSelector  _player;    
    ElfCustomizer   customizer;

    void Start()
    {
        _player = PlayerSelector.FindPlayerById(playerId);

        playerJoinComponent.alpha = 1.0f;
        customizationComponents.alpha = 0.0f;

        var tracker = GetComponent<UITrackObject>();
        var t = _player.elfCustomizer.transform;
        var n = t.GetComponentInChildren<TrackPivot>();
        if (n) t = n.transform;
        tracker.trackedObject = t;

        StartCoroutine(SetupInputCR());

        customizer = _player.GetComponentInChildren<ElfCustomizer>();

        readyButton.onInteract += ReadyButton_onInteract;
    }

    IEnumerator SetupInputCR()
    {
        for (int i = 0; i < 3; i++)
            yield return null;

        var uiGroups = GetComponentsInChildren<UIGroup>();
        foreach (var uiGroup in uiGroups)
        {
            uiGroup.SetPlayerInput(_player.GetComponentInChildren<PlayerInput>());
        }
    }

    private void ReadyButton_onInteract(BaseUIControl control)
    {
        var playerData = new GameManager.PlayerData()
        {
            hatColor = hatColorUI.value,
            hairColor = hairColorUI.value,
            clothesColor = clothesColorUI.value
        };

        _player.Lock(playerData);
    }

    void Update()
    {
        customizer.SetColors(hatColorUI.value, hairColorUI.value, clothesColorUI.value);

        if (_player.isActive)
        {
            playerJoinComponent.FadeOut(0.2f);

            if (_player.isLocked)
            {
                customizationComponents.FadeOut(0.2f);
            }
            else
            {
                customizationComponents.FadeIn(0.2f);
            }
        }
        else
        {
            playerJoinComponent.FadeIn(0.2f);
        }
    }
}
