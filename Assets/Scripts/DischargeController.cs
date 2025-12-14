using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static UnityEngine.UI.Image;

public class DischargeController : MonoBehaviour
{
    [Header("References")]         
    public SpaceshipController playerShip; //reference to player ship   
    public ParticleSystem dischargeVFX; //discharge visual effect

    [Header("Discharge Shape Settings")]
    public float minLength = 1.4f; //minimum length of discharge
    public float maxLength = 4.5f; //maximum length of discharge
    public float dischargeWidth = 0.8f; //width of discharge

    [Header("Damage Settings")]
    public int minDamage = 1; //minimum damage of discharge
    public int maxDamage = 4; //maximum damage of discharge

    [Header("Heat Cost Settings")]
    public float minHeatCost = 0.2f; //minimum heat cost of discharge
    public float maxHeatCost = 0.6f; //maximum heat cost of discharge

    public void TryDischarge()
    {
        if (!playerShip)
            return;
        if (playerShip.currentState == SpaceshipController.ThermalState.Critical)
            return;

        ExecuteDischarge();
    }

    void ExecuteDischarge()
    {
        float thermalNorm = playerShip.GetThermalNorm();

        float dischargeLength = Mathf.Lerp(minLength, maxLength, thermalNorm);
        int damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, thermalNorm));
        float heatCost = Mathf.Lerp(minHeatCost, maxHeatCost, thermalNorm);

        Vector2 shipDirection = playerShip.transform.up.normalized;
        Vector2 dischargeCentre = (Vector2)transform.position + shipDirection * (dischargeLength * 0.5f);
        Vector2 dischargeSize = new Vector2(dischargeWidth, dischargeLength);
        float dischargeAngle = playerShip.transform.eulerAngles.z;
        
        var asteroidsHit = Physics2D.OverlapCapsuleAll(dischargeCentre, dischargeSize, CapsuleDirection2D.Vertical, dischargeAngle);

        foreach (var asteroid in asteroidsHit)
        {
            if (!asteroid.CompareTag("Asteroid"))
                continue;

            asteroid.GetComponent<AsteroidController>()?.TakeDamage(damage);
        }

        PlayDischargeFX(dischargeCentre, shipDirection, thermalNorm);

        playerShip.ApplyThermal(-heatCost);
    }

    public void PlayDischargeFX(Vector3 dischargeCentre, Vector2 shipDirection, float thermalNorm)
    {
        if (!dischargeVFX)
            return;

        var dischargeFX = Instantiate(dischargeVFX, dischargeCentre, transform.rotation);
        dischargeFX.transform.up = -shipDirection;

        float heatScale = Mathf.Lerp(0.2f, 0.5f, thermalNorm);
        dischargeFX.transform.localScale *= heatScale;
        AudioManagerController.audioManagerInstance.sfxSource.PlayOneShot(AudioManagerController.audioManagerInstance.dischargeSFX, AudioManagerController.audioManagerInstance.sfxVolume);
    }

    void OnDrawGizmosSelected()
    {
        if (!playerShip)
            return;

        float thermalNorm = playerShip.GetThermalNorm();

        float dischargeLength = Mathf.Lerp(minLength, maxLength, thermalNorm);
        float dischargeSize = dischargeWidth;
        Vector2 dischargeCentre = (Vector2)transform.position + (Vector2)transform.up * (dischargeLength * 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(dischargeCentre, Quaternion.Euler(0f, 0f, transform.eulerAngles.z), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(dischargeSize, dischargeLength, 0f));
    }
}
