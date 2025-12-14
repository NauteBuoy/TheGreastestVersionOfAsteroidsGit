using System.Collections;
using System.Drawing;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class SpaceshipController : MonoBehaviour
{
    public enum ThermalState
    {
        Frozen3, Frozen2, Frozen1, Neutral, Hot1, Hot2, Hot3, Critical
    }

    [Header("Reference Settings")]
    public static SpaceshipController playerInstance; //reference to THIS player ship
    public Transform shipVisual; //reference to player ship sprite transform
    private Rigidbody2D playerShipRB; //reference to player ship rigidbody
    public ShipVFXController shipVFX; // ship VFX controller
    public ScreenFlashController screenFlash; // screen flash controller
    public CameraController cameraController; // camera shake controller
    public DischargeController dischargeController; //discharge controller

    [Header("Movement Settings")]
    public float thrustForce = 1200f; //force applied to ship on thrust
    public float rotationTorque = 100f; //torque applied for rotation
    public float maxVelocity = 6f; //maximum speed threshold
    private Vector2 playerInput; //player movement input vector
    private float baseMaxVel; //base max velocity
    private float baseThrust; //base thrust force

    [Header("Thermal Settings")]
    public ThermalState currentState; //current heat state
    [Range(-1f, 1f)] public float thermalValue = 0f; //current heat
    public float heatBuildRate = 0.2f; //amount of heat generated per second of thrust
    public float heatDecayRate = 0.4f; //amount of heat cooled per second
    public float freezeBuildRate = 0.4f; //amount of cold generated per second of no thrust
    public bool criticalStateLocked = false; //is critical state locked
    public float criticalCooldownRate = 0.2f; //cooldown rate when in critical state
    public float coldSnapThreshold = -0.99f; //thermal value for cold snap death
    bool isDead = false;

    [Header("Threshold Settings")]
    public float hot1Threshold = 0.30f; //thermal value for hot I state
    public float hot2Threshold = 0.60f;  //thermal value for hot II state
    public float hot3Threshold = 0.80f; //thermal value for hot III state
    public float criticalThreshold = 1f; //thermal value for heat death
    public float frozen1Threshold = -0.30f; //thermal value for frozen I state
    public float frozen2Threshold = -0.60f; //thermal value for frozen II state
    public float frozen3Threshold = -0.80f; //thermal value for frozen III state

    [Header("State Multiplier Settings")]
    public float neutralThrust = 1f;
    public float neutralMaxVel = 1f;
    public float neutralTorque = 1f;

    [Header("Hot I Settings")]
    public float hot1Thrust = 1.15f;
    public float hot1MaxVel = 1.15f;
    public float hot1Torque = 1.05f;

    [Header("Hot II Settings")]
    public float hot2Thrust = 1.3f;
    public float hot2MaxVel = 1.3f;
    public float hot2Torque = 1.1f;

    [Header("Hot III Settings")]
    public float hot3Thrust = 1.6f;
    public float hot3MaxVel = 1.6f;
    public float hot3Torque = 1.2f;

    [Header("CRITICAL Settings")]
    public float criticalThrust = 3.2f;
    public float criticalMaxVel = 3.2f;
    public float criticalTorque = 1.4f;

    [Header("Frozen I Settings")]
    public float frozen1Thrust = 0.85f;
    public float frozen1MaxVel = 0.85f;
    public float frozen1Torque = 0.9f;

    [Header("Frozen II Settings")]
    public float frozen2Thrust = 0.6f;
    public float frozen2MaxVel = 0.6f;
    public float frozen2Torque = 0.7f;

    [Header("Frozen III Settings")]
    public float frozen3Thrust = 0.4f;
    public float frozen3MaxVel = 0.4f;
    public float frozen3Torque = 0.5f;

    [Header("State Blend Settings")]
    public float blendStateDuration = 6f; //duration for stat blending
    public float thrustMultiplier = 1f; //current thrust multiplier
    public float velocityMultiplier = 1f; //current velocity multiplier
    public float torqueMultiplier = 1f; //current torque multiplier
    public float targetThrustMultiplier = 1f; //target thrust multiplier
    public float targetVelocityMultiplier = 1f; //target velocity multiplier
    public float targetTorqueMultiplier = 1f; //target torque multiplier

    [Header("Squash and Stretch Settings")]
    public float velocityStretchX = 0.15f; //how much to squish X at top speed
    public float velocityStretchY = 0.25f; //how much to stretch Y at top speed
    public float squashStretchDuration = 6f; //how fast ship squashes/stretches
    public float squashReturnDuration = 12f;  //how fast ship returns to base scale
    private Vector3 baseScale; //original scale of the ship
    private Vector3 squashVelocity; //velocity reference for SmoothDamp

    [Header("Flip")] // chnage to discharge later
    public float dischargeImpulse = 2f; //impulse applied on flip
    public float dischargeStateLockTime = 0.12f; //time spent locked in discharge state
    public Transform dischargeIndicator; //discharge indicator transform
    bool discharging; //is the ship currently discharging
    float dischargeTimer; //timer for discharge state
    public float squashAmount = 0.6f; //how much does ship squash/stretch

    void Awake()
    {
        if (playerInstance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            playerInstance = this;
        }
    }

    void Start()
    {
        playerShipRB = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        baseMaxVel = maxVelocity;
        baseThrust = thrustForce;
        currentState = CheckThermalState(thermalValue);
        SetNewStateMultipliers(currentState);
        thrustMultiplier = targetThrustMultiplier;
        velocityMultiplier = targetVelocityMultiplier;
        torqueMultiplier = targetTorqueMultiplier;
        shipVFX = ShipVFXController.shipVFXInstance;
    }

    void Update()
    {
        HandleInput();
        HandleThermalValue();
        HandleDeathCheck();
        HandleThermalState();
        HandleBlendStateMultipliers();
        HandleSquashStretch();
        HandleDischargeIndicatorPosition();
        HandleThrusterVFX();
    }

    void FixedUpdate()
    {
        HandleFlipTimer();
        HandleMovement();
    }

    void LateUpdate()
    {
        HandleRotationDelay();
    }

    void HandleInput()
    {
        playerInput.x = Input.GetAxisRaw("Horizontal"); 
        playerInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryDischarge();
        }
    }

    void HandleMovement()
    {
        ApplyThrust(playerInput.y);
        ApplyTorque(playerInput.x);
        ClampVelocity();
    }

    void ApplyThrust(float playerInput)
    {
        if (discharging)
            return;

        float forwardOnly = Mathf.Max(0f, playerInput);
        float thrust = baseThrust * thrustMultiplier;
        playerShipRB.AddForce(transform.up * forwardOnly * thrust * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    void ApplyTorque(float playerInput)
    {
        float torque = rotationTorque * torqueMultiplier;
        playerShipRB.AddTorque(-playerInput * torque * Time.fixedDeltaTime);
    }

    void ClampVelocity()
    {
        float maxVelocity = baseMaxVel * velocityMultiplier;
        float playerVelocity = playerShipRB.linearVelocity.magnitude;
        if (playerVelocity > maxVelocity)
        {
            playerShipRB.linearVelocity = playerShipRB.linearVelocity.normalized * maxVelocity;
        }
    }

    void HandleRotationDelay()
    {
        if (!shipVisual)
            return;

        Quaternion targetRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z);
        shipVisual.rotation = Quaternion.Slerp(shipVisual.rotation, targetRotation, Time.deltaTime * 10f);
    }

    void HandleThermalValue()
    {
        if (currentState == ThermalState.Critical)
        {
            criticalStateLocked = true;
            thermalValue = Mathf.MoveTowards(thermalValue, 0f, criticalCooldownRate * Time.deltaTime);
            if (thermalValue <= 0f)
            {
                criticalStateLocked = false;
            }
            return;
        }

        bool thrusting = playerInput.y > 0.1f;
        if (thrusting)
        {
            thermalValue = Mathf.MoveTowards(thermalValue, 1f, heatBuildRate * Time.deltaTime);
        }
        else
        {
            if (thermalValue > 0f)
            {
                thermalValue = Mathf.MoveTowards(thermalValue, 0f, heatDecayRate * Time.deltaTime);
            }
            else
            {
                thermalValue = Mathf.MoveTowards(thermalValue, -1f, freezeBuildRate * Time.deltaTime);
            }
        }
        thermalValue = Mathf.Clamp(thermalValue, -1f, 1f);
    }

    void HandleDeathCheck()
    {
        if (thermalValue <= coldSnapThreshold)
        {
            KillPlayer();
        }
    }

    ThermalState CheckThermalState(float thermalValue)
    {
        if (criticalStateLocked)
            return ThermalState.Critical;

        if (thermalValue >= criticalThreshold) 
            return ThermalState.Critical;
        if (thermalValue >= hot3Threshold) 
            return ThermalState.Hot3;
        if (thermalValue >= hot2Threshold) 
            return ThermalState.Hot2;
        if (thermalValue >= hot1Threshold) 
            return ThermalState.Hot1;

        if (thermalValue <= frozen3Threshold) 
            return ThermalState.Frozen3;
        if (thermalValue <= frozen2Threshold) 
            return ThermalState.Frozen2;
        if (thermalValue <= frozen1Threshold) 
            return ThermalState.Frozen1;

        return ThermalState.Neutral;
    }

    void HandleThermalState()
    {
        ThermalState newState = CheckThermalState(thermalValue);
        if (newState == currentState)
            return;

        currentState = newState;
        SetNewStateMultipliers(currentState);
        AudioManagerController.audioManagerInstance.PlayStateChange();

        if (shipVFX)
        {
            shipVFX.UpdateThermalStateChange(currentState, GetThermalNorm());
        }
    }

    void SetNewStateMultipliers(ThermalState newState)
    {
        switch (newState)
        {
            case ThermalState.Neutral:
                SetStateMultipliers(neutralThrust, neutralMaxVel, neutralTorque);
                break;
            case ThermalState.Hot1:
                SetStateMultipliers(hot1Thrust, hot1MaxVel, hot1Torque);
                break;
            case ThermalState.Hot2:
                SetStateMultipliers(hot2Thrust, hot2MaxVel, hot2Torque);
                break;
            case ThermalState.Hot3:
                SetStateMultipliers(hot3Thrust, hot3MaxVel, hot3Torque);
                break;
            case ThermalState.Critical:
                SetStateMultipliers(criticalThrust, criticalMaxVel, criticalTorque);
                break;
            case ThermalState.Frozen1:
                SetStateMultipliers(frozen1Thrust, frozen1MaxVel, frozen1Torque);
                break;
            case ThermalState.Frozen2:
                SetStateMultipliers(frozen2Thrust, frozen2MaxVel, frozen2Torque);
                break;
            case ThermalState.Frozen3:
                SetStateMultipliers(frozen3Thrust, frozen3MaxVel, frozen3Torque);
                break;
        }
    }

    void SetStateMultipliers(float newStateThrust, float newStateVelocity, float newStateTorque)
    {
        targetThrustMultiplier = newStateThrust;
        targetVelocityMultiplier = newStateVelocity;
        targetTorqueMultiplier = newStateTorque;
    }

    void HandleBlendStateMultipliers()
    {
        thrustMultiplier = Mathf.Lerp(thrustMultiplier, targetThrustMultiplier, Time.deltaTime * blendStateDuration); 
        velocityMultiplier = Mathf.Lerp(velocityMultiplier, targetVelocityMultiplier, Time.deltaTime * blendStateDuration);
        torqueMultiplier = Mathf.Lerp(torqueMultiplier, targetTorqueMultiplier, Time.deltaTime * blendStateDuration);
    }

    void HandleSquashStretch()
    {
        if (!shipVisual) 
            return;

        float velocityNorm = GetVelocityNorm();
        bool thrusting = playerInput.y > 0.1f;
        Vector3 targetScale = baseScale;

        if (thrusting && velocityNorm > 0.01f)
        {
            float thermalNorm = GetThermalNorm();
            float heatStretch = Mathf.Lerp(frozen3MaxVel, criticalMaxVel, thermalNorm);
            float squashX = 1f - (velocityStretchX * velocityNorm * heatStretch);
            float stretchY = 1f + (velocityStretchY * velocityNorm * heatStretch);
            targetScale = new Vector3(baseScale.x * squashX, baseScale.y * stretchY, baseScale.z);
            shipVisual.localScale = Vector3.SmoothDamp(shipVisual.localScale, targetScale, ref squashVelocity, 1f / squashStretchDuration);
        }
        else
        {
            shipVisual.localScale = Vector3.SmoothDamp(shipVisual.localScale, baseScale, ref squashVelocity, 1f / squashReturnDuration);
        }
    }

    public float GetThermalNorm()
    {
        float thermalNorm = Mathf.InverseLerp(-1f, 1f, thermalValue);
        return thermalNorm;
    }

    public float GetVelocityNorm()
    {
        float playerVelocity = playerShipRB.linearVelocity.magnitude;
        float velocityNorm = Mathf.Clamp01(playerVelocity / baseMaxVel);
        return velocityNorm;
    }

    public void ApplyThermal(float amount)
    {
        thermalValue = Mathf.Clamp(thermalValue + amount, -1f, 1f);          
    }

    public bool canPierce()
    {
        return currentState == ThermalState.Critical;
    }

    void TryDischarge()
    {
        if (discharging)
            return;

        if (dischargeController)
        {
            dischargeController.TryDischarge();
        }

        discharging = true;
        dischargeTimer = dischargeStateLockTime;

        Vector2 shipDirection = transform.up.normalized;
        Vector2 flipDirection = -shipDirection;

        float thermalNorm = GetThermalNorm();
        float cancelVelocity = Mathf.Lerp(0.15f, 0.55f, thermalNorm);
        playerShipRB.linearVelocity *= (1f - cancelVelocity);
        dischargeImpulse = Mathf.Lerp(2f, 5f, thermalNorm);
        playerShipRB.rotation += 180f;
        playerShipRB.AddForce(flipDirection * dischargeImpulse, ForceMode2D.Impulse);
        DischargeSquash();
        cameraController.StartSceenShake(cancelVelocity);

    }

    private void DischargeSquash()
    {
        shipVisual.localScale = new Vector3(baseScale.x * (1f + squashAmount), baseScale.y * (1f - squashAmount), baseScale.z);

    }

    void HandleFlipTimer()
    {
        if (!discharging)
            return;

        dischargeTimer -= Time.fixedDeltaTime;
        if (dischargeTimer <= 0f)
        {
            discharging = false;
            shipVisual.localScale = Vector3.SmoothDamp(shipVisual.localScale, baseScale, ref squashVelocity, 1f / squashReturnDuration);
        }
    }
    public void HandleDischargeIndicatorPosition()
    {
        if (!dischargeIndicator)
            return;
        if (!dischargeController)
            return;

        float thermalNorm = GetThermalNorm();
        float minDischargeLength = dischargeController.minLength;
        float maxDischargeLength = dischargeController.maxLength ;
        float dischargeLength = Mathf.Lerp(minDischargeLength, maxDischargeLength, thermalNorm);

        dischargeIndicator.localPosition = Vector3.up * dischargeLength;
        dischargeIndicator.localRotation = Quaternion.identity;
    }

    private void HandleThrusterVFX()
    {
        bool isThrusting = playerInput.y > 0.1f;
        float thermalNorm = GetThermalNorm();

        AudioManagerController.audioManagerInstance.PlayThruster(isThrusting);
        shipVFX.UpdateThruster(isThrusting, thermalNorm);
    }

    public void KillPlayer()
    {
        if (isDead)
            return;

        isDead = true;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (screenFlash)
        {
            screenFlash.HideFlash();
        }
        if (AudioManagerController.audioManagerInstance)
        {
            AudioManagerController.audioManagerInstance.PlaySFX(AudioManagerController.audioManagerInstance.deathSFX, AudioManagerController.audioManagerInstance.sfxVolumeLoud);
        }
        if (shipVFX)
        {
            shipVFX.PlayDeathExplosion(transform.position);
        }
        yield return new WaitForSecondsRealtime(0.4f);

        if (GameManagerController.gameManagerInstance)
        {
            yield return StartCoroutine(GameManagerController.gameManagerInstance.GameOverRoutine());
        }
        Destroy(gameObject);
    }
}
