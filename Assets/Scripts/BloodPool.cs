using System.ComponentModel;
using UC;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BloodPool : MonoBehaviour
{
    [SerializeField] private float      duration = 4.0f;
    [SerializeField] private Vector2    rotationRange = new Vector2(-180.0f, 180.0f);
    [SerializeField] private Vector2    startSize = Vector2.zero;
    [SerializeField] private Vector2    endSize = Vector2.one;
    [SerializeField] private float      sizeAnimationDuration = 0.5f;

    DecalProjector decalProjector;

    void Start()
    {
        decalProjector = GetComponent<DecalProjector>();
        gameObject.Tween().Interpolate(startSize, endSize, sizeAnimationDuration,
                                       (value) =>
                                       {
                                           decalProjector.size = new Vector3(value.x, value.y, 1.0f);
                                       }, "BloodSizeAnim").EaseFunction(Ease.SmoothStep);

        var angles = new Vector3(90.0f, 0.0f, rotationRange.Random());
        transform.rotation = Quaternion.Euler(angles);
    }

    void Update()
    {
        if (duration > 0.0f)
        {
            duration -= Time.deltaTime;
            if (duration <= 0.0f)
            {
                gameObject.Tween().Interpolate(1.0f, 0.0f, 0.25f, (value => decalProjector.fadeFactor = value), "FadeOutDecal").Done(() => Destroy(gameObject));
            }
        }
    }
}
