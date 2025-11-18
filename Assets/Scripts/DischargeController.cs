using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static UnityEngine.UI.Image;

public class DischargeController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem heatwaveVFX;            // Tiny S1 pop
    public ParticleSystem dischargeVFX;            // S4 beam
    public CameraController cameraShake;               // Your existing shake script

    [Header("Core Settings")]
    public float baseDischargeRecoil = 1f;
    public float maxDischargeRecoil = 10f;                   // tiny scaling at high heat
    public float baseDischargeRange = 1f;
    public float maxDischargeRange = 10f;                   // line pulse gets long at high heat
    public float heatwaveRadius = 0.5f;              // tiny pop
    public float dischargeCooldown = 0.25f;        // prevents spam
    private float dischargeCooldownTimer;


    public struct DischargeResult
    {
        public float recoilAmount;
        public float heatSpent;
    }

    void Start()
    {
        if (!cameraShake)
        {
            cameraShake = Object.FindAnyObjectByType<CameraController>();
        }    
    }

    void Update()
    {
    if (dischargeCooldownTimer > 0)
        {
            dischargeCooldownTimer -= Time.deltaTime;
        }
    }

    public bool CanDischarge()
    {
        return dischargeCooldownTimer <= 0;
    }


    //DISCHARGE SCRIPT
    public DischargeResult DischargeHeatResults(float currentHeat, float maxHeat, Vector2 shipDirection, float heatNorm)
    {
        dischargeCooldownTimer = dischargeCooldown;

        float dischargeRange = Mathf.Lerp(baseDischargeRange, maxDischargeRange, heatNorm);

        var playerShipInstance = SpaceshipController.playerShipInstance;
        Vector2 dischargeOriginPos = (Vector2)playerShipInstance.transform.position - shipDirection * playerShipInstance.dischargeOffset;
        Vector2 directionToDischarge = shipDirection.normalized; // opposite of ship facing

        DischargeFX(dischargeOriginPos, heatNorm, shipDirection);

        int mask = LayerMask.GetMask("Debris"); // adjust to match your layers
        RaycastHit2D[] hits = Physics2D.RaycastAll(dischargeOriginPos, directionToDischarge, dischargeRange, mask);

        foreach (var hit in hits)
        {
            AsteroidController asteroid = hit.collider.GetComponent<AsteroidController>();
            if (asteroid)
            {
                float dischargeDamage = Mathf.Lerp(0.1f, 65f, Mathf.Pow(heatNorm, 1.3f));
                asteroid.TakeDamage(dischargeDamage);
            }
        }

        float scaledRadius = Mathf.Lerp(0.5f, 3f, heatNorm);
        Collider2D[] heatwaveHits = Physics2D.OverlapCircleAll(dischargeOriginPos, scaledRadius);
        foreach (var hit in heatwaveHits)
        {
            AsteroidController asteroid = hit.GetComponent<AsteroidController>();
            if (asteroid)
            {
                asteroid.TakeDamage(0.2f + heatNorm * 0.4f);
            }
        }

        float recoilAmount = Mathf.Lerp(baseDischargeRecoil, maxDischargeRecoil, heatNorm);
        float heatSpent = Mathf.Lerp(2f, 25f, Mathf.Pow(heatNorm, 1.2f));

        float hitstopDuration = Mathf.Lerp(0.015f, 0.065f, heatNorm);
        StartCoroutine(Hitstop(hitstopDuration));

        return new DischargeResult
        {
            recoilAmount = recoilAmount,
            heatSpent = heatSpent
        };
    }

    public void DischargeFX(Vector3 dischargeOriginPos, float heatNorm, Vector2 shipDirection)
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.bulletSFX, AudioManagerController.Instance.normalCollisionVolume);

        if (heatwaveVFX)
        {
            var heatwave = Instantiate(heatwaveVFX, dischargeOriginPos, transform.rotation);
            float heatScale = 1f + heatNorm;
            heatwave.transform.localScale *= heatScale;
        }

        if (dischargeVFX)
        {
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, -shipDirection);
            var discharge = Instantiate(dischargeVFX, dischargeOriginPos, transform.rotation);
            float heatScale = 1f + heatNorm;
            discharge.transform.localScale *= heatScale;
        }

        if (cameraShake)
        {
            cameraShake.StartSceenShake(0.1f + heatNorm * 0.4f);
        }
    }

    IEnumerator Hitstop(float duration)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    void OnDrawGizmos()
    {
        if (!SpaceshipController.playerShipInstance) 
            return;

        float heatNorm = SpaceshipController.playerShipInstance.GetHeatNorm();

        float scaledRadius = Mathf.Lerp(0.5f, 3f, heatNorm);
        Gizmos.color = Color.white; //gravity capture zone
        Gizmos.DrawWireSphere(transform.position, scaledRadius);

        float scaledRange = Mathf.Lerp(baseDischargeRange, maxDischargeRange, heatNorm);
        Gizmos.color = Color.red;
        var playerShipInstance = SpaceshipController.playerShipInstance;
        Vector2 directionToRaycast = SpaceshipController.playerShipInstance.transform.up;
        Vector2 raycastOriginPos = (Vector2)playerShipInstance.transform.position - directionToRaycast * playerShipInstance.dischargeOffset;
        Gizmos.DrawLine(raycastOriginPos, raycastOriginPos + directionToRaycast * scaledRange);
    }
}
