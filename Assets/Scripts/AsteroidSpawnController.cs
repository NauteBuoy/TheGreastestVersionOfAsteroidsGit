using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidSpawnController : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public GameObject[] asteroidArray; //array of asteroid prefabs to spawn

    [Header("Asteroid Spawn Settings")]
    public float spawnInterval = 3f; //time between spawn checks
    public float intitialForce = 100f; //initial force applied to spawned asteroids
    public int spawnThreshold = 10; //total asteroid value threshold to trigger new spawns
    public float spawnOffset = 1.2f; // distance from center of screen to spawn
    public float directionSkew = 2f; //amount to skew force direction

    [Header("Private Settings")]
    private float spawnTimer = 0f; //timer to track spawn intervals
    private SpaceshipController playerShip;


    void Start()
    {
        playerShip = FindAnyObjectByType<SpaceshipController>();
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer > spawnInterval)
        {
            spawnTimer = 0f;
            if (TotalAsteroidValue() < spawnThreshold)
            {
                SpawnNewAsteroid();
            }
        }
    }

    public int TotalAsteroidValue()
    {
        //find all asteroids in the scene
        AsteroidController[] asteroids = FindObjectsByType<AsteroidController>(FindObjectsSortMode.None);
        int value = 0;
        foreach (AsteroidController asteroid in asteroids)
        {
            value += asteroid.asteroidValue;
        }

        return value;
    }

    public void SpawnNewAsteroid()
    {
        if (asteroidArray.Length == 0) 
            return;

        if (!playerShip)
            return;

        //select random asteroid prefab
        int randomAsteroidIndex = Random.Range(0, asteroidArray.Length);
        GameObject asteroidToSpawn = asteroidArray[randomAsteroidIndex];

        //calculate spawn position
        Vector3 asteroidSpawnPos = RandomSpawnPoint();
        GameObject asteroid = Instantiate(asteroidToSpawn, asteroidSpawnPos, Quaternion.identity);

        //apply initial force to asteroid
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
        Vector2 playerPos = playerShip.transform.position;
        Vector2 directionToPlayer = (playerPos - asteroidSpawnPos).normalized;
        Vector2 randomSkew = Random.insideUnitCircle * directionSkew;
        Vector2 endForceDirection = (directionToPlayer + randomSkew).normalized;
        return endForceDirection;
    }
}