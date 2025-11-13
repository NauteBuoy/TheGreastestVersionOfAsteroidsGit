using System.Collections;
using System.Diagnostics;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SpaceshipController : MonoBehaviour
{
    [Header("Health Settings")]
    public float healthMax = 3f;
    public float healthCurrent;


    [Header("Movement Settings")]
    public float thrustForce = 200f;
    public float rotationSpeed = 5f;
    public float maxVelocity = 10f;


    [Header("Dash/Roll Settings")]
    public float dashForce = 10f;
    public float dashCooldown = 1f;
    public float dashDuration = 0.15f;
    public float squashAmount = 0.5f;    //how much to squash/stretch
    public float squashSpeed = 10f;      //how fast it returns to normal


    [Header("Private Settings")]
    public int score = 0;


    [Header("Private Settings")]
    private Rigidbody2D rbShip;
    private Vector2 movementInput;
    private float lastDashTime = -10f;
    private float dashTimer = 0f;
    private Vector3 baseScale;


    [Header("Bullet Settings")]
    public GameObject bulletObj;
    public float bulletOffset = 0.25f;
    public float bulletSpeed = 100f;
    public float fireRate = 0.33f;
    private float fireTimer = 0f;


    [Header("FX Settings")]
    //public GameObject EngineFX;
    public GameObject explosionFX;
    public ScreenFlashController screenFlash;
    public CameraShakeController cameraShake;
    public GameOverUIController gameOverUI;
    public float screenShakeMultiplier = 1f;


    void Start()
    {
        rbShip = GetComponent<Rigidbody2D>();
        rbShip.gravityScale = 0;
        healthCurrent = healthMax;
        baseScale = transform.localScale;
    }

    void Update()
    {
        HandleMovementInput();
        HandleDashInput();
        HandleSquash();
        UpdateFiring();
    }
    
    void FixedUpdate()
    {
        ApplyThrust(movementInput.y);
        ApplyTorque(movementInput.x);
        ClampVelocity();
    }

    public void TakeDamage(float damage)
    {
        healthCurrent = healthCurrent - damage;

        StartCoroutine(screenFlash.FlashRoutine());
        cameraShake.StartSceenShake(screenShakeMultiplier);

        if (healthCurrent <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        Instantiate(explosionFX, transform.position, Quaternion.identity);
        screenFlash.HideFlash();
        StartCoroutine(GameOverRoutine());
        Destroy(gameObject); 
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
        if (Input.GetKeyDown(KeyCode.X))
        {
            QuickTurn();// flip 180
        }
    }

    private void HandleMovementInput()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal"); //rotation
        movementInput.y = Input.GetAxisRaw("Vertical"); //thrust
    }

    private void ApplyThrust(float thrust)
    {
        Vector2 shipThrust = transform.up * thrust * thrustForce * Time.fixedDeltaTime;
        rbShip.AddForce(shipThrust, ForceMode2D.Force);
    }

    private void ApplyTorque(float rotate)
    {
        float shipRotate = rotate * rotationSpeed * Time.fixedDeltaTime;
        rbShip.AddTorque(-shipRotate);
    }

    private void ClampVelocity()
    {
        if (rbShip.linearVelocity.magnitude > maxVelocity)
            rbShip.linearVelocity = rbShip.linearVelocity.normalized * maxVelocity;
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public void SetHighScore (int highScore)
    {
        PlayerPrefs.SetInt("HighScore", highScore);
    }

    public IEnumerator GameOverRoutine()
    {
        bool newHighScore = false;
        if (score > GetHighScore())
        {
            SetHighScore(score);
            newHighScore = true;
        }

        gameOverUI.Show(newHighScore);
        float GameOverDuration = 0.5f;
        yield return new WaitForSeconds(GameOverDuration);
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
