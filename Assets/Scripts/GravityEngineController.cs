using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityEngineController : MonoBehaviour
{
    [Header("Gravity Engine Settings")]
    public float gravityEngineRadius = 3f; //gravity influence radius (bigger)
    public LayerMask debrisMask;

    [Header("List Settings")]
    public List<DebrisController> trackedDebris = new List<DebrisController>();
    public List<DebrisController> capturedDebris = new List<DebrisController>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        Collider2D[] debrisInGravityRange = Physics2D.OverlapCircleAll(transform.position, gravityEngineRadius, debrisMask);
        HashSet<DebrisController> influencedDebris = new HashSet<DebrisController>();

        foreach (var gravityDebris in debrisInGravityRange)
        {
            DebrisController debris = gravityDebris.GetComponent<DebrisController>();
            if (!gravityDebris)
                continue;

            influencedDebris.Add(debris);

            //if not already tracked, add it
            if (!trackedDebris.Contains(debris))
                trackedDebris.Add(debris);

            debris.UpdateCapture(transform, gravityEngineRadius);
        }

        for (int i = trackedDebris.Count - 1; i >= 0; i--)
        {
            DebrisController debris = trackedDebris[i];
            if (debris == null)
            {
                trackedDebris.RemoveAt(i);
                continue;
            }

            if (!influencedDebris.Contains(debris))
            {
                debris.UpdateCapture(transform, gravityEngineRadius);

                // Stop tracking once fully reset
                if (debris.captureProgress <= 0)
                    trackedDebris.RemoveAt(i);
            }

            if (influencedDebris.isInOrbit && !capturedDebris.Contains(influencedDebris))
            {
                capturedDebris.Add(influencedDebris);
                UpdateOrbitSpacing();
            }
        }
    }

    void UpdateOrbitSpacing()
    {
        int count = capturedDebris.Count;
        if (count == 0)     
            return;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            if (capturedDebris[i] == null) 
                continue;

            float angle = angleStep * i * Mathf.Deg2Rad;
            capturedDebris[i].SetOrbitAngle(angle);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gravityEngineRadius);
    }
}
