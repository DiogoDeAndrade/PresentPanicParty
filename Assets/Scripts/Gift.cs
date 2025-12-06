using System.Collections.Specialized;
using UnityEngine;

public class Gift : MonoBehaviour
{
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private Vector3    pickupEffectOffset;

    Rigidbody rb;
    Collider  mainCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Can't be picked up if it's already picked up
        if (rb.isKinematic) return;

        var player = collision.collider.GetComponent<Player>();
        if (player)
        {
            if (player.Carry(this))
            {
                // Raycast
                if (Physics.Raycast(transform.position + Vector3.up * 1.0f, Vector3.down, out var hitInfo, float.MaxValue, UC.GlobalsBase.groundMask))
                {
                    Instantiate(pickupEffectPrefab, hitInfo.point + pickupEffectOffset, transform.rotation);
                }
                Hold();
                return;
            }            
        }
    }

    public void Hold()
    {
        rb.isKinematic = true;
        mainCollider.enabled = false;
    }

    public void Release()
    {
        rb.isKinematic = false;
        mainCollider.enabled = true;

        var randomVector = Random.insideUnitSphere;
        randomVector.y = Mathf.Abs(randomVector.y * 1.25f);
        randomVector.Normalize();

        rb.linearVelocity = randomVector * 4.0f;
    }
}
