using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidSpawnController : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public GameObject[] asteroidArray; //array of asteroid prefabs to spawn


    [Header("Asteroid Spawn Settings")]
    public float spawnInterval = 3f; //time between spawn checks
    public float intitialForce = 100f; //initial force applied to spawned asteroids
    public int spawnValueThreshold = 10; //total asteroid value threshold to trigger new spawns
    public float spawnOffset = 1.2f; // distance from center of screen to spawn
    public float directionSkew = 2f; //amount to skew force direction
    private float spawnTimer = 0f; //timer to track spawn intervals


    [Header("Screen Shake Settings")]
    public CameraController cameraShake; // camera shake controller
    public float screenShakeSpawnMultiplier = 0.1f; // multiplier for screen shake intensity


    void Start()
    {
        cameraShake = Object.FindAnyObjectByType<CameraController>();
    }

    void Update()
    {
        HandleAsteroidSpawn();
    }

    public void HandleAsteroidSpawn()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer > spawnInterval)
        {
            spawnTimer = 0f;
            if (TotalAsteroidValue() < spawnValueThreshold)
            {
                SpawnNewAsteroid();
            }
        }
    }

    public int TotalAsteroidValue()
    {
        AsteroidController[] asteroids = FindObjectsByType<AsteroidController>(FindObjectsSortMode.None);

        int totalAsteroidValue = 0;
        foreach (AsteroidController asteroid in asteroids)
        {
            totalAsteroidValue += asteroid.asteroidSpawnValue;
        }

        return totalAsteroidValue;
    }

    public void SpawnNewAsteroid()
    {
        if (asteroidArray.Length == 0) 
            return;

        int randomAsteroidIndex = Random.Range(0, asteroidArray.Length);
        GameObject asteroidToSpawn = asteroidArray[randomAsteroidIndex];

        Vector3 asteroidSpawnPos = RandomSpawnPoint();
        GameObject asteroid = Instantiate(asteroidToSpawn, asteroidSpawnPos, Quaternion.identity);
        cameraShake.StartSceenShake(screenShakeSpawnMultiplier);

        Rigidbody2D asteroidRB = asteroid.GetComponent<Rigidbody2D>();
        if (asteroidRB)
        {
            Vector2 asteroidForceDirection = ForceDirection(asteroidSpawnPos) * intitialForce;
            asteroidRB.AddForce(asteroidForceDirection);
        }
    }

    public Vector3 RandomSpawnPoint()
    {
        Vector2 viewportPos = Random.insideUnitCircle.normalized;
        Vector2 randomOffset = (Vector2)transform.position + viewportPos * spawnOffset;
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(randomOffset);
        worldPos.z = transform.position.z;

        return worldPos;
    }

    public Vector2 ForceDirection(Vector2 asteroidSpawnPos)
    {
        Vector2 targetPos = Vector2.zero;
        Vector2 directionToPlayer = (targetPos - asteroidSpawnPos).normalized;
        Vector2 randomSkew = Random.insideUnitCircle * directionSkew;
        Vector2 endForceDirection = (directionToPlayer + randomSkew).normalized;

        return endForceDirection;
    }
}