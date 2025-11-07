using System.Collections;
using UnityEngine;

public class DebrisController : MonoBehaviour
{
    [Header("Capture Settings")]
    public bool isCaptured = false;
    [HideInInspector] public Transform orbitTarget;
    public float orbitSpeed = 5f;
    public float orbitDistance = 2f;
    public float pullStrength = 5f;

    [Header("Private Settings")]
    private Rigidbody2D rbDebris;
    private float currentOrbitAngle;

    [Header("Pull Settings")]
    [HideInInspector] public bool isPulled = false;
    [HideInInspector] public float pullRadius;
    public float pullDuration = 0.2f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rbDebris = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (isPulled && !isCaptured)
        {
            if (orbitTarget != null)
                Debug.DrawLine(transform.position, orbitTarget.position, Color.cyan);

            //Start pull toward player
            Vector2 direction = (orbitTarget.position - transform.position);
            float distanceToPlayer = direction.magnitude;

            if (distanceToPlayer > pullRadius * 0.8f)
                rbDebris.linearVelocity *= 0.95f; // slow down near edge

            if (distanceToPlayer > 0.1f)
            {
                //Stronger pull when closer, weaker when far 
                float pullForce = pullStrength / Mathf.Max(0.5f, distanceToPlayer);
                rbDebris.AddForce(direction.normalized * pullForce, ForceMode2D.Force);
            }
        }
        else
        {
            //Once captured, orbit around player
            currentOrbitAngle += orbitSpeed * Time.fixedDeltaTime;
            Vector2 orbitOffset = new Vector2(Mathf.Cos(currentOrbitAngle), Mathf.Sin(currentOrbitAngle));

            Vector2 targetPos = (Vector2)orbitTarget.position + orbitOffset * orbitDistance;
            transform.position = Vector2.Lerp(transform.position, targetPos, Time.fixedDeltaTime * orbitSpeed);
        }
    }

    public void StartPull(Transform target, float radius)
    {
        orbitTarget = target;
        pullRadius = radius;
        isPulled = true;
    }

    public void StopPull()
    {
        isPulled = false;
        orbitTarget = null;
    }

    public void Capture(Transform target)
    {
        orbitTarget = target;
        isCaptured = true;
        rbDebris.linearVelocity = Vector2.zero;

        // Randomize their starting orbit angle (based on current position)
        Vector2 dirFromTarget = (transform.position - orbitTarget.position).normalized;
        currentOrbitAngle = Mathf.Atan2(dirFromTarget.y, dirFromTarget.x);

        StartCoroutine(MoveIntoOrbit());
    }

    private IEnumerator MoveIntoOrbit()
    {
        float elapsed = 0f;
        float duration = pullDuration;
        Vector3 startPos = transform.position;
        Vector3 targetPos = orbitTarget.position + (transform.position - orbitTarget.position).normalized * orbitDistance;

        while (elapsed < duration)
        {
           transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
}
