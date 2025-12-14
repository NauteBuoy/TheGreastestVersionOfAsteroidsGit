using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ScorePopupController : MonoBehaviour
{
    public TextMeshPro scorePopupText; // Reference to the TextMeshPro component for displaying score
    public float lifetime = 0.8f; // Duration the popup stays on screen
    public float riseSpeed = 1.2f; // Speed at which the popup rises
    public float punchScale = 1.4f; // Scale factor for punch effect

    Vector3 textVelocity; // Velocity for text movement
    Vector3 baseScale; // Original scale of the text

    void Start()
    {
        scorePopupText.text = "+10";
        baseScale = transform.localScale;
        transform.localScale = baseScale * punchScale;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 12f);
    }

    public void SetText(string score)
    {
        if (scorePopupText)
        {
            scorePopupText.text = score;
        }
    }

}

