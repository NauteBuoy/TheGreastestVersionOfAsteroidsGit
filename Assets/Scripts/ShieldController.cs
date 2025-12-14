using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Pool;
using static SpaceshipController;
using static UnityEngine.GraphicsBuffer;

public class ShieldController : MonoBehaviour
{
    [Header("Reference Settings")]
    public SpaceshipController playerShip; // reference to player ship
    public SpriteRenderer shieldVisual; // reference to shield visual
    public CircleCollider2D shieldCollider; // reference to shield collider
    public ShieldUIController shieldUI; // reference to shield UI

    [Header("Shield Settings")]
    public int maxShieldCharges = 2; // maximum shield charges  
    public int currentShieldCharges = 2; // current shield charges  
    public float rechargeDelay = 3f; // delay before shield starts recharging
    public float rechargeRate = 1.5f; // time between each shield charge recharge
    public bool recharging = false; // is the shield currently recharging

    [Header("Immunity Settings")]
    public float shieldHeatVent = 0.2f; // amount of thermal vented on hit
    public float immunityDuration = 0.2f; // duration of immunity after taking a hit
    public bool immune = false; // is the player currently immune

    [Header("Follow Settings")]
    public Transform followTransform; // transform to follow
    public float smoothFollowDuration = 0.03f; // smoothing duration for following
    private Vector3 followVel; // velocity reference for SmoothDamp

    [Header("VFX Settings")]
    public GameObject shieldShatterFX; // shield shatter effect prefab

    [Header("Scale Settings")]
    public float baseScale = 1.5f; // base scale of the shield
    public float weakenedScale = 1f; // scale when shield is weak
    public float hitScale = 1.4f; // scale when shield is hit
    public float breakScale = 1.8f; // scale when shield breaks
    public float scaleSpeed = 8f; // speed of scale lerping
    private Vector3 targetScale; // target scale for lerping
    private bool shieldBroken = false; // has the shield been broken

    void Start()
    { 
        playerShip = SpaceshipController.playerInstance;
        shieldVisual = GetComponentInChildren<SpriteRenderer>();
        shieldCollider = GetComponent<CircleCollider2D>();
        shieldUI = Object.FindAnyObjectByType<ShieldUIController>();
        followTransform = playerShip.transform;
        currentShieldCharges = Mathf.Clamp(currentShieldCharges, 0, maxShieldCharges);
        targetScale = Vector3.one * baseScale;
    }

    void Update()
    {
        HandleFollow();
        HandleVisual();
    }
    
    void HandleFollow()
    {
        if (!followTransform)
            return;
        Vector3 targetPos = followTransform.position;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVel, smoothFollowDuration);
    }

    void HandleVisual()
    {
        if (!shieldVisual)
            return;

        bool active = Active();
        shieldVisual.enabled = active;
        shieldCollider.enabled = active;

        float scale = baseScale;
        if (currentShieldCharges == 1)
        {
            scale *= weakenedScale;
        }

        targetScale = Vector3.Lerp(targetScale, Vector3.one * scale, Time.deltaTime * scaleSpeed);
        shieldVisual.transform.localScale = targetScale;

        if (shieldUI)
        {
            shieldUI.SetShieldCount(currentShieldCharges);
        }
    }

    public bool Active()
    {
        return currentShieldCharges > 0;
    }

    public void TakeHit()
    {
        if (playerShip.currentState == ThermalState.Critical)
            return;
        if (immune)
            return;
        if (!Active())
            return;

        currentShieldCharges = Mathf.Max(0, currentShieldCharges - 1);

        if (playerShip)
        {
            playerShip.ApplyThermal(-shieldHeatVent);
        }
            
        if (currentShieldCharges > 0)
        {
            TriggerShieldHit();
        }
        else
        {
            TriggerShieldBreak();
        }

        if (!recharging)
        {
            StartCoroutine(RechargeRoutine());
        }
        StartCoroutine(ImmunityRoutine());
    }

    IEnumerator ImmunityRoutine()
    {
        immune = true;
        yield return new WaitForSeconds(immunityDuration);
        immune = false;
    }

    IEnumerator RechargeRoutine()
    {
        if (recharging)
            yield break;
        recharging = true;

        yield return new WaitForSeconds(rechargeDelay);

        while (currentShieldCharges < maxShieldCharges)
        {
            currentShieldCharges++;
            if (shieldUI)
            {
                shieldUI.SetShieldCount(currentShieldCharges);
            }
            yield return new WaitForSeconds(rechargeRate);
        }

        shieldBroken = false;
        recharging = false;
    }

    public void TriggerShieldHit()
    {
        targetScale = Vector3.one * hitScale;
        AudioManagerController.audioManagerInstance.PlaySFX(AudioManagerController.audioManagerInstance.shieldHitSFX, AudioManagerController.audioManagerInstance.sfxVolume);

        if (playerShip.cameraController)
        {
            playerShip.cameraController.StartSceenShake(0.8f);
        }
        if (ShipVFXController.shipVFXInstance)
        {
            ShipVFXController.shipVFXInstance.PlayHitstop(playerShip.GetThermalNorm());
        }

    }

    private void TriggerShieldBreak()
    {
        if (shieldBroken)
            return;

        shieldBroken = true;
        targetScale = Vector3.one * hitScale;
        Instantiate(shieldShatterFX, transform.position, Quaternion.identity);
        AudioManagerController.audioManagerInstance.PlaySFX(AudioManagerController.audioManagerInstance.shieldBreakSFX, AudioManagerController.audioManagerInstance.sfxVolumeLoud);

        if (playerShip && playerShip.cameraController)
        {
            playerShip.cameraController.StartSceenShake(1f);
        }
        if (playerShip && playerShip.screenFlash)
        {
            StartCoroutine(playerShip.screenFlash.FlashRoutine());
        }
        if (ShipVFXController.shipVFXInstance)
        {
            ShipVFXController.shipVFXInstance.PlayHitstop(playerShip.GetThermalNorm());
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Asteroid"))
            return;
        if (immune)
            return;
        if (!Active())
            return;

        TakeHit();
    }
}