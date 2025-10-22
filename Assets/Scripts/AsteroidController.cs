using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public float healthMax = 5f;
    private float healthCurrent;
    public float collisionDamage = 1f;
    public float asteroidVelocity = 3f;

    [Header("Asteroid Chunks")]
    public GameObject[] asteroidChunks;

    [Header("Chunk Settings")]
    public int chunkMin = 0;
    public int chunkMax = 5;
    public float chunkDistance = 0.5f;
    public float chunkForce = 10f;

    [Header("Particle Settings")]
    public GameObject explosionFX;

    [Header("Private Settings")]
    private Rigidbody2D rb2d;

    private void Awake()
    {
        healthCurrent = healthMax;
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d.AddForce(new Vector2(Random.Range(-asteroidVelocity, asteroidVelocity), Random.Range(-asteroidVelocity, asteroidVelocity)), ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, Random.Range(-asteroidVelocity, asteroidVelocity)) * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        SpaceshipController spaceship = collision.gameObject.gameObject.GetComponent<SpaceshipController>();
        if (spaceship != null)
        {
            spaceship.TakeDamage(collisionDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        healthCurrent = healthCurrent - damage;
        if (healthCurrent <= 0)
        {
            BreakApart();
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    BreakApart();
    //    Destroy(gameObject);
    //}

    private void BreakApart()
    {
        int numChunks = Random.Range(chunkMin, chunkMax + 1);

        if (asteroidChunks != null && asteroidChunks.Length > 0)
        {
            for (int i = 0; i < numChunks; i++)
            {
                CreateAsteroidChunk();
            }
        }  

        Instantiate(explosionFX, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void CreateAsteroidChunk()
    {
        int randomIndex = Random.Range(0, asteroidChunks.Length);
        GameObject asteroidChunk = asteroidChunks[randomIndex];

        Vector2 spawnPos = transform.position;
        spawnPos.x += Random.Range(-chunkDistance, chunkDistance);
        spawnPos.y += Random.Range(-chunkDistance, chunkDistance);

        GameObject chunk = Instantiate(asteroidChunk, spawnPos, transform.rotation);

        Vector2 dir = (spawnPos - (Vector2)transform.position).normalized;

        Rigidbody2D rbChuck = chunk.GetComponent<Rigidbody2D>();
        rbChuck.AddForce(dir * chunkForce);
    }
}
