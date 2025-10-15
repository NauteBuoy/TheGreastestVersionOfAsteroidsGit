using UnityEngine;

public class DestructionController : MonoBehaviour
{
    public float Lifetime = 5f;
    private float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
