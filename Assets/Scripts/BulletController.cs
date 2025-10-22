using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float Damage = 1f;
    public GameObject explosionFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AsteroidController asteroid = collision.gameObject.GetComponent<AsteroidController>();
        if (asteroid)
        {
            Instantiate(explosionFX, transform.position, Quaternion.identity);
            asteroid.TakeDamage(Damage);
            Explode();
        }
    }
    private void Explode()
    {
        Instantiate(explosionFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


}
