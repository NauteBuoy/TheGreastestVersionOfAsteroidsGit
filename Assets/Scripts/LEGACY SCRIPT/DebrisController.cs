using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class DebrisController : MonoBehaviour
{
    public enum State {Free, Pulling, Orbiting, Trailing } //possible states of debris

    [Header("State Settings")]
    [SerializeField] public State currentState = State.Free; //current state of debris
    [SerializeField] private bool isInGravityRange = false; //is debris within gravity engine range
    [SerializeField] private  bool isCapturing = false; //is debris being captured
    [SerializeField] private bool isTransitioning = false; //is debris transitioning between states

    public bool isInOrbit => currentState == State.Orbiting; //is debris currently orbiting
    public bool isTrailing => currentState == State.Trailing; //is debris currently trailing


    [Header("Orbit Settings")]
    public float orbitRotationSpeed = 180f; //speed of orbit in degrees per second
    public float orbitFollowSpeed = 8f; //speed at which debris follows orbit position
    public float stateTransitionDuration = 1f; //time to transition between orbit and trail states


    [Header("Trail Settings")]
    public float trailFollowSpeed = 6f; //speed at which debris follows trail position

        public float trailSpread = 0.6f; //spread of debris when trailing
        public float trailLag = 0.2f; // delay between each debris' response


    [Header("Capture Settings")]
    [SerializeField] private float captureProgress = 0f; //progress towards being captured into orbit
    public float captureDuration = 3f; //time required to be captured into orbit
    public float captureGravityFactor = 0.98f; //factor to reduce velocity while being captured
    public float releaseVelocityFactor = 1.02f; //factor to increase velocity when released from gravity engine


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

        switch (currentState)
        {
            case State.Orbiting:
                Orbit();
                break;
            case State.Trailing:
                Trail();
                break;
            case State.Pulling:
                //handled in UpdateCapture
                break;
            case State.Free:
                //free drift, physics handled by Rigidbody2D
                break;
        }
    }

    public void UpdateCapture(Transform gravityEngineTransfrom, float gravityEngineRadius, float gravityEngineStrength, float gravityEngineOrbitRadius, float playerSpeed, float playerSpeedThreshold, int capturedDebrisIndex)
    {
        //set orbit parameters
        orbitCentre = gravityEngineTransfrom;
        orbitRadius = gravityEngineOrbitRadius;
        debrisIndex = capturedDebrisIndex;

        //calculate distance from gravity engine center
        float distanceFromGravityCentre = Vector2.Distance(transform.position, orbitCentre.position);
        isInGravityRange = distanceFromGravityCentre < gravityEngineRadius;

        //manage orbit and trail state transitions
        UpdateOrbitTrailState(playerSpeed, playerSpeedThreshold);

        //don't process capture while orbiting
        if (currentState == State.Orbiting)
            return;
        //don't process capture while trailing
        if (currentState == State.Trailing)
            return;

        //check if within gravity engine range
        if (isInGravityRange)
        {
            //store initial velocity on first frame of capture
            if (!isCapturing)
            {
                initialVelocity = debrisRB.linearVelocity;
                isCapturing = true;
                currentState = State.Pulling;
            }

            //increase capture progress over time
            captureProgress += Time.fixedDeltaTime;
            captureProgress = Mathf.Clamp(captureProgress, 0f, captureDuration);

            //apply gravitational pull towards gravity engine center
            float gravityScale = Mathf.Lerp(1f, captureGravityFactor, captureProgress / captureDuration);
            Vector2 directionToGravityCenter = (orbitCentre.position - transform.position).normalized;
            debrisRB.AddForce(directionToGravityCenter * gravityEngineStrength, ForceMode2D.Force);
            debrisRB.linearVelocity *= gravityScale;

            //check if capture is complete
            if (captureProgress >= captureDuration && !isTransitioning)
            {
                //capture debris into orbit
                StartCoroutine(CaptureIntoOrbitRoutine());
            }
        }
        //not in gravity engine range
        else
        {
            //decrease capture progress over time
            captureProgress = Mathf.Max(0f, captureProgress - Time.fixedDeltaTime * 0.5f);

            //restore initial velocity
            Vector2 releaseVelocity = initialVelocity * releaseVelocityFactor;
            debrisRB.linearVelocity = Vector2.Lerp(debrisRB.linearVelocity, releaseVelocity, Time.fixedDeltaTime * 2f);

            if (captureProgress <= 0f)
            {
                isCapturing = false;
                currentState = State.Free;
            }
        }
    }

    void UpdateOrbitTrailState(float playerSpeed, float playerSpeedThreshold)
    {
        if (isTransitioning)
            return;

        //update time since trailing
        if (currentState == State.Trailing)
        {
            timeSinceTrail += Time.fixedDeltaTime;
        }
        else
        {
            timeSinceTrail = 0f; //reset timer if not trailing
        }

        if (currentState == State.Orbiting && playerSpeed >= playerSpeedThreshold)
        {
            //switch to trail state
            StartCoroutine(SwitchToTrailRoutine());
        }
        else if (currentState == State.Trailing && playerSpeed < playerSpeedThreshold)
        {
            //switch to orbit state
            StartCoroutine(SwitchToOrbitRoutine());
        }
    }

    private IEnumerator SwitchToTrailRoutine()
    {
        //check if already trailing 
        if (isTrailing)
            yield break;

        isTransitioning = true;

        currentState = State.Trailing;

        //move debris to exact trailing position over transition time
        Vector2 startOrbitPos = transform.position;
        Vector2 trailDirection = Vector2.zero;

        Rigidbody2D playerRB = orbitCentre.GetComponent<Rigidbody2D>();


        if (orbitCentre)
        {
            Vector2 playerVelocity = playerRB.linearVelocity;
            if (playerVelocity.magnitude > 0.1f)
            {
                trailDirection = playerVelocity.normalized;
            }
            else
            {
                trailDirection = -orbitCentre.up;
            }
        }

        Vector2 spreadOffset = Vector2.Perpendicular(trailDirection).normalized * (debrisIndex * trailSpread);
        Vector2 endTrailPos = (Vector2)orbitCentre.position - trailDirection * (2f + debrisIndex * 0.3f) + spreadOffset;

        float timeElapsed = 0f;
        while (timeElapsed < stateTransitionDuration)
        {
            //lerp to trail position
            timeElapsed += Time.fixedDeltaTime;
            float smoothDutration = Mathf.SmoothStep(0f, 1f, timeElapsed / stateTransitionDuration);
            transform.position = Vector2.Lerp(startOrbitPos, endTrailPos, smoothDutration);
            yield return new WaitForFixedUpdate();
        }

        debrisRB.MovePosition(endTrailPos);
        isTransitioning = false;
        yield break;
    }

    private IEnumerator SwitchToOrbitRoutine()
    {
        //check if already orbiting
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        //move debris to exact orbit position over transition time
        Vector2 startTrailPos = transform.position;
        Vector2 orbitOffset = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * orbitRadius;
        Vector2 targetOrbitPos = (Vector2)orbitCentre.position + orbitOffset;

        float timeElapsed = 0f;
        while (timeElapsed < stateTransitionDuration)
        {
            //lerp to orbit position
            timeElapsed += Time.fixedDeltaTime;
            float smoothTransitonDutration = Mathf.SmoothStep(0f, 1f, timeElapsed / stateTransitionDuration);

            debrisRB.MovePosition(transform.position = Vector2.Lerp(startTrailPos, targetOrbitPos, smoothTransitonDutration));

            yield return new WaitForFixedUpdate();
        }

        transform.position = targetOrbitPos;

        currentState = State.Orbiting;
        timeSinceTrail = 0f;
        isTransitioning = false;
        yield break;
    }

    private IEnumerator CaptureIntoOrbitRoutine()
    {
        //set state to orbiting
        if (isTransitioning) 
            yield break;

        isTransitioning = true;
        isCapturing = false;
        currentState = State.Orbiting;

        //disable collider trigger
        debrisRB.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().isTrigger = true;

        //calculate starting angle based on current position
        Vector2 directionToGravityCentre = (transform.position - orbitCentre.position).normalized;
        currentAngle = Mathf.Atan2(directionToGravityCentre.y, directionToGravityCentre.x);

        //move debris to exact orbit position over capture time
        Vector2 startCapturedPos = transform.position;
        Vector2 endOrbitPos = (Vector2)orbitCentre.position + directionToGravityCentre * orbitRadius;

        float timeElapsed = 0f;
        while (timeElapsed < captureDuration)
        {
            //lerp to orbit position
            timeElapsed += Time.fixedDeltaTime;
            float smoothDuration = Mathf.SmoothStep(0f, 1f, timeElapsed / captureDuration);
            transform.position = Vector2.Lerp(startCapturedPos, endOrbitPos, smoothDuration);
            yield return null;
        }

        //ensure exact orbit position
        transform.position = endOrbitPos;
        captureProgress = 0f;
        isTransitioning = false;
        yield break;
    }

    private void Orbit()
    {
        //update current angle based on orbit speed
        currentAngle += orbitRotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;

        Vector2 orbitOffset = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * orbitRadius;
        Vector2 targetOrbitPos = (Vector2)orbitCentre.position + orbitOffset;

        Vector2 targetOrbitRotationPos = Vector2.Lerp(transform.position, targetOrbitPos, Mathf.Clamp01(Time.fixedDeltaTime * orbitFollowSpeed));
        debrisRB.MovePosition(targetOrbitRotationPos);
    }

    private void Trail()
    {

    }

    public void SetInitialAngle(float targetOrbitAngle)
    {
        // only apply this once, right when the debris first enters orbit
        if (currentState != State.Orbiting)
        {
            currentAngle = targetOrbitAngle;
        }
    }

    private void OnDrawGizmos()
    {
        //draw line to gravity engine center if in range
        if (!orbitCentre)
            return;

        if (!isInGravityRange)
            return;

        if (currentState == State.Orbiting)
        {
            Gizmos.color = Color.green;
        }
        else if (currentState == State.Trailing)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }

        Gizmos.DrawLine(transform.position, orbitCentre.position);
    }
}
