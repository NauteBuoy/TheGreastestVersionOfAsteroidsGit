using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class SpaceshipController : MonoBehaviour
{
    [Header("Ship Reference Settings")]
    public static SpaceshipController playerShipInstance; //reference to THIS player ship
    public Transform shipVisual; //reference to player ship sprite transform
    public SpriteRenderer shipRenderer; //reference to player ship sprite renderer
    private Rigidbody2D playerShipRB; //reference to player ship Rigidbody2D
    private Vector2 playerMovementInput; //player movement input vector
    private Vector3 shipBaseScale; //original scale of the ship


    [Header("Heat Settings")]
    public float maxHeat = 100f; //maximum heat threshold
    public float currentHeat = 0f; //current heat
    public float heatRateThrust = 6f; //amount of heat generated per second of thrust
    public float passiveCoolRate = 12f; //amount of heat cooled per second


    [Header("Movement Settings")]
    public float thrustForce = 1200f; //force applied to ship on thrust
    public float thrustForceHeatFactor = 0.01f; //How much % extra thrust is added per heat point
    public float rotationTorque = 100f; //torque applied for rotation
    public float maxVelocity = 6f; //maximum speed threshold
    public float maxVelocityHeatFactor = 0.015f; //How much % extra max velocity scales per heat point 
    public float velocityStretchX = 0.15f; //how much to squish X at top speed
    public float velocityStretchY = 0.25f; //how much to stretch Y at top speed
    private float baseMaxVelocity; //base max velocity
    private float baseThrustForce; //base thrust force


    [Header("Discharge/Barrel Roll Settings")]
    public DischargeController dischargeController;
    public float dischargeOffset = 0.2f; //distance in front of ship to spawn bullet
    public float squashAmount = 0.6f; //how much does ship squash/stretch
    public float squashStretchReturnSpeed = 10f;  //how fast ship returns to base scale


    [Header("Score Settings")]
    public int score = 0; //current player score
    public int combo = 0; //current combo count
    public float comboResetTime = 1.5f; //time it takes to reset combo
    public float comboTimer = 0f; //timer to track combo reset


    [Header("FX Settings")]
    public GameObject explosionFX; // explosion effect prefab
    public GameObject collsionFX; // collision effect prefab
    public ParticleSystem thrustFX; // thrust particle system
    public ScreenFlashController screenFlash; // screen flash controller
    public GameOverUIController gameOverUI; // game over UI controller
    public CameraController cameraShake; // camera shake controller
    public float screenShakeDamageMultiplier = 2f; // multiplier for screen shake intensity
    public float visualRotationLag = 10f; // how quickly the visual lags behind rotation

    void Start()
    {
        playerShipInstance = this;
        playerShipRB = GetComponent<Rigidbody2D>();
        playerShipRB.gravityScale = 0;

        shipBaseScale = transform.localScale;
        baseMaxVelocity = maxVelocity;
        baseThrustForce = thrustForce;

        if (!cameraShake)
        {
            cameraShake = Object.FindAnyObjectByType<CameraController>();
        }
        if (!screenFlash)
        {
            screenFlash = Object.FindAnyObjectByType<ScreenFlashController>();
        }
        if (!gameOverUI)
        {
            gameOverUI = Object.FindAnyObjectByType<GameOverUIController>();
        }  
    }

    void Update()
    {
        HandleMovementInput();
        HandleHeat();
        HandleDiscarge();
        HandleFX();
        HandleVisualSquashStretch();
        HandleComboTimer();
    }

    void FixedUpdate()
    {
        HandleMovementPhysics();
    }

    private void LateUpdate()
    {
        HandleVisualLagRotation();
    }

    private void HandleMovementInput()
    {
        playerMovementInput.x = Input.GetAxisRaw("Horizontal"); //rotation
        playerMovementInput.y = Input.GetAxisRaw("Vertical"); //thrust
    }

    void HandleHeat()
    {
        bool thrusting = playerMovementInput.y > 0.1f;
        // Handle heat generation and cooling
        if (thrusting)
        {
            currentHeat = Mathf.Min(maxHeat, currentHeat + heatRateThrust * Time.deltaTime);
        }
        else
        {
            currentHeat = Mathf.Max(0f, currentHeat - passiveCoolRate * Time.deltaTime);
        }
        // clamp safety
        currentHeat = Mathf.Clamp(currentHeat, 0f, maxHeat);
        // optional: update ship glow here
        UpdateHeatVisuals();

        if (currentHeat >= maxHeat)
        {
            Explode();
        }
    }

    public void AddHeat(float amount)
    {
        currentHeat = Mathf.Clamp(currentHeat + amount, 0f, maxHeat);
    }

    public float GetHeatNorm()
    {
        return Mathf.Clamp01(currentHeat / maxHeat);
    }

    void HandleMovementPhysics()
    {
        ApplyThrust(playerMovementInput.y);
        ApplyTorque(playerMovementInput.x);
        ClampVelocity();
    }

    private void ApplyThrust(float thrust)
    {
        float heatBoost = 1f + (GetHeatNorm() * thrustForceHeatFactor);
        float heatedThrust = baseThrustForce * heatBoost;

        playerShipRB.AddForce(transform.up * thrust * heatedThrust * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    private void ApplyTorque(float rotation)
    {
        float torque = rotation * rotationTorque * Time.fixedDeltaTime;
        playerShipRB.AddTorque(-torque);
    }

    private void ClampVelocity()
    {
        float heatBoost = 1f + (GetHeatNorm() * maxVelocityHeatFactor);
        float maxVelocity = baseMaxVelocity * heatBoost;

        if (playerShipRB.linearVelocity.magnitude > maxVelocity)
        {
            playerShipRB.linearVelocity = playerShipRB.linearVelocity.normalized * maxVelocity;
        }
    }

    private void HandleDiscarge()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryDischargeHeat();
        }
    }

    void TryDischargeHeat()
    {
        if (!playerShipRB)
            return;
        if (!dischargeController.CanDischarge())
            return;

        float heatNorm = GetHeatNorm();
        Vector2 shipDirection = transform.up.normalized;

        //get discharge output results
        var dischargeOutputs = dischargeController.DischargeHeatResults(currentHeat, maxHeat, shipDirection, heatNorm);

        //cancel velocity before recoil
        float cancelVelocityFactor = Mathf.Lerp(0.05f, 0.55f, heatNorm);
        playerShipRB.linearVelocity *= (1f - cancelVelocityFactor);

        // Flip ship instantly
        Vector2 recoilDirection = shipDirection;
        transform.up = -transform.up;

        // Apply recoil
        float recoilForce = dischargeOutputs.recoilAmount;
        playerShipRB.AddForce(-recoilDirection * recoilForce, ForceMode2D.Impulse);

        DischargeVisualSquash();

        // Reduce heat
        currentHeat = Mathf.Max(0, currentHeat - dischargeOutputs.heatSpent);
    }

    private void DischargeVisualSquash()
    {
        transform.localScale = new Vector3(shipBaseScale.x * (1f + squashAmount), shipBaseScale.y * (1f - squashAmount), shipBaseScale.z);
    }

    private void HandleFX()
    {
        if (!thrustFX)
            return;

        bool thrusting = playerMovementInput.y > 0.1f;
        float heatNorm = GetHeatNorm();

        var emission = thrustFX.emission;
        float targetRate = Mathf.Lerp(0f, 60f, Mathf.Clamp01(playerMovementInput.y) + (heatNorm * 40f));
        emission.rateOverTime = targetRate;

        AudioManagerController.Instance.PlayThruster(thrusting);

        if (thrusting)
        {
            if (!thrustFX.isPlaying)
            {
                thrustFX.Play();
            }
        }
        else
        {
            if (thrustFX.isPlaying)
            {
                thrustFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    void HandleVisualLagRotation()
    {
        if (!shipVisual)
            return;

        Quaternion targetRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z);
        shipVisual.rotation = Quaternion.Slerp(shipVisual.rotation, targetRotation, Time.deltaTime * visualRotationLag);
    }

    void HandleVisualSquashStretch()
    {
        float velocityNorm = GetVelocityNorm();

        Vector3 targetVelocityScale = new Vector3(shipBaseScale.x * (1f - velocityStretchX * velocityNorm), shipBaseScale.y * (1f + velocityStretchY * velocityNorm), shipBaseScale.z);

        transform.localScale = Vector3.Lerp(transform.localScale, targetVelocityScale, Time.deltaTime * squashStretchReturnSpeed);
    }

    public float GetVelocityNorm()
    {
        float currentShipVelocity = playerShipRB.linearVelocity.magnitude;
        float maxVelocity = baseMaxVelocity * (1f + maxVelocityHeatFactor * maxHeat);
        float velocityNorm = Mathf.InverseLerp(0f, maxVelocity, currentShipVelocity);
        return velocityNorm;
    }

    private void UpdateHeatVisuals()
    {
        float heatNorm = GetHeatNorm();

        Color coolEngine = Color.white;
        Color warmEngine = new Color(1f, 0.65f, 0.25f);
        Color ultraHotEngine = Color.Lerp(warmEngine, Color.red, Mathf.Clamp01((heatNorm - 0.85f) / 0.15f));

        Color shipHeatColour = Color.Lerp(coolEngine, warmEngine, heatNorm);

        if (heatNorm > 0.85f) shipHeatColour = ultraHotEngine;

        if (shipRenderer)
        {
            shipRenderer.color = shipHeatColour;
        }
    }

    public void TakeDamage(float damage)
    {
        AddHeat(damage);
        UpdateHeatVisuals();

        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.shipCollisionSFX, AudioManagerController.Instance.normalCollisionVolume);

        if (collsionFX)
        {
            Instantiate(collsionFX, transform.position, Quaternion.identity);
        }

        StartCoroutine(screenFlash.FlashRoutine());

        if (cameraShake)
        {
            cameraShake.StartSceenShake(screenShakeDamageMultiplier);
        }
    }

    public void Explode()
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.shipDeathSFX, AudioManagerController.Instance.normalCollisionVolume);

        if (explosionFX)
        {
            Instantiate(explosionFX, transform.position, Quaternion.identity);
        }

        screenFlash.HideFlash();

        StartCoroutine(GameOverRoutine());
        Destroy(gameObject);
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public void SetHighScore(int highScore)
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
        yield return null;
    }

    private void HandleComboTimer()
    {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0)
            combo = 0;
    }
}
