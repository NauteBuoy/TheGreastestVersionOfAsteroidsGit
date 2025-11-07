using System.Collections;
using UnityEngine;

public class DebrisController : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform orbitTarget; //assigned while in gravityRadius
    public bool isInOrbit = false;
    public float orbitSpeed = 60f; //degrees per second
    //public float orbitRadiusMultiplier = 1f; //how far from orbit pivot
    public float orbitSnapDuration = 0.15f; //time to snap into orbit
    public float orbitFollowSpeed = 5f; //how fast debris follows gravity target

    [Header("Capture Progress")]
    public float captureProgress = 0f;
    public float captureTime = 3f; //seconds to lock-on to gravity target

    [Header("Private Settings")]
    [HideInInspector] public float orbitAngleOffset;
    [HideInInspector] public float currentOrbitRadius;
    [HideInInspector] public float rotationAccumulator = 0f; // increments each FixedUpdate to rotate

    private Rigidbody2D rbDebris;
    private Vector2 initialDebrisVelocity;

    // optional legacy pull vars (unused by current flow, keep for future)
    [Header("LEGACY Pull Settings")]
    public float pullStrength = 10f;
    //[HideInInspector] public bool isPulled = false;
    //[HideInInspector] public float pullRadius;
    //[HideInInspector] public float pullDuration;
    
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
        //if captured, orbit around the gravity target
        if (isInOrbit && orbitTarget != null)
        {
            //get player speed
            float playerSpeed = orbitTarget.GetComponent<Rigidbody2D>().linearVelocity.magnitude;

            //slow orbit when player moves
            float orbitMultiplier = Mathf.Clamp(1f - playerSpeed * 0.05f, 0.4f, 1f);

            // advance rotation accumulator (radians)
            rotationAccumulator += orbitSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime * orbitMultiplier;
            // final angle = base phase offset + rotation accumulator
            float angle = orbitAngleOffset + rotationAccumulator;

            //orbit offset around pivot
            Vector2 orbitOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * currentOrbitRadius;
            Vector2 targetPos = (Vector2)orbitTarget.position + orbitOffset;

            //lerp and follow the orbit position
            transform.position = Vector2.Lerp(transform.position, targetPos, Time.fixedDeltaTime * orbitFollowSpeed);
        }
    }

    public void UpdateCapture(Transform orbitPivot, float gravityEngineRadius)
    {
        //this function handles everything: progress, decay, and restoring velocity
        if (isInOrbit)  
            return;

        //assign pivot
        orbitTarget = orbitPivot;
        float distance = Vector2.Distance(transform.position, orbitTarget.position);

        //if inside gravityEngineRadius, increase capture progress
        if (distance < gravityEngineRadius)
        {
            if (captureProgress == 0f) //store initial velocity first frame inside radius
                initialDebrisVelocity = rbDebris.linearVelocity;

            //increase capture
            captureProgress += Time.deltaTime;

            //slow down slightly for capturing
            float slowdownFactor = Mathf.Lerp(1f, 0.98f, captureProgress / captureTime);
            rbDebris.linearVelocity *= slowdownFactor;

            //apply gravity pull towards the center
            Vector2 directionToCenter = ((Vector2)orbitTarget.position - (Vector2)transform.position).normalized;
            rbDebris.AddForce(directionToCenter * pullStrength, ForceMode2D.Force);
        }
        else
        {
            //decay if outside of gravityEngineRadius
            captureProgress = Mathf.Max(0f, captureProgress - Time.deltaTime * 0.5f);

            //restore initial velocity with a slight multiplier for overshoot
            Vector2 targetVelocity = initialDebrisVelocity * 1.05f; // 5% faster than initial speed
            rbDebris.linearVelocity = Vector2.Lerp(rbDebris.linearVelocity, targetVelocity, Time.deltaTime * orbitFollowSpeed);
        }

        if (captureProgress >= captureTime)
        {
            Capture(orbitTarget);
        }
    }

    public void Capture(Transform orbitPivot)
    {
        if (isInOrbit)  
            return;

        //assign pivot
        orbitTarget = orbitPivot;
        isInOrbit = true;

        //stop physics
        rbDebris.bodyType = RigidbodyType2D.Kinematic;
        rbDebris.linearVelocity = Vector2.zero;

        //switch collider to trigger to not block player or each other
        Collider2D colDebris = GetComponent<Collider2D>();
        if (colDebris != null)  
            colDebris.isTrigger = true;

        //calculate starting orbit angle based on current position
        Vector2 dirFromTarget = (transform.position - orbitTarget.position).normalized;
        rotationAccumulator = Mathf.Atan2(dirFromTarget.y, dirFromTarget.x);

        // randomize follow speed slightly for natural effect
        float randomFactor = Random.Range(0.95f, 1.05f);
        orbitFollowSpeed = orbitFollowSpeed * randomFactor;

        //snap into orbit smoothly (coroutine)
        StartCoroutine(MoveIntoOrbitRoutine());
    }

    private IEnumerator MoveIntoOrbitRoutine()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, orbitSnapDuration);

        Vector3 startPos = transform.position;

        // Calculate direction safely
        Vector3 dirFromTarget = (transform.position - orbitTarget.position);
        if (dirFromTarget.sqrMagnitude < 0.01f) // too close, set a default
            dirFromTarget = Vector3.right;

        Vector3 targetPos = orbitTarget.position + dirFromTarget.normalized * GravityEngineController.Instance.orbitRadius;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    public void SetOrbitRadius(float radius)
    {
        currentOrbitRadius = radius;
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    // Optional hook: if you want the debris to damage stuff while orbiting, implement this:
    //    // Example: notify player or enemies - handle damage logic elsewhere
    //    // if (other.CompareTag("Enemy")) { /* apply damage */ }
    //}

    //public void Release(Vector2 impulse)
    //{
    //    //isInOrbit = false;
    //    //captureProgress = 0f;

    //    ////restore physics & collider
    //    //rbDebris.bodyType = RigidbodyType2D.Dynamic;
    //    //if (colDebris != null) colDebris.isTrigger = false;

    //    ////give it a push away
    //    //rbDebris.linearVelocity = impulse;
    //}

    private void OnDrawGizmos()
    {
        if (orbitTarget != null)
        {
            float distance = Vector2.Distance(transform.position, orbitTarget.position);
            Gizmos.color = distance < 3f ? Color.cyan : Color.red;
            Gizmos.DrawLine(transform.position, orbitTarget.position);
        }
    }
}
