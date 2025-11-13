using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public float asteroidVelocity = 3f; //initial velocity of asteroid

    [Header("Asteroid Health Settings")]
    public float healthMax = 5f; //maximum health of asteroid
    private float healthCurrent; //current health of asteroid

    [Header("Asteroid Damage Settings")]
    public float collisionDamage = 1f; //damage dealt to player ship on collision

    [Header("Asteroid Spawn Settings")]
    public int asteroidValue = 3; //value of asteroid for spawning purposes

    [Header("Asteroid Chunk Settings")]
    public GameObject[] asteroidChunks; //array of asteroid chunk prefabs

    [Header("Chunk Explosion Settings")]
    public GameObject explosionFX; //explosion effect prefab
    public int chunkMin = 0; //minimum number of chunks to spawn
    public int chunkMax = 5; //maximum number of chunks to spawn
    public float explosionDistance = 0.5f; //maximum distance chunks can spawn from asteroid center
    public float explosionForce = 10f; //force applied to chunks on spawn

    [Header("Highscore Settings")]
    public int scoreValue = 10; //score value awarded to player on asteroid destruction

    [Header("Private Settings")]
    private Rigidbody2D rbAsteroid; //reference to asteroid rigidbody
    private float asteroidRotationSpeed; //rotation speed of asteroid
    private SpaceshipController playerShip; //reference to player ship


    private void Awake()
    {
        healthCurrent = healthMax;
        rbAsteroid = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        rbAsteroid.AddForce(Random.insideUnitCircle * asteroidVelocity, ForceMode2D.Impulse); //random initial force
        asteroidRotationSpeed = Random.Range(-asteroidVelocity, asteroidVelocity); //random rotation speed
        playerShip = FindAnyObjectByType<SpaceshipController>(); //find player ship in scene
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * asteroidRotationSpeed * Time.deltaTime); //rotate asteroid
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        playerShip = collision.gameObject.gameObject.GetComponent<SpaceshipController>(); //check if player ship collided
        if (playerShip != null)
        {
            playerShip.TakeDamage(collisionDamage); //damage player ship
        }
    }

    public void TakeDamage(float damage)
    {
        healthCurrent -= damage; //reduce health
        if (healthCurrent <= 0)
        {
            Explode(); //explode asteroid
        }
    }

    private void Explode()
    {
        if (playerShip)
        {
            playerShip.score += scoreValue; //increase player score
        }

        int numChunks = Random.Range(chunkMin, chunkMax + 1);

        if (asteroidChunks != null && asteroidChunks.Length > 0)
        {
            for (int i = 0; i < numChunks; i++)
            {
                CreateAsteroidChunk(); //create asteroid chunks
            }
        }

        if (explosionFX)
        {
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
