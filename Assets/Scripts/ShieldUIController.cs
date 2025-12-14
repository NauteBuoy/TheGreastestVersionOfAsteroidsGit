using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ShieldUIController : MonoBehaviour
{
    [Header("Reference Settings")]
    public SpaceshipController playership; //reference to player ship
    public ShieldController shield; //reference to shield controller
    public Transform followTransform; //transform to follow
    public Transform shieldIndicator1; //first shield indicator
    public Transform shieldIndicator2; //second shield indicator

    [Header("Orbit/Follow Settings")]
    public float orbitRadius = 1.1f; //radius of orbit around player
    public float orbitSpeed = 180f; //degrees per second
    public float smoothFollowDuration = 0.06f; //smoothing duration for following
    public float orbitAngle; //current orbit angle in degrees
    Vector3 orbitCentreVel; //velocity reference for SmoothDamp


    void Start()
    {
        playership = SpaceshipController.playerInstance;
        shield = Object.FindAnyObjectByType<ShieldController>();
        followTransform = playership.transform;
    }

    void Update()
    {
        HandleShieldUI();
    }

    public void HandleShieldUI()
    {
        if (!shield)
            return;
        if (!followTransform)
            return;

        int shieldCount = shield.currentShieldCharges;

        Vector3 centre = Vector3.SmoothDamp(transform.position, followTransform.position, ref orbitCentreVel, smoothFollowDuration);
        transform.position = centre; 

        if (orbitAngle > 360f)
        {
            orbitAngle -= 360f;
        }

        orbitAngle += orbitSpeed * Time.deltaTime;
        float orbitRad = orbitAngle * Mathf.Deg2Rad;

        shieldIndicator1.gameObject.SetActive(shieldCount >= 1);
        shieldIndicator2.gameObject.SetActive(shieldCount >= 2);

        if (shieldCount >= 1 && shieldIndicator1)
        {
            Vector3 orbitPos1 = centre + new Vector3(Mathf.Cos(orbitRad), Mathf.Sin(orbitRad)) * orbitRadius;
            shieldIndicator1.position = orbitPos1;
        }

        if (shieldCount >= 2 && shieldIndicator2)
        {
            Vector3 orbitPos2 = centre + new Vector3(Mathf.Cos(orbitRad + Mathf.PI), Mathf.Sin(orbitRad + Mathf.PI)) * orbitRadius;
            shieldIndicator2.position = orbitPos2;
        }
    }

    public void SetShieldCount(int count)
    {
        if (shield)
        {
            shield.currentShieldCharges = count;
        }
    }
}
