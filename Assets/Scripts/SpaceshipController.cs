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
    [Header("Ship Reference Settings")]
    public static SpaceshipController playerInstance; //reference to player ship
    private Rigidbody2D playerShipRB; // reference to the ship's Rigidbody2D
    private Vector2 movementInput; // movement input vector
    private Vector3 baseScale; //original scale of the ship
    public UnityEngine.Transform shipVisual; // reference to the ship's visual transform
    public float visualRotationLag = 10f; // how quickly the visual lags behind rotation
    public SpriteRenderer shipRenderer;


    [Header("Health Settings")]
    public float healthMax = 10f; // max health
    public float healthCurrent; // current health


    [Header("Movement Settings")]
    public float thrustForce = 1200f; // force applied for thrust
    public float rotationTorque = 100f; // torque applied for rotation
    public float maxVelocity = 6f; // maximum speed
    public float velocityStretchX = 0.15f; // how much to squish X at top speed
    public float velocityStretchY = 0.25f; // how much to stretch Y at top speed
    public float velocityStretchLerp = 4f; // lerp speed for the visual stretch


    [Header("Barrel Roll Settings")]
    public float brCooldown = 0.2f; // seconds between dashes
    public float brDuration = 0.2f; // duration of dash
    public float squashAmount = 0.6f; //how much to squash/stretch
    public float squashSpeed = 10f;  //how fast it returns to normal


    [Header("Score Settings")]
    public int score = 0; // player score
    public int combo = 0; // current combo count
    public float comboResetTime = 1.5f; // time to reset combo
    public float comboTimer = 0f; // timer to track combo reset


    [Header("Bullet Settings")]
    public GameObject bulletPFB; // prefab for the bullet
    public float bulletOffset = 0.2f; // distance in front of ship to spawn bullet
    public float bulletForce = 10f; // force applied to the bullet
    public float bulletRate = 0.2f; // seconds between shots
    private float bulletTimer = 0f; // timer to track firing rate


    [Header("FX Settings")]
    public GameObject explosionFX; // explosion effect prefab
    public GameObject collsionFX; // collision effect prefab
    public GameObject muzzleFlashFX; // muzzle flash effect prefab
    public ParticleSystem thrustFX; // thrust particle system
    public ScreenFlashController screenFlash; // screen flash controller
    public GameOverUIController gameOverUI; // game over UI controller
    public CameraController cameraShake; // camera shake controller
    public float screenShakeMultiplier = 2f; // multiplier for screen shake intensity


    void Start()
    {
        playerInstance = this;
        playerShipRB = GetComponent<Rigidbody2D>();
        playerShipRB.gravityScale = 0;

        score = 0;
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
        HandleSpeedStretch();
        HandleComboTimer();
    }

    void FixedUpdate()
    {
        ApplyThrust(movementInput.y);
        ApplyTorque(movementInput.x);
        ClampVelocity();
    }

    private void LateUpdate()
    {
        // Smoothly rotate the ship visual to match the ship's rotation
        if (shipVisual)
        {
            //shipVisual.rotation = Quaternion.Lerp(shipVisual.rotation, transform.rotation, Time.deltaTime * visualRotationLag);

            Quaternion targetRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z);
            shipVisual.rotation = Quaternion.Slerp(shipVisual.rotation, targetRotation, Time.deltaTime * visualRotationLag);
        }
    }

    public void TakeDamage(float damage)
    {
        UpdateShipDamageVisual();

        healthCurrent = healthCurrent - damage;
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.shipCollisionSFX, AudioManagerController.Instance.normalCollisionVolume);

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
        Instantiate(muzzleFlashFX, spawnPos, transform.rotation);

        GameObject bulletInstance = Instantiate(bulletPFB, spawnPos, transform.rotation);
        Rigidbody2D bulletRB = bulletInstance.GetComponent<Rigidbody2D>();
        bulletRB.AddForce(transform.up * bulletForce, ForceMode2D.Impulse);

        playerShipRB.AddForce(-transform.up * 1f, ForceMode2D.Impulse);

        cameraShake.StartSceenShake(0.025f);
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
        {
            playerShipRB.linearVelocity = playerShipRB.linearVelocity.normalized * maxVelocity;
        }
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

    public float GetVelocity()
    {
        float velocity = playerShipRB.linearVelocity.magnitude;
        return Mathf.InverseLerp(0f, maxVelocity, velocity);
    }

    private void HandleSpeedStretch()
    {
        float velocity = GetVelocity();

        Vector3 stretchedScale = new Vector3(baseScale.x * (1f - velocityStretchX * velocity), baseScale.y * (1f + velocityStretchY * velocity), baseScale.z);
        transform.localScale = Vector3.Lerp(transform.localScale, stretchedScale, Time.deltaTime * velocityStretchLerp);
    }

    private void HandleComboTimer()
    {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0)
            combo = 0;
    }

    private void UpdateShipDamageVisual()
    {
        float healthPercentage = 1f - (healthCurrent / healthMax);
        // t goes 0 (full health) to 1 (dead)

        Color shipColour = Color.Lerp(Color.white, Color.red, healthPercentage * 0.8f);
        shipRenderer.color = shipColour;
    }
}
