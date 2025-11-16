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
    public float healthMax = 3f; // max health
    public float healthCurrent; // current health


    [Header("Movement Settings")]
    public float thrustForce = 200f; // force applied for thrust
    public float rotationTorque = 5f; // torque applied for rotation
    public float maxVelocity = 10f; // maximum speed


    [Header("Dash/Roll Settings")]
    public float dashCooldown = 1f; // seconds between dashes
    public float dashDuration = 0.15f; // duration of dash
    public float squashAmount = 0.5f; //how much to squash/stretch
    public float squashSpeed = 10f;  //how fast it returns to normal


    [Header("Private Settings")]
    public int score = 0; // player score
    public static SpaceshipController playerInstance; //reference to player ship


    [Header("Bullet Settings")]
    public GameObject bulletPFB; // prefab for the bullet
    public float bulletOffset = 0.25f; // distance in front of ship to spawn bullet
    public float bulletForce = 100f; // force applied to the bullet
    public float bulletRate = 0.33f; // seconds between shots
    private float bulletTimer = 0f; // timer to track firing rate
    public float bulletDuration = 2f; // seconds before bullet is destroyed


    [Header("FX Settings")]
    //public GameObject EngineFX;
    public GameObject explosionFX; // explosion effect prefab
    public GameObject collsionFX; // collision effect prefab
    public ScreenFlashController screenFlash; // screen flash controller
    public CameraShakeController cameraShake; // camera shake controller
    public GameOverUIController gameOverUI; // game over UI controller
    public float screenShakeMultiplier = 1f; // multiplier for screen shake intensity
    public ParticleSystem thrustFX;


    [Header("Private Settings")]
    private Rigidbody2D playerShipRB; // reference to the ship's Rigidbody2D
    private Vector2 movementInput; // movement input vector
    private float lastDashTime = -10f; // time when the last dash occurred
    private float dashTimer = 0f; // timer to track dash duration
    private Vector3 baseScale; //original scale of the ship


    void Start()
    {
        playerInstance = this;
        playerShipRB = GetComponent<Rigidbody2D>();
        playerShipRB.gravityScale = 0;
        healthCurrent = healthMax;
        baseScale = transform.localScale;
    }

    void Update()
    {
        HandleMovementInput();
        HandleDashInput();
        HandleSquash();
        UpdateFiring();
        HandleFX();
    }
    
    void FixedUpdate()
    {
        ApplyThrust(movementInput.y);
        ApplyTorque(movementInput.x);
        ClampVelocity();
    }

    public void TakeDamage(float damage)
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.shipCollisionSFX, AudioManagerController.Instance.normalCollisionVolume);


        healthCurrent = healthCurrent - damage;

        Instantiate(collsionFX, transform.position, Quaternion.identity);
        StartCoroutine(screenFlash.FlashRoutine());
        cameraShake.StartSceenShake(screenShakeMultiplier);

        if (healthCurrent <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.shipDeathSFX, AudioManagerController.Instance.normalCollisionVolume);

        Instantiate(explosionFX, transform.position, Quaternion.identity);
        screenFlash.HideFlash();
        StartCoroutine(GameOverRoutine());
        Destroy(gameObject); 
    }

    private void UpdateFiring()
    {
        bool isFiring = Input.GetKey(KeyCode.Space);
        bulletTimer = bulletTimer - Time.deltaTime;

        if (isFiring && bulletTimer <= 0f)
        {  
            bulletTimer = bulletRate;
            FireBullet();
        }
    }

    public void FireBullet()
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.bulletSFX, AudioManagerController.Instance.normalCollisionVolume);

        Vector3 spawnPos = transform.position + transform.up * bulletOffset;
        GameObject bulletInstance = Instantiate(bulletPFB, spawnPos, transform.rotation);

        Rigidbody2D bulletRB = bulletInstance.GetComponent<Rigidbody2D>();
        bulletRB.AddForce(transform.up * bulletForce, ForceMode2D.Impulse);

        Destroy(bulletInstance, bulletDuration);
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
        playerShipRB.AddForce(shipThrust, ForceMode2D.Force);
    }

    private void ApplyTorque(float rotate)
    {
        float shipRotate = rotate * rotationTorque * Time.fixedDeltaTime;
        playerShipRB.AddTorque(-shipRotate);
    }

    private void ClampVelocity()
    {
        if (playerShipRB.linearVelocity.magnitude > maxVelocity)
            playerShipRB.linearVelocity = playerShipRB.linearVelocity.normalized * maxVelocity;
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

    private void HandleFX()
    {
        if (!thrustFX)
            return;

        var emission = thrustFX.emission;
        bool thrusting = movementInput.y > 0.1f;
        AudioManagerController.Instance.PlayThruster(thrusting);

        if (thrusting)
        {
            emission.rateOverTime = Mathf.Lerp(0, 30f, movementInput.y);

            if (!thrustFX.isPlaying)
            {
                thrustFX.Play();
            }
        }
        else
        {
            emission.rateOverTime = 0;

            if (thrustFX.isPlaying)
            {
                thrustFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            } 
        }
    }
}
