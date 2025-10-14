using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float Damage = 1f;
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
        Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
