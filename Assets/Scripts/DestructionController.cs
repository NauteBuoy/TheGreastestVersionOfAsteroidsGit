using UnityEngine;

public class DestructionController : MonoBehaviour
{
    public float lifetime = 5f;
    private float timer = 0f;

    void Start()
    {

    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
