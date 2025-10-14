using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    [Header("Asteroid Settings")]
    public float healthMax = 3f;
    private float healthCurrent;
    public float collisionDamage = 1f;
    public GameObject[] asteroids;
    public float asteroidVelocity = 3f;

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
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BreakApart();
        Destroy(gameObject);
    }

    private void BreakApart()
    {
        foreach (GameObject i in asteroids)
        {
            Instantiate(i, transform.position, transform.rotation);
        }

    }
}
