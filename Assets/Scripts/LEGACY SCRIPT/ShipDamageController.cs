using UnityEngine;

public class ShipDamageController : MonoBehaviour
{
    [Header("Reference Settings")]
    public SpaceshipController playership; //reference to player ship
    public ShieldController shield; //reference to shield controller
     
    [Header("Collision Settings")]
    private float lastCollisionTime; //time of last collision
    public float collisionCooldown = 0.15f; //cooldown time between collisions

    [Header("Damage Settings")]
    public float heatVentAmount = 8f; //amount of thermal vented per hit

    void Start()
    {
        playership = SpaceshipController.playerInstance;
        shield = Object.FindAnyObjectByType<ShieldController>();
    }

    public void ApplyDirectDamage(float damage)
    {
        if (Time.time - lastCollisionTime < collisionCooldown) 
            return;
        lastCollisionTime = Time.time;

        if (shield && shield.Active())
        {
            shield.TakeHit();
            return;
        }

        if (playership)
        {
            float thermalVent = damage * heatVentAmount;
            playership.ApplyThermal(thermalVent);
        }
    }
}
