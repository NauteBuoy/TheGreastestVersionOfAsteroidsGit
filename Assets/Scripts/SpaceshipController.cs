using System.Diagnostics;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SpaceshipController : MonoBehaviour
{
    [Header("Ship Settings")]
    public float healthMax = 3f;
    public float healthCurrent;
    public float enginePower = 200f;
    public float rotationPower = 5f;
    public float maxVelocity = 10f;

    [Header("Dash/Roll Settings")]
    public float dashForce = 10f;
    public float dashCooldown = 1f;
    public float dashDuration = 0.15f;
    public float squashAmount = 0.5f;    //how much to squash/stretch
    public float squashSpeed = 10f;      //how fast it returns to normal

    [Header("Private Settings")]
    private Rigidbody2D rbShip;
    private float lastDashTime = -10f;
    private float dashTimer = 0f;
    private Vector3 baseScale;

    [Header("Bullet Settings")]
    public GameObject bulletObj;
    public float bulletOffset = 0.25f;
    public float bulletSpeed = 100f;
    public float fireRate = 0.33f;
    private float fireTimer = 0f;

    [Header("Particle Settings")]
    public GameObject EngineFX;
    public GameObject explosionFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthCurrent = healthMax;
        rbShip = GetComponent<Rigidbody2D>();
        rbShip.gravityScale = 0f;
        baseScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        HandleDashInput();
        HandleSquash();
        UpdateFiring();
    }
    
    void FixedUpdate()
    {
        float horiz = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");

        ApplyThrust(vert);
        ApplyTorque(horiz);
        ClampVelocity();
    }

    public void TakeDamage(float damage)
    {
        healthCurrent = healthCurrent - damage;
        if (healthCurrent <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        Instantiate(explosionFX, transform.position, Quaternion.identity);
        Destroy(gameObject); // remove the spaceship!
    }

    private void UpdateFiring()
    {
        bool isFiring = Input.GetKeyDown(KeyCode.Space);
        fireTimer = fireTimer - Time.deltaTime; 

        if (isFiring && fireTimer <= 0f)
        {
            FireBullet();
            fireTimer = fireRate;
        }
    }

    public void FireBullet()
    {
        Vector3 spawnPos = transform.position + transform.up * bulletOffset;
        GameObject bullet = Instantiate(bulletObj, spawnPos, transform.rotation);

        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        rbBullet.AddForce(transform.up * bulletSpeed, ForceMode2D.Impulse);

        Destroy(bullet, 1f);
    }

    public void HandleDashInput()
    {
        if (Time.time < lastDashTime + dashCooldown) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Dash(-transform.right);     // sidestep left
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Dash(transform.right);      // sidestep right
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            QuickTurn();                // flip 180
        }
    }

    private void ApplyThrust(float amount)
    {
        Vector2 thrust = transform.up * enginePower * Time.deltaTime * amount;
        rbShip.AddForce(thrust);
    }

    private void ApplyTorque(float amount)
    {
        float torque = amount * Time.deltaTime * rotationPower;
        rbShip.AddTorque(-torque);
    }

    private void ClampVelocity()
    {
        if (rbShip.linearVelocity.magnitude > maxVelocity)
            rbShip.linearVelocity = rbShip.linearVelocity.normalized * maxVelocity;
    }

    private void Dash(Vector2 direction, bool isSideDash = false)
    {
        rbShip.AddForce(direction.normalized * dashForce, ForceMode2D.Impulse);
        lastDashTime = Time.time;
        dashTimer = dashDuration;
        SquashStretch(direction, true);
    }

    private void QuickTurn()
    {
        lastDashTime = Time.time;
        dashTimer = dashDuration;
        
        transform.up = -transform.up;   // Flip instantly 180 degrees around Z
        SquashStretch(-transform.up, false);
    }

    private void SquashStretch(Vector2 direction, bool isSideways)
    {
        // squash in the direction of movement
        if (isSideways)
        {
            // Sidestep dash: squash perpendicular to movement
            transform.localScale = new Vector3(baseScale.x * (1f - squashAmount), baseScale.y * (1f + squashAmount), baseScale.z);
        }
        else
        {
            // Flip/back dash: squash along forward/backward
            transform.localScale = new Vector3(baseScale.x * (1f + squashAmount), baseScale.y * (1f - squashAmount), baseScale.z);
        }
    }

    private void HandleSquash()
    {
        // smoothly return to normal scale
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * squashSpeed);
    }
}
