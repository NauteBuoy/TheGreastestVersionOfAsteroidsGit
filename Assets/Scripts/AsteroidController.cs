using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static SpaceshipController;
using static UnityEngine.GraphicsBuffer;

public class AsteroidController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int asteroidSpawnValue = 1; //value of asteroid for spawning purposes

    [Header("Reference Setings")]
    private ShieldController shield; //reference to shield controller
    public CameraController cameraShake; // camera shake controller

    [Header("Movement Settings")]
    public float rotationSkew = 3f; //initial velocity of asteroid
    private float rotationSpeed; //rotation speed of asteroid

    [Header("Hit Point Settings")]
    public int hitPointMax = 5; //maximum health of asteroid
    private int hitPointCurrent; //current health of asteroid

    [Header("Damage Settings")]
    public float collisionDamage = 1f; //damage dealt to player ship on collision
    public GameObject collisionFX; //collision effect prefab
    public float screenShakeMultiplier = 1f; // multiplier for screen shake intensity

    [Header("Collision Settings")]
    public float lastCollisionTime; //time of last collision
    public float collisionCooldown = 0.1f; //cooldown time between collisions

    [Header("Chunk Settings")]
    public GameObject[] asteroidChunks; //array of asteroid chunk prefabs
    public GameObject explosionFX; //explosion effect prefab
    public int chunkMin = 0; //minimum number of chunks to spawn
    public int chunkMax = 5; //maximum number of chunks to spawn
    public float explosionDistance = 0.5f; //maximum distance chunks can spawn from asteroid center
    public float explosionForce = 10f; //force applied to chunks on spawn

    [Header("Score Settings")]
    public int scoreValue = 10; //score value awarded to player on asteroid destruction

    void Start()
    {
        shield = Object.FindAnyObjectByType<ShieldController>();
        cameraShake = Object.FindAnyObjectByType<CameraController>();
        hitPointCurrent = hitPointMax;
        rotationSpeed = Random.Range(-rotationSkew, rotationSkew); 
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime); 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time - lastCollisionTime < collisionCooldown)
            return;
        lastCollisionTime = Time.time;

        var asteroid = collision.gameObject.GetComponent<AsteroidController>();
        if (asteroid)
        {
            SpawnCollisionAtPoint(collision);
            if (cameraShake)
            {
                cameraShake.StartSceenShake(screenShakeMultiplier * 0.4f);
            }
            AudioManagerController.audioManagerInstance.PlaySFX(AudioManagerController.audioManagerInstance.collisionSFX, AudioManagerController.audioManagerInstance.scoreTickVolume);
            return;
        }

        var playerShip = collision.gameObject.GetComponent<SpaceshipController>();
        if (playerShip)
        {
            if (playerShip.currentState == ThermalState.Critical)
            { 
                Explode();
                return;
            }

            if (shield && shield.Active())
            {
                shield.TakeHit();
                SpawnCollisionAtPoint(collision);

                var spaceshipImmunity = playerShip.GetComponent<ShipVFXController>();
                if (spaceshipImmunity)
                {
                    spaceshipImmunity.PlayImmunityFlash(shield.immunityDuration);
                }
            }
            else
            {
                if (playerShip.currentState != ThermalState.Critical)
                {
                    SpawnCollisionAtPoint(collision);
                    playerShip.KillPlayer();
                }
            }
        }
    }

    void SpawnCollisionAtPoint(Collision2D collision)
    {
        if (collision == null)
            return;
        if (collision.contactCount == 0) 
            return;

        Vector2 spawnPos = collision.GetContact(0).point;
        if (collisionFX)
        {
            Instantiate(collisionFX, spawnPos, Quaternion.identity);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPointCurrent -= damage;
        if (cameraShake)
        {
            cameraShake.StartSceenShake(screenShakeMultiplier * 0.5f);
        }
        if (hitPointCurrent <= 0)
        {
            cameraShake.StartSceenShake(screenShakeMultiplier * 1.2f);
            AudioManagerController.audioManagerInstance.PlaySFX(AudioManagerController.audioManagerInstance.explosionSFX, AudioManagerController.audioManagerInstance.sfxVolumeLoud);
            Explode();
        }
    }

    public void Explode()
    {
        var playerShip = SpaceshipController.playerInstance;
        var gameManagerInstance = GameManagerController.gameManagerInstance;

        if (gameManagerInstance)
        {
            float heatBonus = 1f + (playerShip.GetThermalNorm() * 2f);
            int finalScore = Mathf.RoundToInt(scoreValue * heatBonus);
            gameManagerInstance.AddScore(finalScore, transform.position);
        }

        int numChunks = Random.Range(chunkMin, chunkMax + 1);
        if (asteroidChunks != null && asteroidChunks.Length > 0)
        {
            for (int i = 0; i < numChunks; i++)
            {
                CreateAsteroidChunk();
            }
        }
        if (explosionFX)
        {
            Instantiate(explosionFX, transform.position, transform.rotation); 
        }
        Destroy(gameObject); 
    }

    private void CreateAsteroidChunk()
    {
        if (asteroidChunks == null)
            return;
        if (asteroidChunks.Length == 0)
            return;

        int randomIndex = Random.Range(0, asteroidChunks.Length);
        var asteroidChunk = asteroidChunks[randomIndex];

        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * explosionDistance;
        var chunk = Instantiate(asteroidChunk, spawnPos, transform.rotation);

        var chunckRB = chunk.GetComponent<Rigidbody2D>();
        if (chunckRB)
        {
            chunckRB.AddForce(Random.insideUnitCircle.normalized * Random.Range(explosionForce * 0.5f, explosionForce), ForceMode2D.Impulse);
        }
    }

    public void ForceExplode(int scoreReward, bool spawnFX = true)
    {
        var gameManagerInstance = GameManagerController.gameManagerInstance;
        gameManagerInstance.AddScore(scoreReward);

        if (spawnFX && explosionFX)
        {
            Instantiate(explosionFX, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}
