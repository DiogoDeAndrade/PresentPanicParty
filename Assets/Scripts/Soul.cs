using UC;
using UnityEngine;

public class Soul : MonoBehaviour
{
    [SerializeField] private int    playerId;
    [SerializeField] private float  duration;

    Player player;
    Bag     bag;
    Essence essence;

    DualQuaternion src;
    DualQuaternion dest;

    float elapsedTime;

    void Start()
    {
        player = Player.FindPlayerById(playerId);
        bag = Bag.FindBagById(playerId);
        essence = GetComponent<Essence>();

        src = new DualQuaternion(transform.position, transform.rotation);
        dest = new DualQuaternion(bag.transform.position + Vector3.up * transform.position.y, bag.transform.rotation);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);
        var ret = DualQuaternion.ScrewInterpolate(src, dest, t);

        ret.GetRotationTranslation(out var rotation, out var translation);

        transform.position = translation;
        transform.rotation = rotation;

        if (t >= 1.0f)
        {
            enabled = false;
            essence.FadeOut();
            player.Ressurrect();
        }
    }
}
