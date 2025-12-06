using UnityEngine;

public class RedirectAnimEvents : MonoBehaviour
{
    void FinishAttack()
    {
        var player = GetComponentInParent<Player>();
        player?.FinishAttack();
    }
}
