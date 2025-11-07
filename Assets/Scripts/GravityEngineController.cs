using System.Collections.Generic;
using UnityEngine;

public class GravityEngineController : MonoBehaviour
{
    public static GravityEngineController Instance;

    [Header("Gravity Engine Settings")]
    public float gravityEngineRadius = 3f; //gravity influence radius
    public LayerMask debrisMask;

    [Header("Tracking")]
    public List<DebrisController> trackedDebris = new List<DebrisController>();

    [Header("Orbit Spacing")]
    public float orbitRadius = 1.5f;
    public float orbitRadiusSpacing = 0.3f;
    //public float spacingLerpSpeed = 2f; //how fast spacing adjusts

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //find all debris within radius
        Collider2D[] debrisInGravityRange = Physics2D.OverlapCircleAll(transform.position, gravityEngineRadius, debrisMask);

        foreach (var gravityDebris in debrisInGravityRange)
        {
            DebrisController debris = gravityDebris.GetComponent<DebrisController>();
            if (!debris)
                continue;

            //check for new tracked debris
            if (!trackedDebris.Contains(debris))
                trackedDebris.Add(debris);
        }

        // update orbit spacing if more than one debris is captured
        UpdateOrbitSpacing();
    }

    void UpdateOrbitSpacing()
    {
        int count = trackedDebris.Count;
        for (int i = trackedDebris.Count - 1; i >= 0; i--)
        {
            DebrisController debris = trackedDebris[i];
            if (!debris)
            {
                trackedDebris.RemoveAt(i);
                continue;
            }

            //update capture/decay/pull on every tracked debris
            debris.UpdateCapture(transform, gravityEngineRadius);

            if (!debris.isInOrbit)  
                continue;

            //evenly distribute captured debris
            float angleOffset = (2 * Mathf.PI / count) * i;
            debris.orbitAngleOffset = angleOffset;

            //smoothly adjust orbit radius
            float targetRadius = orbitRadius + i * orbitRadiusSpacing;
            debris.SetOrbitRadius(targetRadius);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gravityEngineRadius);
    }
}
