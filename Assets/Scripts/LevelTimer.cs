using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    void Update()
    {
        text.text = string.Format("{0:00}", Mathf.FloorToInt(LevelManager.timeRemaining));
    }
}
