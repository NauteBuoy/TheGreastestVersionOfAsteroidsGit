using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlashController : MonoBehaviour
{
    public float flashDuration = 0.33f;
    private Image flashImage;
    private Color flashColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flashImage = GetComponent<Image>();
        flashColor = flashImage.color;
    }
    public IEnumerator FlashRoutine()
    {
        float timer = 0f;
        float t = 0f;
        float alphaFrom = 1f; // fully opaque
        float alphaTo = 0f; // fully transparent

        while (t < 1f) // repeats while condition is true
        {
            timer += Time.deltaTime;
            t = Mathf.Clamp01(timer / flashDuration);
            float alpha = Mathf.Lerp(alphaFrom, alphaTo, t);
            Color col = flashColor;
            col.a = alpha;
            flashImage.color = col;
            yield return new WaitForEndOfFrame();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
