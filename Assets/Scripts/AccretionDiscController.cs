using UnityEngine;

public class AccretionDiscController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer discRenderer; //accretion disc sprite
    public Transform playerTransform; //player transform


    [Header("Scale / Radius")]
    public float minScale = 0.2f;        // scale at 0 heat
    public float maxScale = 1.8f;        // scale at full heat
    public float minRadius = 0.5f;       // radius at 0 heat (used by gameplay)
    public float maxRadius = 6f;         // radius at full heat (used by gameplay)


    [Header("Color / Glow")]
    public Color coolColor = Color.white;
    public Color warmColor = new Color(1f, 0.65f, 0.25f); // orange
    public Color criticalColor = Color.cyan;              // overdrive/critical color
    [Range(0f, 1f)] public float criticalThreshold = 0.85f;


    [Header("Animation")]
    public float scaleLerp = 12f;       // how snappy scale follows heat
    public float colorLerp = 8f;        // how snappy color follows heat
    public float pulseStrength = 0.06f; // slight pulse at high heat
    public float pulseSpeed = 8f;


    float currentHeatNorm = 0f;



    void Start()
    {
        if (!playerTransform && transform.parent != null)
        {
            playerTransform = transform.parent;
        }
            
        if (!discRenderer)
        {
            discRenderer = GetComponent<SpriteRenderer>();
        }
            
    }

    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (playerTransform)
        {
            transform.position = playerTransform.position;
            transform.rotation = Quaternion.identity; // keep upright / no rotation
        }

        float baseScale = Mathf.Lerp(minScale, maxScale, currentHeatNorm);

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseStrength * (Mathf.Max(0f, currentHeatNorm - 0.5f) * 2f);
        float finalScale = baseScale + pulse;
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * finalScale, Time.deltaTime * scaleLerp);

        // smooth color
        if (discRenderer)
        {
            Color target;
            if (currentHeatNorm >= criticalThreshold)
            {
                // blend from warm -> critical when heat is high
                float t = Mathf.InverseLerp(criticalThreshold, 1f, currentHeatNorm);
                target = Color.Lerp(warmColor, criticalColor, t);
            }
            else
            {
                target = Color.Lerp(coolColor, warmColor, currentHeatNorm);
            }

            discRenderer.color = Color.Lerp(discRenderer.color, target, Time.deltaTime * colorLerp);
        }
    }

    public void SetHeatNormalized(float heatNorm)
    {
        currentHeatNorm = Mathf.Clamp01(heatNorm);
    }

    public float GetCurrentRadius()
    {
        return Mathf.Lerp(minRadius, maxRadius, currentHeatNorm);
    }

    public float GetCurrentVisualScale()
    {
        return Mathf.Lerp(minScale, maxScale, currentHeatNorm);
    }
}
