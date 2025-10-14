using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem waterSpray;   // continuous engine spray
    public ParticleSystem dashSpray;    // Q/E sidestep burst
    public ParticleSystem ripple;       // ripple under ship

    [Header("Settings")]
    public float waterSprayReductionTime = 0.15f; // short pause during X flip
    public float minSprayVelocity = 0.05f;       // when movement triggers particles

    [Header("Private Settings")]
    private Rigidbody2D rb2d;
    private float waterSprayTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleWaterSpray();
    }

    private void HandleWaterSpray()
    {
        if (waterSpray == null) return;

        if (waterSprayTimer > 0f)
        {
            waterSprayTimer -= Time.deltaTime;
            if (waterSpray.isEmitting) waterSpray.Stop();
            return;
        }

        // Check if ship is moving
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity.magnitude > minSprayVelocity)
        {
            if (!waterSpray.isEmitting) waterSpray.Play();
        }
        else
        {
            if (waterSpray.isEmitting) waterSpray.Stop();
        }
    }

    public void PauseWaterSpray()
    {
        waterSprayTimer = waterSprayReductionTime;
    }
}
