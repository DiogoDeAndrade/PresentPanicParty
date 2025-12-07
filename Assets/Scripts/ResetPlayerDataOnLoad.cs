using UnityEngine;

public class ResetPlayerDataOnLoad : MonoBehaviour
{
    void Start()
    {
        GameManager.ResetPlayerData();
    }
}
