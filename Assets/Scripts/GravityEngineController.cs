using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class GravityEngineController : MonoBehaviour
{
    public float pullRadius = 5f;
    public float captureRadius = 1.5f;
    public LayerMask debrisMask;

    List<DebrisController> capturedDebris = new List<DebrisController>();

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
        Collider2D[] debrisInRange = Physics2D.OverlapCircleAll(transform.position, pullRadius, debrisMask);
        HashSet<DebrisController> debrisInPullRange = new HashSet<DebrisController>();

        foreach (var debrisPiece in debrisInRange)
        {
            DebrisController debris = debrisPiece.GetComponent<DebrisController>();
            if (!debris) 
                continue;

            debrisInPullRange.Add(debris);

            if (!debris.isCaptured)
                debris.StartPull(transform, pullRadius);

            float distance = Vector2.Distance(transform.position, debris.transform.position);

            if (distance < captureRadius && !debris.isCaptured)
            {
                debris.Capture(transform);
                capturedDebris.Add(debris);
            }
        }

        foreach (var debris in Object.FindObjectsByType<DebrisController>(FindObjectsSortMode.None))
        {
            float distance = Vector2.Distance(transform.position, debris.transform.position);

            if (!debrisInPullRange.Contains(debris) && debris.isPulled && !debris.isCaptured)
                debris.StopPull();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pullRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }
}
