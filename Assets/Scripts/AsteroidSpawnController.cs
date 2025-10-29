using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidSpawnController : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public GameObject[] asteroidArray;
    public float spawnInterval = 3f;
    public float intitialForce = 100f;
    public int spawnThreshold = 10;
    public float spawnDistance = 2f;
    public float directionSkew = 2f;

    [Header("Private Settings")]
    private float spawnTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
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
        int randomAsteroidIndex = Random.Range(0, asteroidArray.Length);
        GameObject asteroidToSpawn = asteroidArray[randomAsteroidIndex];

        Vector3 asteroidSpawnPos = RandomSpawnPoint();
        GameObject asteroid = Instantiate(asteroidToSpawn, asteroidSpawnPos, transform.rotation);

        Vector2 asteroidForce = PushDirection(asteroidSpawnPos) * intitialForce;
        Rigidbody2D rb = asteroid.GetComponent<Rigidbody2D>();
        rb.AddForce(asteroidForce);
    }

    public Vector3 RandomSpawnPoint()
    {
        Vector2 randomPos = Random.insideUnitCircle;
        Vector2 direction = randomPos.normalized;
        Vector2 finalPos = (Vector2)transform.position + direction * spawnDistance;
        Vector3 result = Camera.main.ViewportToWorldPoint(finalPos);
        result.z = transform.position.z; // ensure same z-depth!
        return result;
    }

    public Vector2 PushDirection(Vector2 randomPosition)
    {
        Vector2 randomSkew = Random.insideUnitCircle * directionSkew;
        Vector2 destination = (Vector2)transform.position + randomSkew;
        Vector2 direction = (destination - randomPosition).normalized;
        return direction;
    }
}