using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisController : MonoBehaviour
{
    [Header("State Settings")]
    public bool isInGravityRange = false; //is debris within gravity engine range
    public bool isInOrbit = false; //is debris currently orbiting
    public bool isTrailing = false; //is debris currently trailing


    [Header("Orbit Settings")]
    public float orbitRotationSpeed = 180f; //speed of orbit in degrees per second
    public float orbitFollowSpeed = 4f; //speed at which debris follows orbit position
    public float stateTransitionTime = 1f; //time to transition between orbit and trail states


    [Header("Trail Settings")]
    public float trailFollowSpeed = 4f; //speed at which debris follows trail position
    public float trailSpread = 1f; //spread of debris when trailing
    public float trailLag = 0.2f; // delay between each debris' response


    [Header("Capture Settings")]
    public float captureProgress = 0f; //progress towards being captured into orbit
    public float captureTime = 2f; //time required to be captured into orbit
    public float captureGravityFactor = 0.95f; //factor to reduce velocity while being captured
    public float releaseVelocityFactor = 1.05f; //factor to increase velocity when released from gravity engine


    [Header("Private Settings")]
    private Transform orbitCentre; //center of gravity engine
    private Rigidbody2D debrisRB; //rigidbody of debris
    private Vector2 initialVelocity; //initial velocity of debris before capture
    private float orbitRadius; //radius of orbit
    private float currentAngle; //current angle of debris in orbit
    private int debrisIndex; //index of debris in captured debris list
    private float timeSinceTrail; //time since debris entered trail state


    void Start()
    {
        //get debris rigidbody
        debrisRB = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        //update time since trailing
        if (!orbitCentre)
            return;

        //if (isInOrbit && !isTrailing)
        if (isInOrbit && !isTrailing)
        {
            //orbiting
            Orbit();
        }
        else if (isTrailing && !isInOrbit)
        {
            //trailing
            Trail();
        }
    }

    public void UpdateCapture(Transform gravityEngineTransfrom, float gravityEngineRadius, float gravityEngineStrength, float gravityEngineOrbitRadius, float playerSpeed, float playerSpeedThreshold, float reentryDelay, int capturedDebrisIndex)
    {
        //set orbit parameters
        orbitCentre = gravityEngineTransfrom;
        orbitRadius = gravityEngineOrbitRadius;
        debrisIndex = capturedDebrisIndex;

        //calculate distance from gravity engine center
        float distanceFromGravityCentre = Vector2.Distance(transform.position, orbitCentre.position);
        isInGravityRange = distanceFromGravityCentre < gravityEngineRadius;

        //manage orbit and trail state transitions
        UpdateOrbitTrailState(playerSpeed, playerSpeedThreshold, reentryDelay);

        //don't process capture while orbiting
        if (isInOrbit)
            return;
        //don't process capture while trailing
        if (isTrailing)
            return;

        //check if within gravity engine range
        if (isInGravityRange)
        {
            //store initial velocity on first frame of capture
            if (captureProgress == 0f)
            {
                initialVelocity = debrisRB.linearVelocity;
            }

            //increase capture progress over time
            captureProgress += Time.deltaTime;
            captureProgress = Mathf.Clamp(captureProgress, 0f, captureTime);

            //apply gravitational pull towards gravity engine center
            float gravityOverTime = Mathf.Lerp(1f, captureGravityFactor, captureProgress / captureTime);
            Vector2 directionToGravityCenter = (orbitCentre.position - transform.position).normalized;
            debrisRB.AddForce(directionToGravityCenter * gravityEngineStrength, ForceMode2D.Force);
            debrisRB.linearVelocity *= gravityOverTime;

            //check if capture is complete
            if (captureProgress >= captureTime)
            {
                //capture debris into orbit
                StartCoroutine(CaptureIntoOrbitRoutine());
            }
        }
        //not in gravity engine range
        else
        {
            //decrease capture progress over time
            captureProgress = Mathf.Max(0f, captureProgress - Time.deltaTime * 0.5f);

            //restore initial velocity
            Vector2 releaseVelocity = initialVelocity * releaseVelocityFactor;
            debrisRB.linearVelocity = Vector2.Lerp(debrisRB.linearVelocity, releaseVelocity, Time.deltaTime * 2f);
        }
    }

    private IEnumerator CaptureIntoOrbitRoutine()
    {
        //set state to orbiting
        isInOrbit = true;
        isTrailing = false;

        //set debris to kinematic and disable collider trigger
        debrisRB.bodyType = RigidbodyType2D.Kinematic;
        debrisRB.linearVelocity = Vector2.zero;
        //GetComponent<Collider2D>().isTrigger = true;

        //calculate starting angle based on current position
        Vector2 directionToGravityCentre = (transform.position - orbitCentre.position).normalized;
        currentAngle = Mathf.Atan2(directionToGravityCentre.y, directionToGravityCentre.x);

        //move debris to exact orbit position over capture time
        Vector2 startCapturedPos = transform.position;
        Vector2 endOrbitPos = (Vector2)orbitCentre.position + directionToGravityCentre * orbitRadius;

        float timeElapsed = 0f;
        //float captureDuration = Mathf.Max(0.01f, captureTime);
        while (timeElapsed < stateTransitionTime)
        {
            //lerp to orbit position
            timeElapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startCapturedPos, endOrbitPos, timeElapsed / stateTransitionTime);
            yield return null;
        }

        //ensure exact orbit position
        transform.position = endOrbitPos;
    }

    void UpdateOrbitTrailState(float playerSpeed, float playerSpeedThreshold, float reentryDelay)
    {
        //update time since trailing
        timeSinceTrail += Time.deltaTime;

        if (isInOrbit && playerSpeed > playerSpeedThreshold)
        {
            //switch to trail state
            StartCoroutine(SwitchToTrail());
        }
        else if (isTrailing && playerSpeed <= playerSpeedThreshold)// && timeSinceTrail >= reentryDelay)
        {
            //switch to orbit state
            StartCoroutine(SwitchToOrbit());
        }
        else
        {
            timeSinceTrail = 0f;
        }
    }

    private IEnumerator SwitchToTrail()
    {
        //check if already trailing
        if (isTrailing)
            yield break;

        //set state to trailing
        isInOrbit = false;
        isTrailing = true;

        //move debris to exact trailing position over transition time
        Vector2 startOrbitPos = transform.position;
        Vector2 trailDirection = Vector2.zero;

        if (orbitCentre.TryGetComponent(out Rigidbody2D orbitRb))
        {
            Vector2 velocity = orbitRb.linearVelocity;
            if (velocity.magnitude > 0.1f)
            {
                trailDirection = velocity.normalized;
            }
            else
            {
                trailDirection = -orbitCentre.up;
            }
        }

        Vector2 spreadOffset = Vector2.Perpendicular(trailDirection).normalized * (debrisIndex * trailSpread);
        Vector2 endTrailPos = (Vector2)orbitCentre.position - trailDirection * (2f + debrisIndex * 0.3f) + spreadOffset;

        float timeElapsed = 0f;
        while (timeElapsed < stateTransitionTime)
        {
            //lerp to trail position
            timeElapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startOrbitPos, endTrailPos, timeElapsed / stateTransitionTime);
            yield return null;
        }

        transform.position = endTrailPos;
    }

    private IEnumerator SwitchToOrbit()
    {
        //check if already orbiting
        if (isInOrbit)
            yield break;

        //set state to orbiting
        isInOrbit = true;
        isTrailing = false;

        //move debris to exact orbit position over transition time
        Vector2 startTrailPos = transform.position;
        Vector2 orbitOffset = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * orbitRadius;
        Vector2 targetOrbitPos = (Vector2)orbitCentre.position + orbitOffset;

        float timeElapsed = 0f;
        while (timeElapsed < stateTransitionTime)
        {
            //lerp to orbit position
            timeElapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startTrailPos, targetOrbitPos, timeElapsed / stateTransitionTime);
            yield return null;
        }

        transform.position = targetOrbitPos;
        timeSinceTrail = 0f;
    }

    private void Orbit()
    {
        //update current angle based on orbit speed
        currentAngle += orbitRotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
        Vector2 orbitOffset = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * orbitRadius;
        Vector2 targetOrbitPos = (Vector2)orbitCentre.position + orbitOffset;

        transform.position = Vector2.Lerp(transform.position, targetOrbitPos, Time.fixedDeltaTime * orbitFollowSpeed);
    }

    private void Trail()
    {
        if (!orbitCentre)
            return;

        Rigidbody2D orbitRb = orbitCentre.GetComponent<Rigidbody2D>();
        if (!orbitRb)
            return;

        // Get movement direction based on velocity, not facing direction
        Vector2 velocity = orbitRb.linearVelocity;

        // If velocity is near zero, fallback to facing direction
        Vector2 trailDirection;
        if (velocity.magnitude > 0.1f)
        {
            trailDirection = velocity.normalized;
        }
        else
        {
            trailDirection = -orbitCentre.up;
        }

        // Position debris behind player, relative to movement direction
        Vector2 spreadOffset = Vector2.Perpendicular(trailDirection).normalized * (debrisIndex * trailSpread);
        Vector2 lagOffset = -spreadOffset * (debrisIndex * trailLag);

        Vector2 targetTrailPos = (Vector2)orbitCentre.position - trailDirection * 2f + spreadOffset + lagOffset;

        transform.position = Vector2.Lerp(transform.position, targetTrailPos, Time.fixedDeltaTime * trailFollowSpeed);
    }

    public void SetInitialAngle(float angleOffset)
    {
        currentAngle = angleOffset;
    }

    private void OnDrawGizmos()
    {
        //draw line to gravity engine center if in range
        if (!orbitCentre)
        {
            return;
        }

        if (isInOrbit == true)
        {
            Gizmos.color = Color.green;
        }
        else if (isTrailing == true)
        {
            Gizmos.color = Color.red;
        }
        else if (isInGravityRange == true)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.gray;
        }

        Gizmos.DrawLine(transform.position, orbitCentre.position);
    }
}
