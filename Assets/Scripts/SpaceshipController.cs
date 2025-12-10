using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class SpaceshipController : MonoBehaviour
{
    public enum HeatState
    {
        Cool, Warm, Hot, Critical
    }

    [Header("Ship Reference Settings")]
    public static SpaceshipController playerShipInstance; //reference to THIS player ship
    public Transform shipVisual; //reference to player ship sprite transform
    public SpriteRenderer shipRenderer; //reference to player ship sprite renderer
    private Rigidbody2D playerShipRB; //reference to player ship Rigidbody2D
    private Vector2 playerMovementInput; //player movement input vector
    private Vector3 shipBaseScale; //original scale of the ship


    [Header("Shield Reference Settings")]
    public SpriteRenderer shieldVisual;
    public float shieldBaseScale = 1f;
    public float shieldWeakScale = 0.8f;
    public float shieldHitScale = 1.2f;
    public int maxShields = 2;
    public int currentShields = 2;
    public float shieldRechargeDelay = 3f;
    public float shieldRechargeRate = 2f;
    public GameObject shieldShatterFX;
    public bool isRecharging = false;


    [Header("Heat State Settings")]
    public HeatState currentHeatState = HeatState.Warm; //current heat state
    public float stateBlendDuration = 3f; //duration for stat blending
    public float velocityHeatFactor = 0.8f; //how much does heat affect max velocity
    float thrustMultiplier = 1f;
    float velMultiplier = 1f;
    float torqueMultiplier = 1f;
    float targetThrustMultiplier = 1f;
    float targetVelMultiplier = 1f;
    float targetTorqueMultiplier = 1f;


    [Header("Frozen State Settings")]
    public float freezeThrust = 0.4f;
    public float freezeMaxVel = 0.4f;
    public float freezeTorque = 0.6f;

        public float freezeMeter = 0f;

    public float freezeBuildRate = 0.25f;
    public float freezeDecayRate = 0.5f;
    public bool isFrozen = false;

    [Header("Cool State Settings")]
    public float coolThrust = 0.8f;
    public float coolMaxVel = 0.8f;
    public float coolTorque = 1f;

    [Header("Warm State Settings")]
    public float warmThrust = 1f;
    public float warmMaxVel = 1f;
    public float warmTorque = 1.1f;

    [Header("Hot State Settings")]
    public float hotThrust = 1.3f;
    public float hotMaxVel = 1.3f;
    public float hotTorque = 1.2f;

    [Header("Critical State Settings")]
    public float criticalThrust = 1.8f;
    public float criticalMaxVel = 2.0f;
    public float criticalTorque = 1.5f;


    [Header("Heat Settings")]
    public float maxHeat = 100f; //maximum heat threshold
    public float currentHeat = 0f; //current heat
    public float heatBuildRate = 2f; //amount of heat generated per second of thrust
    public float heatDecayRate = 6f; //amount of heat cooled per second


    [Header("Movement Settings")]
    public float thrustForce = 1200f; //force applied to ship on thrust
    public float rotationTorque = 100f; //torque applied for rotation
    public float maxVelocity = 6f; //maximum speed threshold
    private float baseMaxVel; //base max velocity
    private float baseThrust; //base thrust force


    [Header("Discharge/Barrel Roll Settings")]
    public DischargeController dischargeController;
    public float dischargeOffset = 0.2f; //distance in front of ship to spawn bullet


    [Header("Squash and Stretch Settings")]
    public float squashAmount = 0.6f; //how much does ship squash/stretch
    public float squashStretchDuration = 6f;
    public float squashStretchReturnSpeed = 12f;  //how fast ship returns to base scale
    public float velocityStretchX = 0.15f; //how much to squish X at top speed
    public float velocityStretchY = 0.25f; //how much to stretch Y at top speed
    private Vector3 squashScaleVelocity;


    [Header("Score Settings")]
    public int score = 0; //current player score


    [Header("FX Settings")]
    public ParticleSystem thrustFX; // thrust particle system
    public GameObject explosionFX; // explosion effect prefab
    public GameObject collsionFX; // collision effect prefab
    public ScreenFlashController screenFlash; // screen flash controller
    public CameraController cameraShake; // camera shake controller
    public GameOverUIController gameOverUI; // game over UI controller
    public ShieldUIController shieldUI; // shield UI controller
    public float screenShakeCollisionMultiplier = 1f; // multiplier for screen shake intensity
    public float screenShakeDamageMultiplier = 2f; // multiplier for screen shake intensity
    public float visualRotationLag = 10f; // how quickly the visual lags behind rotation


    private void Awake()
    {
        if (!playerShipInstance) 
        {
            playerShipInstance = this;
        }
    }

    void Start()
    {
        playerShipRB = GetComponent<Rigidbody2D>();
        shipBaseScale = transform.localScale;
        baseMaxVel = maxVelocity;
        baseThrust = thrustForce;

        OnHeatStateChanged(currentHeatState);
        thrustMultiplier = targetThrustMultiplier;
        velMultiplier = targetVelMultiplier;
        torqueMultiplier = targetTorqueMultiplier;
    }

    void Update()
    {
        HandleMovementInput();
        HandleHeat();
        HandleShieldVFX();
        HandleVFX();
        HandleDiscarge();
        HandleSquashStretch();
    }

    void FixedUpdate()
    {
        HandleMovementPhysics();
    }

    private void LateUpdate()
    {
        //HandleVisualLagRotation();
    }

    private void HandleMovementInput()
    {
        playerMovementInput.x = Input.GetAxisRaw("Horizontal"); 
        playerMovementInput.y = Input.GetAxisRaw("Vertical"); 
    }

    void HandleMovementPhysics()
    {
        ApplyThrust(playerMovementInput.y);
        ApplyTorque(playerMovementInput.x);
        ClampVelocity();
    }

    private void ApplyThrust(float thrustInput)
    {
        float heatBoost = 1f + (GetHeatNorm() * velocityHeatFactor);
        float finalThrust = baseThrust * heatBoost * thrustMultiplier;
        playerShipRB.AddForce(transform.up * thrustInput * finalThrust * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    private void ApplyTorque(float torqueInput)
    {
        float finalTorque = rotationTorque * torqueMultiplier * Time.fixedDeltaTime;
        playerShipRB.AddTorque(-torqueInput * finalTorque);
    }

    private void ClampVelocity()
    {
        float heatBoost = 1f + (GetHeatNorm() * velocityHeatFactor);
        float finalVelocity = baseMaxVel * heatBoost * velMultiplier;
        float playerVelocity = playerShipRB.linearVelocity.magnitude;

        if (playerVelocity > finalVelocity)
        {
            playerShipRB.linearVelocity = playerShipRB.linearVelocity.normalized * finalVelocity;
        }
    }

   

    void HandleHeat()
    {
        bool thrusting = playerMovementInput.y > 0.1f;

        if (thrusting)
        {
            currentHeat = Mathf.Min(maxHeat, currentHeat + heatBuildRate * Time.deltaTime);
        }
        else
        {
            currentHeat = Mathf.Max(0f, currentHeat - heatDecayRate * Time.deltaTime);
        }

        HandleFreezeState();
        HandleHeatState();
        BlendStateMultipliers();

        //if (currentHeat >= maxHeat)
        //{
        //    //OVERDRIVE MODE TRIGGERED
        //}
    }



    public float GetHeatNorm()
    {
        return Mathf.Clamp01(currentHeat / maxHeat);
    }



    public void HandleFreezeState()
    {
        if (currentHeat <= 0f)
        {
            freezeMeter += freezeBuildRate * Time.deltaTime;
            freezeMeter = Mathf.Clamp01(freezeMeter);

            if (!isFrozen && freezeMeter >= 1f)
            {
                isFrozen = true;
                targetThrustMultiplier = freezeThrust;
                targetVelMultiplier = freezeMaxVel;
                targetTorqueMultiplier = freezeTorque;
            }
        }
        else
        {
            freezeMeter -= freezeDecayRate * Time.deltaTime;
            freezeMeter = Mathf.Clamp01(freezeMeter);

            if (isFrozen && freezeMeter <= 0f)
            {
                isFrozen = false;
                OnHeatStateChanged(currentHeatState);
            }
        }
    }

    public void HandleHeatState()
    {
        if (isFrozen) 
            return;

        float heatNorm = GetHeatNorm();
        HeatState newState;

        if (heatNorm < 0.2f)
        {
            newState = HeatState.Cool;
        }
        else if (heatNorm < 0.5f)
        {
            newState = HeatState.Warm;
        }
        else if (heatNorm < 0.9f)
        {
            newState = HeatState.Hot;
        }
        else
        {
            newState = HeatState.Critical;
        }

        if (newState != currentHeatState)
        {
            currentHeatState = newState;
            OnHeatStateChanged(newState);
        }
    }

    public void OnHeatStateChanged(HeatState newState)
    {
        switch (newState)
        {
            case HeatState.Cool:
                targetThrustMultiplier = coolThrust;
                targetVelMultiplier = coolMaxVel;
                targetTorqueMultiplier = coolTorque;
                break;

            case HeatState.Warm:
                targetThrustMultiplier = warmThrust;
                targetVelMultiplier = warmMaxVel;
                targetTorqueMultiplier = warmTorque;
                break;

            case HeatState.Hot:
                targetThrustMultiplier = hotThrust;
                targetVelMultiplier = hotMaxVel;
                targetTorqueMultiplier = hotTorque;
                break;

            case HeatState.Critical:
                targetThrustMultiplier = criticalThrust;
                targetVelMultiplier = criticalMaxVel;
                targetTorqueMultiplier = criticalTorque;
                break;
        }
    }

    public void BlendStateMultipliers()
    {
        thrustMultiplier = Mathf.Lerp(thrustMultiplier, targetThrustMultiplier, Time.deltaTime * stateBlendDuration);
        velMultiplier = Mathf.Lerp(velMultiplier, targetVelMultiplier, Time.deltaTime * stateBlendDuration);
        torqueMultiplier = Mathf.Lerp(torqueMultiplier, targetTorqueMultiplier, Time.deltaTime * stateBlendDuration);
    }



    public float GetVelocityNorm()
    {
        float playerVelocity = playerShipRB.linearVelocity.magnitude;
        float velocityNorm = Mathf.Clamp01(playerVelocity / baseMaxVel);
        return velocityNorm;
    }

    void HandleSquashStretch()
    {
        float velocityNorm = GetVelocityNorm();
        float heatNorm = GetHeatNorm();
        float heatStretchMultiplier = Mathf.Lerp(freezeMaxVel, criticalMaxVel, heatNorm);

        bool thrusting = playerMovementInput.y > 0.1f;

        Vector3 targetVelocityScale;

        if (thrusting)
        {
            float squashX = 1f - (velocityStretchX * velocityNorm * heatStretchMultiplier);
            float stretchY = 1f + (velocityStretchY * velocityNorm * heatStretchMultiplier);

            targetVelocityScale = new Vector3(shipBaseScale.x * squashX, shipBaseScale.y * stretchY, shipBaseScale.z);

            transform.localScale = Vector3.SmoothDamp(transform.localScale, targetVelocityScale, ref squashScaleVelocity, 1f / squashStretchDuration);
            //transform.localScale = Vector3.Lerp(transform.localScale, targetVelocityScale, Time.deltaTime * squashStretchDuration);
        }
        else
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, shipBaseScale, ref squashScaleVelocity, 1f / squashStretchReturnSpeed);
            //transform.localScale = Vector3.Lerp(transform.localScale, shipBaseScale, Time.deltaTime * squashStretchReturnSpeed);
        }
    }




    private void HandleDiscarge()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DischargeHeat();
        }
    }

    void DischargeHeat()
    {
        if (!dischargeController.CanDischarge())
            return;

        float heatNorm = GetHeatNorm();
        Vector2 shipDirection = transform.up.normalized;

        //get discharge output results
        var dischargeOutputs = dischargeController.DischargeHeatResults(currentHeat, maxHeat, shipDirection, heatNorm);

        //cancel velocity before recoil
        float cancelVelocityFactor = Mathf.Lerp(0.05f, 0.55f, heatNorm);
        playerShipRB.linearVelocity *= (1f - cancelVelocityFactor);

        // Apply recoil force
        Vector2 recoilDirection = -shipDirection;
        playerShipRB.AddForce(recoilDirection * dischargeOutputs.recoilAmount, ForceMode2D.Impulse);
        // Flip ship instantly
        transform.up = recoilDirection;

        DischargeSquash();

        // Reduce heat
        currentHeat = Mathf.Max(0, currentHeat - dischargeOutputs.heatSpent);
    }

    private void DischargeSquash()
    {
        transform.localScale = new Vector3(shipBaseScale.x * (1f + squashAmount), shipBaseScale.y * (1f - squashAmount), shipBaseScale.z);
    }

    private void HandleVFX()
    {
        if (!thrustFX)
            return;

        bool thrusting = playerMovementInput.y > 0.1f;
        float heatNorm = GetHeatNorm();

        var emission = thrustFX.emission;
        float targetEmissionRate = Mathf.Lerp(0f, 60f, Mathf.Clamp01(playerMovementInput.y) + (heatNorm * 40f));
        emission.rateOverTime = targetEmissionRate;

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



    public void TakeDamage(float damage)
    {
        //AddHeat(damage);
        //Explode();
    }




    public void ShieldDamage()
    {

        UpdateImmuneVisual();

        currentShields--;

        if (currentShields > 0)
        {
            TriggerShieldHit();
            StartCoroutine(RechargeShieldRoutine());
        }
        else
        {
            TriggerShieldBreak();
            StartCoroutine(RechargeShieldRoutine());
        }
    }

    private void HandleShieldVFX()
    {
        if (!shieldVisual)
            return;

        shieldVisual.enabled = currentShields > 0;

        float shieldTargetScale = shieldBaseScale;
        if (currentShields == 1)
        {
            shieldTargetScale *= shieldWeakScale;
        }

        shieldVisual.transform.localScale = Vector3.Lerp(shieldVisual.transform.localScale, Vector3.one * shieldTargetScale, Time.deltaTime * squashStretchReturnSpeed);
    }

    public void TriggerShieldHit()
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.collisionSFX, AudioManagerController.Instance.normalCollisionVolume);
        Instantiate(collsionFX, transform.position, Quaternion.identity);
        shieldVisual.transform.localScale = Vector3.one * shieldHitScale;
        cameraShake.StartSceenShake(screenShakeCollisionMultiplier);
    }

    private void TriggerShieldBreak()
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.collisionSFX, AudioManagerController.Instance.normalCollisionVolume);
        Instantiate(shieldShatterFX, transform.position, Quaternion.identity);

        shieldVisual.transform.localScale = Vector3.one * (shieldHitScale * 1.5f);

        cameraShake.StartSceenShake(screenShakeDamageMultiplier);
        StartCoroutine(screenFlash.FlashRoutine());
    }

    IEnumerator RechargeShieldRoutine()
    {
        if (isRecharging) 
            yield break;

        isRecharging = true;

        yield return new WaitForSeconds(shieldRechargeDelay);

        while (currentShields < maxShields)
        {
            yield return new WaitForSeconds(shieldRechargeRate);
            currentShields++;

            //shieldVisual.transform.localScale = Vector3.one * shieldHitScale;
        }

        isRecharging = false;
    }

    private void UpdateImmuneVisual()
    {
        if (!shipRenderer)
            return;

        ShieldController shield = FindAnyObjectByType<ShieldController>();
        bool isimmune = shield.isImmune;

        if (isimmune)
        {
            Color immunityColour = Color.red;
            shipRenderer.color = immunityColour;
        }
        else
        {
            shipRenderer.color = Color.white;
        }
    }




    public void AddHeat(float amount)
    {
        currentHeat = Mathf.Clamp(currentHeat + amount, 0f, maxHeat);
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
}
