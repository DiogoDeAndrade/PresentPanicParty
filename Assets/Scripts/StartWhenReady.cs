using UC;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class StartWhenReady : MonoBehaviour
{
    [SerializeField, Scene] private string gameScene;

    PlayerSelector[] playerSelectors;

    void Start()
    {
        playerSelectors = FindObjectsByType<PlayerSelector>(FindObjectsSortMode.None);
    }

    void Update()
    {
        int playersCount = 0;
        int playersReady = 0;
        for (int i = 0; i < 4; i++)
        {
            if (playerSelectors[i].isActive)
            {
                playersCount++;
                if (playerSelectors[i].isLocked)
                {
                    playersReady++;
                }
            }
        }

        if ((playersReady >= 2) && (playersReady == playersCount))
        {
            // Let's goooooooooooooooo
            FullscreenFader.FadeOut(0.5f, Color.black, () =>
            {
                SceneManager.LoadScene(gameScene);
            });

            enabled = false;
        }
    }
}
