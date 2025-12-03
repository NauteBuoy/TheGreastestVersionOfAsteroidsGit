using UnityEngine;

public class ShieldUIController : MonoBehaviour
{
    public Transform orbitCentre;
    public Transform shieldIndicator1;
    public Transform shieldIndicator2;

    public float orbitRadius = 1.1f;
    public float orbitSpeed = 180f;
    public float smoothDuration = 0.06f;

    public int shieldCount = 2;

    float orbitAngle;
    Vector3 orbitCentreVel;



    void Start()
    {

    }

    void Update()
    {
        if (!orbitCentre)
            return;

        Vector3 center = Vector3.SmoothDamp(transform.position, orbitCentre.position, ref orbitCentreVel, smoothDuration);

        // Orbit positions
        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;

        // Only show dots equal to shields
        shieldIndicator1.gameObject.SetActive(shieldCount >= 1);
        shieldIndicator2.gameObject.SetActive(shieldCount >= 2);

        if (shieldCount >= 1)
        {
            Vector3 orbitPos1 = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
            shieldIndicator1.position = orbitPos1;
        }

        if (shieldCount >= 2)
        {
            Vector3 orbitPos2 = center + new Vector3(Mathf.Cos(rad + Mathf.PI), Mathf.Sin(rad + Mathf.PI)) * orbitRadius;
            shieldIndicator2.position = orbitPos2;
        }
    }
}
