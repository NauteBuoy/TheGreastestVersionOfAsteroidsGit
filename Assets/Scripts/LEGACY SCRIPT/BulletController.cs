using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int Damage = 1;
    public GameObject explosionFX;


    void Start()
    {

    }

    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AsteroidController asteroid = collision.gameObject.GetComponent<AsteroidController>();
        if (asteroid)
        {
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
