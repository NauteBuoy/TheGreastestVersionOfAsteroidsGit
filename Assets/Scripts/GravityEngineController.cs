using System.Collections.Generic;
using UnityEngine;

public class GravityEngineController : MonoBehaviour
{
    [Header("Gravity Engine Settings")]
    public float gravityRadius = 3f; //how far gravity engine pulls debris
    public float gravityStrength = 3f; //how strong the gravity pull is


    [Header("Gravity Orbit Settings")]
    public float orbitRadius = 2f; //how far debris orbits from gravity engine center
    public float orbitSpacingSmoothness = 5f;


    [Header("Gravity Trail Settings")]
    public float playerSpeedThreshold = 3f; //speed above which debris will trail instead of orbiting
    public float reenterOrbitDelay = 1f; //how long debris must wait before re-entering orbit


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
            DebrisController debris = pulledDebris.GetComponent<DebrisController>();
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
            DebrisController debris = trackedDebris[i];
            if (debris)
            {
                //this function handles everything: progress, decay, and restoring velocity
                debris.UpdateCapture(transform, gravityRadius, gravityStrength, orbitRadius, playerSpeed, playerSpeedThreshold, reenterOrbitDelay, i);
            }
        }
    }

    public void UpdateOrbitAngles()
    {
        //get all debris currently in orbit
        List<DebrisController> orbitingDebris = trackedDebris.FindAll(debris => debris && debris.isInOrbit);
        int count = orbitingDebris.Count;
        if (count == 0)
            return;

        //set evenly spaced orbit angles for each debris
        for (int i = 0; i < count; i++)
        {
            //set initial angle for orbiting debris
            float angleOffset = (2 * Mathf.PI / count) * i;
            orbitingDebris[i].SetInitialAngle(angleOffset);
        }
    }

    void OnDrawGizmos()
    {
        //draw gravity and orbit ranges`
        Gizmos.color = Color.yellow; //gravity capture zone
        Gizmos.DrawWireSphere(transform.position, gravityRadius);

        Gizmos.color = Color.green; //orbit zone
        Gizmos.DrawWireSphere(transform.position, orbitRadius);
    }
}
