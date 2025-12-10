using UnityEngine;

public class ShieldUIController : MonoBehaviour
{
    [Header("Reference Settings")]
    public SpaceshipController playerShip;
    public Transform followTransform;
    public Transform shieldIndicator1;
    public Transform shieldIndicator2;


    [Header("Orbit/Follow Settings")]
    public float orbitRadius = 1.1f;
    public float orbitSpeed = 180f;
    public float smoothFollowDuration = 0.06f;
    public float orbitAngle;
    Vector3 followCentreVel;


    void Start()
    {
        playerShip = SpaceshipController.playerShipInstance;
        followTransform = playerShip.transform;
    }

    void Update()
    {
        HandleShieldUIOrbit();
        HandleShieldUIVisual();
    }

    public void HandleShieldUIOrbit()
    {
        if (!playerShip)
            return;

        Vector3 center = Vector3.SmoothDamp(transform.position, followTransform.position, ref followCentreVel, smoothFollowDuration);

        // Orbit positions
        orbitAngle += orbitSpeed * Time.deltaTime;
        float orbitRad = orbitAngle * Mathf.Deg2Rad;

        if (playerShip.currentShields >= 1)
        {
            Vector3 shieldUIPos1 = center + new Vector3(Mathf.Cos(orbitRad), Mathf.Sin(orbitRad)) * orbitRadius;
            shieldIndicator1.position = shieldUIPos1;
        }

        if (playerShip.currentShields >= 2)
        {
            Vector3 shieldUIPos2 = center + new Vector3(Mathf.Cos(orbitRad + Mathf.PI), Mathf.Sin(orbitRad + Mathf.PI)) * orbitRadius;
            shieldIndicator2.position = shieldUIPos2;
        }
    }

    public void HandleShieldUIVisual()
    {
        if (!playerShip)
            return;

        shieldIndicator1.gameObject.SetActive(playerShip.currentShields >= 1);
        shieldIndicator2.gameObject.SetActive(playerShip.currentShields >= 2);
    }
}
