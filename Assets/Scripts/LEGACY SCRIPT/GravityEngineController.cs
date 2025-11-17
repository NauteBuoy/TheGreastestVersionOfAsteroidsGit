using System.Collections.Generic;
using UnityEngine;

public class GravityEngineController : MonoBehaviour
{
    [Header("Gravity Engine Settings")]
    public float gravityRadius = 3f; //how far gravity engine pulls debris
    public float gravityStrength = 6f; //how strong the gravity pull is


    [Header("Gravity Orbit Settings")]
    public float orbitRadius = 2f; //how far debris orbits from gravity engine center
    public float orbitspacingLerpSpeed = 1.2f; // how quickly debris align to spacing


    [Header("Gravity Trail Settings")]
    public float playerSpeedThreshold = 3f; //speed above which debris will trail instead of orbiting
    

    [Header("Debris Tracking List")]
    public List<DebrisController> trackedDebris = new List<DebrisController>(); //list of all debris currently being tracked by this gravity engine
    public LayerMask debrisMask; //layer mask for debris objects
    private Rigidbody2D playerRb; //reference to player rigidbody


    void Start()
    {
        //get player rigidbody
        playerRb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //check for new debris to track
        CheckForDebris();
        //update all tracked debris
        UpdateDebrisStates();
        //update orbit spacing for captured debris
        UpdateOrbitAngles();
    }

    public void CheckForDebris()
    {
        //find all debris within gravity engine range
        Collider2D[] insideGravityRange = Physics2D.OverlapCircleAll(transform.position, gravityRadius, debrisMask);

        //add any new debris to tracking list
        foreach (var pulledDebris in insideGravityRange)
        {
            var debris = pulledDebris.GetComponent<DebrisController>();
            if (debris && !trackedDebris.Contains(debris))
            {
                trackedDebris.Add(debris);
            } 
        }

        //clean up any debris that have been destroyed
        for (int i = trackedDebris.Count - 1; i >= 0; i--)
        { 
            if (!trackedDebris[i])
            {
                trackedDebris.RemoveAt(i);
            }
        }
    }

    public void UpdateDebrisStates()
    {
        //get player speed
        float playerSpeed = 0f;
        if (playerRb)
        {
            playerSpeed = playerRb.linearVelocity.magnitude;
        }

        //update each tracked debris
        for (int i = 0; i < trackedDebris.Count; i++)
        {
            var debris = trackedDebris[i];
            if (debris)
            {
                //this function handles everything: progress, decay, and restoring velocity
                debris.UpdateCapture(transform, gravityRadius, gravityStrength, orbitRadius, playerSpeed, playerSpeedThreshold, i);
            }
        }
    }

    public void UpdateOrbitAngles()
    {
        //get all debris currently in orbit
        var orbitingDebris = trackedDebris.FindAll(debris => debris && debris.isInOrbit);
        int count = orbitingDebris.Count;
        if (count == 0)
            return;

        float angleOffset = 2f * Mathf.PI / count;

        //set evenly spaced orbit angles for each debris
        for (int i = 0; i < count; i++)
        {
            DebrisController debris = orbitingDebris[i];
            if (debris.currentState == DebrisController.State.Orbiting)
            {
                //set initial angle for orbiting debris
                float targetOrbitAngle = angleOffset * i;
                debris.SetInitialAngle(targetOrbitAngle);
            }
        }
    }

    void OnDrawGizmos()
    {
        //draw gravity and orbit ranges`
        Gizmos.color = Color.white; //gravity capture zone
        Gizmos.DrawWireSphere(transform.position, gravityRadius);

        Gizmos.color = Color.cyan; //orbit zone
        Gizmos.DrawWireSphere(transform.position, orbitRadius);
    }
}
