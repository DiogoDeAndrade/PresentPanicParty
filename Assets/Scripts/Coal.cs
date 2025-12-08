using NaughtyAttributes;
using UC;
using UnityEngine;

public class Coal : MonoBehaviour
{
    [SerializeField] 
    private float speed = 5.0f;
    [SerializeField] 
    private bool  autoAim = true;
    [SerializeField, ShowIf(nameof(autoAim))]
    private float _angleTolerance = 45.0f;
    [SerializeField, ShowIf(nameof(autoAim)), Range(0.0f, 1.0f)]
    private float prediction = 0.0f;
    [SerializeField] 
    private float duration = 3.0f;
    [SerializeField] 
    private float timeToGravity = 1.0f;

    Rigidbody rb;
    bool      hostile = true;

    public int Owner {  get; set; }
    public float angleTolerance
    {
        set { _angleTolerance = value; }
    }

    void Start()
    {
        if (autoAim)
        {
            var     players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            float   minAngle = float.MaxValue;
            foreach (var p in players)
            {
                if (p.playerId == Owner) continue;

                Vector3 toEnemy = (p.transform.position - transform.position).x0z().normalized;
                float   angle = Vector3.Angle(toEnemy, transform.forward.x0z());
                if ((angle < minAngle) && (angle < _angleTolerance))
                {
                    // Prediction
                    var otherRB = p.GetComponent<Rigidbody>();
                    if (otherRB)
                    {
                        float distance =   Vector3.Distance(transform.position, p.transform.position) ;
                        Vector3 predPoint = p.transform.position + prediction * (otherRB.linearVelocity * (distance / speed));
                        toEnemy = (predPoint - transform.position).x0z().normalized;
                    }

                    transform.rotation = Quaternion.LookRotation(toEnemy, Vector3.up);
                    minAngle = angle;
                }
            }
        }

        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;

        Debug.DrawLine(transform.position, transform.position + transform.forward * 100.0f, Color.blue, 5.0f);
    }

    void Update()
    {
        if (timeToGravity > 0.0f)
        {
            timeToGravity -= Time.deltaTime;
            if (timeToGravity <= 0.0f)
            {
                rb.useGravity = true;
            }
        }
        if (duration > 0.0f)
        {
            duration -= Time.deltaTime;
            if (duration <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!hostile) return;

        // Check if it hit a player
        var player = collider.GetComponent<Player>();
        if (player != null)
        {
            if ((player.playerId != Owner) && (!player.invulnerable))
            {
                player.HitCoal();
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if it hit a player
        var player = collision.collider.GetComponent<Player>();
        if (player != null)
        {
            if (player.playerId == Owner) return;
        }
        // Stops being hostile when it hits something physical
        if (hostile)
        {
            rb.linearVelocity = rb.linearVelocity * 0.5f;
            hostile = false;
        }
    }
}
