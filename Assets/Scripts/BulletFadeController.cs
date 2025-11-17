using UnityEngine;

public class BulletFadeController : MonoBehaviour
{
    public float lifeTime = 0.6f;
    public float fadeTime = 0.2f;

    private SpriteRenderer bulletSprite;
    private float fadeTimer;

    void Start()
    {
        bulletSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        fadeTimer += Time.deltaTime;

        if (fadeTimer >= lifeTime - fadeTime)
        {
            float elapsedTime = 1f - ((fadeTimer - (lifeTime - fadeTime)) / fadeTime);
            Color bulletColour = bulletSprite.color;
            bulletColour.a = elapsedTime;
            bulletSprite.color = bulletColour;
        }

        if (fadeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
