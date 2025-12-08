using UC;
using UnityEngine;
using UnityEngine.Audio;

public class Soul : MonoBehaviour
{
    [SerializeField] private int    _playerId;
    [SerializeField] private float  duration;

    Player      player;
    Bag         bag;
    Essence     essence;
    AudioSource audioSource;

    DualQuaternion src;
    DualQuaternion dest;

    float elapsedTime;

    public int playerId
    {
        set { _playerId = value; }
    }

    void Start()
    {
        player = Player.FindPlayerById(_playerId);
        bag = Bag.FindBagById(_playerId);
        essence = GetComponent<Essence>();
        audioSource = GetComponentInChildren<AudioSource>();
        var v = audioSource.volume;
        audioSource.volume = 0.0f;
        audioSource.FadeTo(v, 0.1f);

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
            audioSource.FadeTo(0.0f, 0.1f);
            enabled = false;
            essence.FadeOut();
            player.Ressurrect();
        }
    }
}
