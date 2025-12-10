using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AsteroidController : MonoBehaviour
{
    [Header("Asteroid Spawn Settings")]
    public int asteroidSpawnValue = 1; //value of asteroid for spawning purposes


    [Header("Asteroid References")]
    private Rigidbody2D rbAsteroid; //reference to asteroid rigidbody
    private SpaceshipController playerShip; //reference to player ship


    [Header("Movement Settings")]
    public float rotationSkew = 3f; //initial velocity of asteroid
    private float rotationSpeed; //rotation speed of asteroid


    [Header("Hit Point Settings")]
    public float hitPointMax = 5f; //maximum health of asteroid
    private float hitPointCurrent; //current health of asteroid


    [Header("Damage Settings")]
    public float collisionDamage = 1f; //damage dealt to player ship on collision
    public GameObject collsionFX; // collision effect prefab


    [Header("Screen Shake Settings")]
    public CameraController cameraShake; // camera shake controller
    public float screenShakeMultiplier = 1f; // multiplier for screen shake intensity


    [Header("Collision Settings")]
    public float collisionHitTime;
    public float nextCollisionHitDuration = 0.1f;


    [Header("Chunk Settings")]
    public GameObject[] asteroidChunks; //array of asteroid chunk prefabs
    public GameObject explosionFX; //explosion effect prefab
    public int chunkMin = 0; //minimum number of chunks to spawn
    public int chunkMax = 5; //maximum number of chunks to spawn
    public float explosionDistance = 0.5f; //maximum distance chunks can spawn from asteroid center
    public float explosionForce = 10f; //force applied to chunks on spawn


    [Header("Highscore Settings")]
    public int scoreValue = 10; //score value awarded to player on asteroid destruction


    void Start()
    {
        playerShip = SpaceshipController.playerShipInstance;
        cameraShake = Object.FindAnyObjectByType<CameraController>();
        rbAsteroid = GetComponent<Rigidbody2D>();
        hitPointCurrent = hitPointMax;
        rotationSpeed = Random.Range(-rotationSkew, rotationSkew); 
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime); 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!playerShip)
            return;

        else if (collision.gameObject.GetComponent<SpaceshipController>())
        {
            playerShip.TakeDamage(collisionDamage);
        }
        else if (collision.gameObject.GetComponent<AsteroidController>())
        {
            if (Time.time - collisionHitTime < nextCollisionHitDuration)
                return;

            AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.collisionSFX, AudioManagerController.Instance.collisionVolume);
            Instantiate(collsionFX, transform.position, transform.rotation);
            cameraShake.StartSceenShake(screenShakeMultiplier * 0.6f);
            
        }
    }


    public void TakeDamage(float damage)
    {
        AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.collisionSFX, AudioManagerController.Instance.normalCollisionVolume);

        hitPointCurrent -= damage; 
        cameraShake.StartSceenShake(screenShakeMultiplier);

        if (hitPointCurrent <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (playerShip)
        {
            playerShip.score += scoreValue;  
            ScoreUIController.scoreUIInstance.UpdateScore(playerShip.score);
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
            AudioManagerController.Instance.PlaySFX(AudioManagerController.Instance.explosionSFX, AudioManagerController.Instance.normalCollisionVolume);
            Instantiate(explosionFX, transform.position, transform.rotation); //create explosion effect
        }

        Destroy(gameObject); //destroy asteroid
    }

    private void CreateAsteroidChunk()
    {
        //get random chunk prefab
        int randomIndex = Random.Range(0, asteroidChunks.Length); 
        GameObject asteroidChunk = asteroidChunks[randomIndex];

        //randomize spawn position
        Vector2 spawnPos = transform.position;
        spawnPos.x += Random.Range(-explosionDistance, explosionDistance); 
        spawnPos.y += Random.Range(-explosionDistance, explosionDistance);

        GameObject chunk = Instantiate(asteroidChunk, spawnPos, transform.rotation); //instantiate chunk

        Vector2 randomDirection = Random.insideUnitCircle.normalized; //random direction for explosion force
        Rigidbody2D chunckRB = chunk.GetComponent<Rigidbody2D>(); //get chunk rigidbody
        if (chunckRB)
        {
            chunckRB.AddForce(randomDirection * Random.Range(explosionForce * 0.5f, explosionForce), ForceMode2D.Impulse); //apply explosion force
        }
    }
}
